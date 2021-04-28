using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Data.Git;
using GenshinLyreMidiPlayer.Data.Midi;
using GenshinLyreMidiPlayer.Data.Notification;
using GenshinLyreMidiPlayer.Data.Properties;
using GenshinLyreMidiPlayer.WPF.Core;
using GenshinLyreMidiPlayer.WPF.ModernWPF;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Animation;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Animation.Transitions;
using ModernWpf;
using Stylet;
using StyletIoC;

namespace GenshinLyreMidiPlayer.WPF.ViewModels
{
    public class SettingsPageViewModel : Screen
    {
        private static readonly Settings Settings = Settings.Default;
        private readonly IEventAggregator _events;
        private int _keyOffset = Settings.KeyOffset;

        public SettingsPageViewModel(IContainer ioc)
        {
            _events = ioc.Get<IEventAggregator>();

            ThemeManager.Current.ApplicationTheme = Settings.AppTheme switch
            {
                0 => ApplicationTheme.Light,
                1 => ApplicationTheme.Dark,
                _ => null
            };
        }

        public bool AutoCheckUpdates { get; set; } = Settings.AutoCheckUpdates;

        public bool IncludeBetaUpdates { get; set; } = Settings.IncludeBetaUpdates;

        public bool IsCheckingUpdate { get; set; }

        public bool MergeNotes { get; set; } = Settings.MergeNotes;

        public bool NeedsUpdate => ProgramVersion < LatestVersion?.Version;

        public static CaptionedObject<Transition>? Transition { get; set; } =
            TransitionCollection.Transitions[Settings.SelectedTransition];

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

        public GitVersion? LatestVersion { get; set; }

        public int KeyOffset
        {
            get => _keyOffset;
            set => SetAndNotify(ref _keyOffset, Math.Clamp(value, MinOffset, MaxOffset));
        }

        public int MaxOffset => KeyOffsets.Keys.Max();

        public int MinOffset => KeyOffsets.Keys.Min();

        public KeyValuePair<Keyboard.Layout, string> SelectedLayout { get; set; }

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

        public string Key => $"Key: {KeyOffsets[KeyOffset]}";

        public string UpdateString { get; set; } = string.Empty;

        public uint MergeMilliseconds { get; set; } = Settings.MergeMilliseconds;

        public static Version ProgramVersion => Assembly.GetExecutingAssembly().GetName().Version!;

        private void OnAutoCheckUpdatesChanged()
        {
            if (AutoCheckUpdates)
                _ = CheckForUpdate();
        }

        private void OnIncludeBetaUpdatesChanged() => _ = CheckForUpdate();

        private void OnKeyOffsetChanged() => Settings.Modify(s => s.KeyOffset = KeyOffset);

        public async Task CheckForUpdate()
        {
            if (IsCheckingUpdate)
                return;

            UpdateString     = "Checking for updates...";
            IsCheckingUpdate = true;

            try
            {
                LatestVersion = await GetLatestVersion();
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

        public async Task<GitVersion> GetLatestVersion()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/sabihoshi/GenshinLyreMidiPlayer/releases");

            var productInfo = new ProductInfoHeaderValue("GenshinLyreMidiPlayer", ProgramVersion.ToString());

            request.Headers.UserAgent.Add(productInfo);

            var response = await client.SendAsync(request);
            var versions = JsonSerializer.Deserialize<List<GitVersion>>(await response.Content.ReadAsStringAsync());

            return versions
                .OrderByDescending(v => v.Version)
                .First(v => !v.Draft && !v.Prerelease || IncludeBetaUpdates);
        }

        private void OnSelectedLayoutIndexChanged()
        {
            var layout = (int) SelectedLayout.Key;
            Settings.Modify(s => s.SelectedLayout = layout);
        }

        private void OnMergeMillisecondsChanged()
        {
            Settings.Modify(s => s.MergeMilliseconds = MergeMilliseconds);
            _events.Publish(this);
        }

        private void OnMergeNotesChanged()
        {
            Settings.Modify(s => s.MergeNotes = MergeNotes);
            _events.Publish(new MergeNotesNotification(MergeNotes));
        }

        private void OnSelectedSpeedChanged() { _events.Publish(this); }

        public void OnThemeChanged()
        {
            var theme = (int?) ThemeManager.Current.ApplicationTheme ?? -1;
            Settings.Modify(s => s.AppTheme = theme);
        }

        protected override void OnActivate()
        {
            if (AutoCheckUpdates)
                _ = CheckForUpdate();
        }
    }
}