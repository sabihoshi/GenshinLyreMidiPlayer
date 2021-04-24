using System.Collections.Generic;
using System.Linq;
using GenshinLyreMidiPlayer.Data;
using GenshinLyreMidiPlayer.WPF.Views;
using ModernWpf.Controls;
using Stylet;
using StyletIoC;

namespace GenshinLyreMidiPlayer.WPF.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.StackNavigation
    {
        private readonly Stack<NavigationViewItem> _history = new();
        private readonly IContainer _ioc;
        private NavigationView _navView;

        public MainWindowViewModel(IContainer ioc, IEventAggregator events)
        {
            _ioc = ioc;

            SettingsView = ioc.Get<SettingsPageViewModel>();
            PlaylistView = ioc.Get<PlaylistViewModel>();
            PlayerView   = new(ioc, SettingsView, PlaylistView);
        }

        public bool ShowUpdate => SettingsView.NeedsUpdate && ActiveItem != SettingsView;

        public LyrePlayerViewModel PlayerView { get; }

        public PlaylistViewModel PlaylistView { get; }

        public SettingsPageViewModel SettingsView { get; }

        public string Title { get; set; } = "Genshin Lyre MIDI Player";

        protected override async void OnViewLoaded()
        {
            // Work around because events do not conform to the signatures Stylet supports
            _navView = ((MainWindowView) View).NavView;

            _navView.SelectionChanged += Navigate;
            _navView.BackRequested    += NavigateBack;

            var menuItems = _navView.MenuItems.Cast<NavigationViewItemBase>();
            _navView.SelectedItem = menuItems.FirstOrDefault(item => item is NavigationViewItem);

            if (SettingsView.AutoCheckUpdates)
            {
                _ = SettingsView.CheckForUpdate()
                    .ContinueWith(_ => NotifyOfPropertyChange(() => ShowUpdate));
            }

            await using var db = _ioc.Get<LyreContext>();
            await PlaylistView.AddFiles(db.History.Select(midi => midi.Path));
        }

        private void NavigateBack(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            GoBack();

            // Work around to select the navigation item that this IScreen is a part of
            _history.Pop();
            sender.SelectedItem  = _history.Pop();
            sender.IsBackEnabled = _history.Count > 1;
        }

        private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
                NavigateToSettings();
            else if ((args.SelectedItem as NavigationViewItem)?.Tag is IScreen viewModel)
            {
                ActivateItem(viewModel);
                _history.Push((NavigationViewItem) sender.SelectedItem);
            }

            sender.IsBackEnabled = _history.Count > 1;
            NotifyOfPropertyChange(() => ShowUpdate);
        }

        public void NavigateToSettings()
        {
            ActivateItem(SettingsView);
            _history.Push((NavigationViewItem) _navView.SettingsItem);
            _navView.SelectedItem = _navView.SettingsItem;
        }
    }
}