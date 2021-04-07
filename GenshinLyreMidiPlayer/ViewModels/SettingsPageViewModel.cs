using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _keyOffset;
        private uint _mergeMilliseconds;
        private bool _mergeNotes;
        private MidiSpeedModel _selectedSpeed;

        public SettingsPageViewModel(IEventAggregator events)
        {
            _events     = events;
            Transitions = new TransitionCollection();
            Transition  = Transitions.First();

            SelectedLayout = Keyboard.LayoutNames.First();
            SelectedSpeed  = MidiSpeeds[3];
        }

        public bool HoldNotes { get; set; } = false;

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

        public static CaptionedObject<Transition> Transition { get; set; }

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

        public static IEnumerable<CaptionedObject<Transition>> Transitions { get; private set; }

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

        public uint MergeMilliseconds
        {
            get => _mergeMilliseconds;
            set
            {
                SetAndNotify(ref _mergeMilliseconds, value);
                _events.Publish(this);
            }
        }
    }
}