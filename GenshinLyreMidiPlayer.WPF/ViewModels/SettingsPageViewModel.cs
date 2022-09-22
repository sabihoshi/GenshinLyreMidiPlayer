using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Data;
using GenshinLyreMidiPlayer.Data.Entities;
using GenshinLyreMidiPlayer.Data.Git;
using GenshinLyreMidiPlayer.Data.Midi;
using GenshinLyreMidiPlayer.Data.Notification;
using GenshinLyreMidiPlayer.Data.Properties;
using GenshinLyreMidiPlayer.WPF.Core;
using GenshinLyreMidiPlayer.WPF.ModernWPF;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Animation;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Animation.Transitions;
using JetBrains.Annotations;
using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Stylet;
using StyletIoC;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;
using static GenshinLyreMidiPlayer.Data.Entities.Transpose;

namespace GenshinLyreMidiPlayer.WPF.ViewModels;

public class SettingsPageViewModel : Screen
{
    public static readonly Dictionary<Transpose, string> TransposeNames = new()
    {
        [Ignore] = "Ignore notes",
        [Up]     = "Shift one semitone up",
        [Down]   = "Shift one semitone down"
    };

    private static readonly Settings Settings = Settings.Default;
    private readonly IContainer _ioc;
    private readonly IEventAggregator _events;
    private readonly IThemeService _theme;
    private readonly MainWindowViewModel _main;
    private int _keyOffset;

    public SettingsPageViewModel(IContainer ioc, MainWindowViewModel main)
    {
        _ioc    = ioc;
        _events = ioc.Get<IEventAggregator>();
        _theme  = ioc.Get<IThemeService>();
        _main   = main;

        _keyOffset = Playlist.OpenedFile?.History.Key ?? 0;

        ThemeManager.Current.ApplicationTheme = Settings.AppTheme switch
        {
            0 => ApplicationTheme.Light,
            1 => ApplicationTheme.Dark,
            _ => null
        };
    }

    public bool AutoCheckUpdates { get; set; } = Settings.AutoCheckUpdates;

    public bool CanChangeTime => PlayTimerToken is null;

    public bool CanStartStopTimer => DateTime - DateTime.Now > TimeSpan.Zero;

    public bool CanUseSpeakers { get; set; } = true;

    public bool IncludeBetaUpdates { get; set; } = Settings.IncludeBetaUpdates;

    public bool IsCheckingUpdate { get; set; }

    public bool MergeNotes { get; set; } = Settings.MergeNotes;

    public bool NeedsUpdate => ProgramVersion < LatestVersion.Version;

    [UsedImplicitly] public CancellationTokenSource? PlayTimerToken { get; private set; }

    public static CaptionedObject<Transition>? Transition { get; set; } =
        TransitionCollection.Transitions[Settings.SelectedTransition];

    public DateTime DateTime { get; set; } = DateTime.Now;

    [UsedImplicitly]
    public Dictionary<int, string> KeyOffsets { get; set; } = new()
    {
        [-27] = "A0",
        [-26] = "A♯0",
        [-25] = "B0",
        [-24] = "C1",
        [-23] = "C♯1",
        [-22] = "D1",
        [-21] = "D♯1",
        [-20] = "E1",
        [-19] = "F1",
        [-18] = "F♯1",
        [-17] = "G1",
        [-16] = "G♯1",
        [-15] = "A1",
        [-14] = "A♯1",
        [-13] = "B1",
        [-12] = "C2",
        [-11] = "C♯2",
        [-10] = "D2",
        [-9]  = "D♯2",
        [-8]  = "E2",
        [-7]  = "F2",
        [-6]  = "F♯2",
        [-5]  = "G2",
        [-4]  = "G♯2",
        [-3]  = "A2",
        [-2]  = "A♯2",
        [-1]  = "B2",
        [0]   = "C3",
        [1]   = "C♯3",
        [2]   = "D3",
        [3]   = "D♯3",
        [4]   = "E3",
        [5]   = "F3",
        [6]   = "F♯3",
        [7]   = "G3",
        [8]   = "G♯3",
        [9]   = "A3",
        [10]  = "A♯3",
        [11]  = "B3",
        [12]  = "C4 Middle C",
        [13]  = "C♯4",
        [14]  = "D4",
        [15]  = "D♯4",
        [16]  = "E4",
        [17]  = "F4",
        [18]  = "F♯4",
        [19]  = "G4",
        [20]  = "G♯4",
        [21]  = "A4 Concert Pitch",
        [22]  = "A♯4",
        [23]  = "B4",
        [24]  = "C5"
    };

    public GitVersion LatestVersion { get; set; } = new();

    public int KeyOffset
    {
        get => _keyOffset;
        set => SetAndNotify(ref _keyOffset, Math.Clamp(value, MinOffset, MaxOffset));
    }

    public int MaxOffset => KeyOffsets.Keys.Max();

    public int MinOffset => KeyOffsets.Keys.Min();

    public KeyValuePair<Keyboard.Instrument, string> SelectedInstrument { get; set; }

    public KeyValuePair<Keyboard.Layout, string> SelectedLayout { get; set; }

    public KeyValuePair<Transpose, string>? Transpose { get; set; }

    public static List<MidiSpeed> MidiSpeeds { get; } = new()
    {
        new("0.25x", 0.25),
        new("0.5x", 0.5),
        new("0.75x", 0.75),
        new("Normal", 1),
        new("1.25x", 1.25),
        new("1.5x", 1.5),
        new("1.75x", 1.75),
        new("2x", 2)
    };

    public MidiSpeed SelectedSpeed { get; set; } = MidiSpeeds[Settings.SelectedSpeed];

    public static string GenshinLocation
    {
        get => Settings.GenshinLocation;
        set => Settings.GenshinLocation = value;
    }

    public string Key => $"Key: {KeyOffsets[KeyOffset]}";

    public string TimerText => CanChangeTime ? "Start" : "Stop";

    [UsedImplicitly] public string UpdateString { get; set; } = string.Empty;

    public uint MergeMilliseconds { get; set; } = Settings.MergeMilliseconds;

    public static Version ProgramVersion => Assembly.GetExecutingAssembly().GetName().Version!;

    private PlaylistViewModel Playlist => _main.PlaylistView;

    public async Task<bool> TryGetLocationAsync()
    {
        var locations = new[]
        {
            // User set location
            Settings.GenshinLocation,

            // Default install location
            @"C:\Program Files\Genshin Impact\Genshin Impact Game\GenshinImpact.exe",
            @"C:\Program Files\Genshin Impact\Genshin Impact Game\YuanShen.exe",

            // Custom install location
            Path.Combine(WindowHelper.InstallLocation ?? string.Empty, @"Genshin Impact Game\GenshinImpact.exe"),
            Path.Combine(WindowHelper.InstallLocation ?? string.Empty, @"Genshin Impact Game\YuanShen.exe"),

            // Relative location
            AppContext.BaseDirectory + "GenshinImpact.exe",
            AppContext.BaseDirectory + "YuanShen.exe"
        };

        foreach (var location in locations)
        {
            if (await TrySetLocationAsync(location))
                return true;
        }

        return false;
    }

    public async Task CheckForUpdate()
    {
        if (IsCheckingUpdate)
            return;

        UpdateString     = "Checking for updates...";
        IsCheckingUpdate = true;

        try
        {
            LatestVersion = await GetLatestVersion() ?? new GitVersion();
            UpdateString = LatestVersion.Version > ProgramVersion
                ? "(Update available!)"
                : string.Empty;
        }
        catch (Exception)
        {
            UpdateString = "Failed to check updates";
        }
        finally
        {
            IsCheckingUpdate = false;
            NotifyOfPropertyChange(() => NeedsUpdate);
        }
    }

    public async Task LocationMissing()
    {
        var dialog = new ContentDialog
        {
            Title   = "Error",
            Content = "Could not find Game's Location, please find GenshinImpact.exe or YuanShen.exe",

            PrimaryButtonText   = "Find Manually...",
            SecondaryButtonText = "Ignore (Notes might not play)",
            CloseButtonText     = "Exit"
        };

        var result = await dialog.ShowAsync();

        switch (result)
        {
            case ContentDialogResult.None:
                RequestClose();
                break;
            case ContentDialogResult.Primary:
                await SetLocation();
                break;
            case ContentDialogResult.Secondary:
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(result), result, $"Invalid {nameof(ContentDialogResult)}");
        }
    }

    [PublicAPI]
    public async Task SetLocation()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Executable|*.exe|All files (*.*)|*.*",
            InitialDirectory = WindowHelper.InstallLocation is null
                ? @"C:\Program Files\Genshin Impact\Genshin Impact Game\"
                : Path.Combine(WindowHelper.InstallLocation, "Genshin Impact Game")
        };

        var success = openFileDialog.ShowDialog() == true;
        var set = await TrySetLocationAsync(openFileDialog.FileName);

        if (!(success && set)) await LocationMissing();
    }

    [UsedImplicitly]
    public async Task StartStopTimer()
    {
        if (PlayTimerToken is not null)
        {
            PlayTimerToken.Cancel();
            return;
        }

        PlayTimerToken = new();

        var start = DateTime - DateTime.Now;
        await Task.Delay(start, PlayTimerToken.Token)
            .ContinueWith(_ => { });

        if (!PlayTimerToken.IsCancellationRequested)
            _events.Publish(new PlayTimerNotification());

        PlayTimerToken = null;
    }

    [UsedImplicitly]
    public void OnThemeChanged()
    {
        _theme.SetTheme(ThemeManager.Current.ApplicationTheme switch
        {
            ApplicationTheme.Light => ThemeType.Light,
            ApplicationTheme.Dark  => ThemeType.Dark,
            _                      => _theme.GetSystemTheme()
        });

        Settings.Modify(s => s.AppTheme = (int?) ThemeManager.Current.ApplicationTheme ?? -1);
    }

    [UsedImplicitly]
    public void SetTimeToNow() => DateTime = DateTime.Now;

    protected override void OnActivate()
    {
        if (AutoCheckUpdates)
            _ = CheckForUpdate();
    }

    private async Task<bool> TrySetLocationAsync(string? location)
    {
        if (!File.Exists(location)) return false;
        if (Path.GetFileName(location).Equals("launcher.exe", StringComparison.OrdinalIgnoreCase))
        {
            var dialog = new ContentDialog
            {
                Title   = "Incorrect Location",
                Content = "launcher.exe is not the game, please find GenshinImpact.exe",

                CloseButtonText = "Ok"
            };

            await dialog.ShowAsync();
            return false;
        }

        Settings.GenshinLocation = location;
        NotifyOfPropertyChange(() => Settings.GenshinLocation);

        return true;
    }

    private async Task<GitVersion?> GetLatestVersion()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://api.github.com/repos/sabihoshi/GenshinLyreMidiPlayer/releases");

        var productInfo = new ProductInfoHeaderValue("GenshinLyreMidiPlayer", ProgramVersion.ToString());
        request.Headers.UserAgent.Add(productInfo);

        var response = await client.SendAsync(request);
        var versions = await response.Content.ReadFromJsonAsync<List<GitVersion>>();

        return versions?
            .OrderByDescending(v => v.Version)
            .FirstOrDefault(v => (!v.Draft && !v.Prerelease) || IncludeBetaUpdates);
    }

    [UsedImplicitly]
    private void OnAutoCheckUpdatesChanged()
    {
        if (AutoCheckUpdates)
            _ = CheckForUpdate();

        Settings.Modify(s => s.AutoCheckUpdates = AutoCheckUpdates);
    }

    [UsedImplicitly]
    private void OnIncludeBetaUpdatesChanged() => _ = CheckForUpdate();

    [UsedImplicitly]
    private async void OnKeyOffsetChanged()
    {
        if (Playlist.OpenedFile is null)
            return;

        await using var db = _ioc.Get<LyreContext>();

        Playlist.OpenedFile.History.Key = KeyOffset;
        db.Update(Playlist.OpenedFile.History);

        await db.SaveChangesAsync();
    }

    [UsedImplicitly]
    private void OnMergeMillisecondsChanged()
    {
        Settings.Modify(s => s.MergeMilliseconds = MergeMilliseconds);
        _events.Publish(this);
    }

    [UsedImplicitly]
    private void OnMergeNotesChanged()
    {
        Settings.Modify(s => s.MergeNotes = MergeNotes);
        _events.Publish(new MergeNotesNotification(MergeNotes));
    }

    [UsedImplicitly]
    private void OnSelectedInstrumentIndexChanged()
    {
        var instrument = (int) SelectedInstrument.Key;
        Settings.Modify(s => s.SelectedInstrument = instrument);
    }

    [UsedImplicitly]
    private void OnSelectedLayoutIndexChanged()
    {
        var layout = (int) SelectedLayout.Key;
        Settings.Modify(s => s.SelectedLayout = layout);
    }

    [UsedImplicitly]
    private void OnSelectedSpeedChanged() => _events.Publish(this);

    [UsedImplicitly]
    private async void OnTransposeChanged()
    {
        if (Playlist.OpenedFile is null)
            return;

        await using var db = _ioc.Get<LyreContext>();

        Playlist.OpenedFile.History.Transpose = Transpose?.Key;
        db.Update(Playlist.OpenedFile.History);

        await db.SaveChangesAsync();
    }
}