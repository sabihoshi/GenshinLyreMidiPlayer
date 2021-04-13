using System.Collections.Generic;
using System.Linq;
using GenshinLyreMidiPlayer.Views;
using ModernWpf.Controls;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.StackNavigation
    {
        private readonly Stack<NavigationViewItem> _history = new();

        public MainWindowViewModel(IEventAggregator events)
        {
            SettingsView = new SettingsPageViewModel(events);
            PlaylistView = new PlaylistViewModel(events);
            PlayerView   = new LyrePlayerViewModel(events, SettingsView, PlaylistView);

            if (SettingsView.AutoCheckUpdates)
                _ = SettingsView.CheckForUpdate();
        }

        public LyrePlayerViewModel PlayerView { get; }

        public PlaylistViewModel PlaylistView { get; }

        public SettingsPageViewModel SettingsView { get; }

        public string Title { get; set; } = "Genshin Lyre MIDI Player";

        protected override void OnViewLoaded()
        {
            // Work around because events do not conform to the signatures Stylet supports
            var navView = ((MainWindowView) View).NavView;
            navView.SelectionChanged += Navigate;
            navView.BackRequested    += NavigateBack;

            var menuItems = navView.MenuItems.Cast<NavigationViewItemBase>();
            navView.SelectedItem = menuItems.FirstOrDefault(item => item is NavigationViewItem);
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
                ActivateItem(SettingsView);
                _history.Push((NavigationViewItem) sender.SettingsItem);
            }
            else if ((args.SelectedItem as NavigationViewItem)?.Tag is IScreen viewModel)
            {
                ActivateItem(viewModel);
                _history.Push((NavigationViewItem) sender.SelectedItem);
            }

            sender.IsBackEnabled = _history.Count > 1;
        }
    }
}