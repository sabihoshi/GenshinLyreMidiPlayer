using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Views;
using ModernWpf.Controls;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.StackNavigation
    {
        private readonly Stack<NavigationViewItem> _history = new();
        private NavigationView _navView;

        public MainWindowViewModel(IEventAggregator events)
        {
            SettingsView = new SettingsPageViewModel(events);
            PlaylistView = new PlaylistViewModel(events);
            PlayerView   = new LyrePlayerViewModel(events, SettingsView, PlaylistView);

            if (SettingsView.AutoCheckUpdates)
                Task.Run(async () =>
                {
                    await SettingsView.CheckForUpdate();
                    NotifyOfPropertyChange(() => ShowUpdate);
                });
        }

        public bool ShowUpdate => SettingsView.NeedsUpdate && ActiveItem != SettingsView;

        public LyrePlayerViewModel PlayerView { get; }

        public PlaylistViewModel PlaylistView { get; }

        public SettingsPageViewModel SettingsView { get; }

        public string Title { get; set; } = "Genshin Lyre MIDI Player";

        protected override void OnViewLoaded()
        {
            // Work around because events do not conform to the signatures Stylet supports
            _navView = ((MainWindowView) View).NavView;

            _navView.SelectionChanged += Navigate;
            _navView.BackRequested    += NavigateBack;

            var menuItems = _navView.MenuItems.Cast<NavigationViewItemBase>();
            _navView.SelectedItem = menuItems.FirstOrDefault(item => item is NavigationViewItem);
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
            {
                NavigateToSettings();
            }
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