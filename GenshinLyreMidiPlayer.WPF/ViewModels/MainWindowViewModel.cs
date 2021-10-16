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
        private readonly IContainer _ioc;
        private readonly Stack<NavigationViewItem> _history = new();
        private NavigationView _navView;

        public MainWindowViewModel(IContainer ioc, IEventAggregator events)
        {
            _ioc = ioc;

            SettingsView   = ioc.Get<SettingsPageViewModel>();
            PlaylistView   = ioc.Get<PlaylistViewModel>();
            PlayerView     = new(ioc, SettingsView, PlaylistView);
            PianoSheetView = new(SettingsView, PlaylistView);
        }

        public bool ShowUpdate => SettingsView.NeedsUpdate && ActiveItem != SettingsView;

        public LyrePlayerViewModel PlayerView { get; }

        public PianoSheetViewModel PianoSheetView { get; }

        public PlaylistViewModel PlaylistView { get; }

        public SettingsPageViewModel SettingsView { get; }

        public string Title { get; set; } = "Genshin Lyre MIDI Player";

        protected override async void OnViewLoaded()
        {
            // Work around because events do not conform to the signatures Stylet supports
            _navView = ((MainWindowView) View).NavView;

            _navView.AutoSuggestBox.TextChanged += AutoSuggestBoxOnTextChanged;

            _navView.SelectionChanged += Navigate;
            _navView.BackRequested    += NavigateBack;

            var menuItems = _navView.MenuItems.Cast<NavigationViewItemBase>();
            _navView.SelectedItem = menuItems.FirstOrDefault(item => item is NavigationViewItem);

            if (!SettingsView.TryGetLocation()) _ = SettingsView.LocationMissing();

            if (SettingsView.AutoCheckUpdates)
            {
                _ = SettingsView.CheckForUpdate()
                    .ContinueWith(_ => NotifyOfPropertyChange(() => ShowUpdate));
            }

            await using var db = _ioc.Get<LyreContext>();
            await PlaylistView.AddFiles(db.History.Select(midi => midi.Path));
        }

        private void AutoSuggestBoxOnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            PlaylistView.OnFilterTextChanged(sender, e);
            if (ActiveItem != PlaylistView)
            {
                var playlist = (NavigationViewItem) _navView.MenuItems
                    .Cast<NavigationViewItemBase>()
                    .First(nav => nav.Tag == PlaylistView);

                _navView.SelectedItem = playlist;
            }
        }

        private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
                Activate(SettingsView);
            else if ((args.SelectedItem as NavigationViewItem)?.Tag is IScreen viewModel)
                Activate(viewModel);

            sender.IsBackEnabled = _history.Count > 1;
            NotifyOfPropertyChange(() => ShowUpdate);

            void Activate(IScreen viewModel)
            {
                ActivateItem(viewModel);
                _history.Push((NavigationViewItem) sender.SelectedItem);
            }
        }

        private void NavigateBack(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            _history.Pop();
            sender.SelectedItem  = _history.Pop();
            sender.IsBackEnabled = _history.Count > 1;
        }
    }
}