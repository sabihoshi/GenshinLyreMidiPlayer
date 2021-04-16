using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Core;
using GenshinLyreMidiPlayer.Models;
using GenshinLyreMidiPlayer.ModernWPF;
using GenshinLyreMidiPlayer.ModernWPF.Animation;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class SettingsPageViewModel : Screen
    {
        private readonly IEventAggregator _events;
        private bool _autoCheckUpdates = true;
        private bool _includeBetaUpdates = true;
        private int _keyOffset;
        private uint _mergeMilliseconds;
        private bool _mergeNotes;
        private MidiSpeedModel _selectedSpeed;
        private bool _useSpeakers;

        public SettingsPageViewModel(IEventAggregator events)
        {
            _events     = events;
            Transitions = new TransitionCollection();
            Transition  = Transitions.First();

            SelectedLayout = Keyboard.LayoutNames.First();
            SelectedSpeed  = MidiSpeeds[3];
        }

        public bool UseSpeakers
        {
            get => _useSpeakers;
            set
            {
                SetAndNotify(ref _useSpeakers, value);
                _events.Publish(this);
            }
        }

        public bool HoldNotes { get; set; }

        public bool TransposeNotes { get; set; } = true;

        public bool MergeNotes
        {
            get => _mergeNotes;
            set
            {
                SetAndNotify(ref _mergeNotes, value);
                _events.Publish(this);
            }
        }

        public bool IncludeBetaUpdates
        {
            get => _includeBetaUpdates;
            set
            {
                SetAndNotify(ref _includeBetaUpdates, value);

                _ = CheckForUpdate();
            }
        }

        public bool IsCheckingUpdate { get; set; }

        public bool AutoCheckUpdates
        {
            get => _autoCheckUpdates;
            set
            {
                SetAndNotify(ref _autoCheckUpdates, value);

                if (_autoCheckUpdates)
                    _ = CheckForUpdate();
            }
        }

        public static CaptionedObject<Transition>? Transition { get; set; }

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

        public IEnumerable<CaptionedObject<Transition>> Transitions { get; }

        public int MinOffset => KeyOffsets.Keys.Min();

        public int MaxOffset => KeyOffsets.Keys.Max();

        public int KeyOffset
        {
            get => _keyOffset;
            set => SetAndNotify(ref _keyOffset, Math.Clamp(value, MinOffset, MaxOffset));
        }

        public KeyValuePair<Keyboard.Layout, string> SelectedLayout { get; set; }

        public List<MidiSpeedModel> MidiSpeeds { get; } = new()
        {
            new MidiSpeedModel("0.25x", 0.25),
            new MidiSpeedModel("0.5x", 0.5),
            new MidiSpeedModel("0.75x", 0.75),
            new MidiSpeedModel("Normal", 1),
            new MidiSpeedModel("1.25x", 1.25),
            new MidiSpeedModel("1.5x", 1.5),
            new MidiSpeedModel("1.75x", 1.75),
            new MidiSpeedModel("2x", 2)
        };

        public MidiSpeedModel SelectedSpeed
        {
            get => _selectedSpeed;
            set
            {
                SetAndNotify(ref _selectedSpeed, value);
                _events.Publish(this);
            }
        }

        public string Key => $"Key: {KeyOffsets[KeyOffset]}";

        public static string VersionString { get; } = $"{ProgramVersion()?.ToString(3)}";

        public string UpdateString { get; set; } = string.Empty;

        public uint MergeMilliseconds
        {
            get => _mergeMilliseconds;
            set
            {
                SetAndNotify(ref _mergeMilliseconds, value);
                _events.Publish(this);
            }
        }

        protected override void OnActivate()
        {
            if (AutoCheckUpdates)
                _ = CheckForUpdate();
        }

        public static Version? ProgramVersion() => Assembly.GetExecutingAssembly().GetName().Version;

        public async Task CheckForUpdate()
        {
            if (IsCheckingUpdate)
                return;

            UpdateString     = "(Checking for updates)";
            IsCheckingUpdate = true;

            try
            {
                var version = await GetLatestVersion();
                UpdateString = version.Version > ProgramVersion()
                    ? $"(Update available! {version.TagName})"
                    : string.Empty;
            }
            catch (Exception)
            {
                UpdateString = "(Failed to check updates)";
            }
            finally
            {
                IsCheckingUpdate = false;
            }
        }

        public async Task<GitVersion> GetLatestVersion()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/sabihoshi/GenshinLyreMidiPlayer/releases");

            var productInfo = new ProductInfoHeaderValue("GenshinLyreMidiPlayer", ProgramVersion()?.ToString());

            request.Headers.UserAgent.Add(productInfo);

            var response = await client.SendAsync(request);
            var versions = JsonSerializer.Deserialize<List<GitVersion>>(await response.Content.ReadAsStringAsync());

            return versions
                .OrderByDescending(v => v.Version)
                .First(v => !v.Draft && !v.Prerelease || IncludeBetaUpdates);
        }
    }
}