using System.Linq;
using System.Windows.Controls;
using GenshinLyreMidiPlayer.Data;
using GenshinLyreMidiPlayer.WPF.Views;
using JetBrains.Annotations;
using ModernWpf;
using Stylet;
using StyletIoC;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using AutoSuggestBox = Wpf.Ui.Controls.AutoSuggestBox;

namespace GenshinLyreMidiPlayer.WPF.ViewModels;

[UsedImplicitly]
public class MainWindowViewModel : Conductor<IScreen>
{
    public static NavigationStore Navigation = null!;
    private readonly IContainer _ioc;
    private readonly IThemeService _theme;

    public MainWindowViewModel(IContainer ioc, IThemeService theme)
    {
        Title = $"Genshin Lyre MIDI Player {SettingsPageViewModel.ProgramVersion}";

        _ioc   = ioc;
        _theme = theme;

        PlaylistView   = new(ioc, this);
        SettingsView   = new(ioc, this);
        PianoSheetView = new(this);

        ActiveItem = PlayerView = new(ioc, this);
    }

    public bool ShowUpdate => SettingsView.NeedsUpdate && ActiveItem != SettingsView;

    public LyrePlayerViewModel PlayerView { get; }

    public PianoSheetViewModel PianoSheetView { get; }

    public PlaylistViewModel PlaylistView { get; }

    public SettingsPageViewModel SettingsView { get; }

    public string Title { get; set; }

    public void Navigate(INavigation sender, RoutedNavigationEventArgs args)
    {
        if ((args.CurrentPage as NavigationItem)?.Tag is IScreen viewModel)
            ActivateItem(viewModel);

        NotifyOfPropertyChange(() => ShowUpdate);
    }

    public void NavigateToSettings() => ActivateItem(SettingsView);

    public void ToggleTheme()
    {
        ThemeManager.Current.ApplicationTheme = _theme.GetTheme() switch
        {
            ThemeType.Unknown      => ApplicationTheme.Dark,
            ThemeType.Dark         => ApplicationTheme.Light,
            ThemeType.Light        => ApplicationTheme.Dark,
            ThemeType.HighContrast => ApplicationTheme.Dark,
            _                      => ApplicationTheme.Dark
        };

        SettingsView.OnThemeChanged();
    }

    public void SearchSong(AutoSuggestBox sender, TextChangedEventArgs e)
    {
        if (ActiveItem != PlaylistView)
        {
            ActivateItem(PlaylistView);

            var playlist = Navigation.Items
                .OfType<NavigationItem>()
                .First(nav => nav.Tag == PlaylistView);
            var index = Navigation.Items.IndexOf(playlist);
            Navigation.SelectedPageIndex = index;
        }

        PlaylistView.FilterText = sender.Text;
    }

    protected override async void OnViewLoaded()
    {
        Navigation = ((MainWindowView) View).RootNavigation;
        SettingsView.OnThemeChanged();

        if (!await SettingsView.TryGetLocationAsync()) _ = SettingsView.LocationMissing();
        if (SettingsView.AutoCheckUpdates)
        {
            _ = SettingsView.CheckForUpdate()
                .ContinueWith(_ => { NotifyOfPropertyChange(() => ShowUpdate); });
        }

        await using var db = _ioc.Get<LyreContext>();
        await PlaylistView.AddFiles(db.History);
    }
}