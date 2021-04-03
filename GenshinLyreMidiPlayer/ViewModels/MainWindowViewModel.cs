using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using GenshinLyreMidiPlayer.Models;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Win32;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class MainWindowViewModel : Screen, IHandle<MidiTrackModel>
    {
        private bool _ignoreSliderChange;
        private InputDevice _inputDevice;
        private int _keyOffset;
        private MidiFile _midiFile;
        private Playback _playback;
        private ITimeSpan _playTime = new MidiTimeSpan();
        private Timer _playTimer;
        private bool _reloadPlayback;
        private MidiInputModel _selectedMidiInput;
        private double _songSlider;

        public MainWindowViewModel()
        {
            SelectedSpeed     = MidiSpeeds[3];
            SelectedMidiInput = MidiInputs[0];
        }

        public BindableCollection<MidiInputModel> MidiInputs { get; set; } = new BindableCollection<MidiInputModel>
        {
            new MidiInputModel("None")
        };

        public bool TransposeNotes { get; set; } = true;

        public Dictionary<int, string> KeyOffsets { get; set; } = new Dictionary<int, string>
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

        public double MaximumTime { get; set; } = 1;

        public double SongSlider
        {
            get => _songSlider;
            set
            {
                SetAndNotify(ref _songSlider, value);

                if (!_ignoreSliderChange && _playback != null)
                {
                    if (_playback.IsRunning)
                    {
                        _playback.Stop();
                        PlayPauseIcon = PlayIcon;
                    }

                    var time = TimeSpan.FromSeconds(_songSlider);

                    CurrentTime = time.ToString("m\\:ss");
                    _playback.MoveToTime((MetricTimeSpan) time);
                }

                _ignoreSliderChange = false;
            }
        }

        public IEnumerable<MidiTrackModel> MidiTracks { get; set; }

        public int MinOffset => KeyOffsets.Keys.Min();

        public int MaxOffset => KeyOffsets.Keys.Max();

        public int KeyOffset
        {
            get => _keyOffset;
            set => SetAndNotify(ref _keyOffset, Math.Clamp(value, MinOffset, MaxOffset));
        }

        public List<MidiSpeedModel> MidiSpeeds { get; set; } = new List<MidiSpeedModel>
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

        public MidiInputModel SelectedMidiInput
        {
            get => _selectedMidiInput;
            set
            {
                SetAndNotify(ref _selectedMidiInput, value);

                _inputDevice?.Dispose();

                if (_selectedMidiInput?.DeviceName != null && _selectedMidiInput.DeviceName != "None")
                {
                    _inputDevice = InputDevice.GetByName(_selectedMidiInput.DeviceName);

                    _inputDevice.EventReceived += OnNoteEvent;
                    _inputDevice.StartEventsListening();
                }
            }
        }

        public MidiSpeedModel SelectedSpeed { get; set; }

        private static string PlayIcon => "\xEDB5";

        private static string PauseIcon => "\xEDB4";

        public string PlayPauseIcon { get; set; } = PlayIcon;

        public string TotalTime { get; set; } = "0:00";

        public string CurrentTime { get; set; } = "0:00";

        public string SongName { get; set; } = "Open MIDI file...";

        public string Key => $"Key: {KeyOffsets[KeyOffset]}";

        public void Handle(MidiTrackModel message)
        {
            _reloadPlayback = true;
        }

        public void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "MIDI file|*.mid;*.midi"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            CloseFile();

            SongName  = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            _midiFile = MidiFile.Read(openFileDialog.FileName);
            TimeSpan duration = _midiFile.GetDuration<MetricTimeSpan>();

            UpdateSlider(0);
            CurrentTime = "0:00";
            TotalTime   = duration.ToString("m\\:ss");
            MaximumTime = duration.TotalSeconds;

            MidiTracks = _midiFile
                .GetTrackChunks()
                .Select(t => new MidiTrackModel(t));
            MidiTracks.First().IsChecked = true;
        }

        public void CloseFile()
        {
            if (_playback != null)
            {
                _playback.Stop();
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(_playback);
                _playback.Dispose();
                _playback = null;
            }

            _midiFile  = null;
            MidiTracks = Enumerable.Empty<MidiTrackModel>();

            PlayPauseIcon = PlayIcon;
            SongName      = string.Empty;
            TotalTime     = "0:00";
            CurrentTime   = "0:00";
            MaximumTime   = 1;
        }

        public void Previous()
        {
            if (_playback != null)
            {
                _playback.MoveToStart();
                UpdateSlider(0);
                CurrentTime = "0:00";
            }
        }

        public void Next()
        {
            CloseFile();
        }

        public void PlayPause()
        {
            if (_midiFile == null || MaximumTime == 0d) return;
            if (_playback == null || _reloadPlayback)
            {
                if (_playback != null)
                {
                    _playback.Stop();
                    _playTime = _playback.GetCurrentTime(TimeSpanType.Midi);
                    _playback.Dispose();
                    _playback     = null;
                    PlayPauseIcon = PlayIcon;
                }

                _midiFile.Chunks.Clear();
                _midiFile.Chunks.AddRange(MidiTracks
                    .Where(t => t.IsChecked)
                    .Select(t => t.Track));

                _playback       = _midiFile.GetPlayback();
                _playback.Speed = SelectedSpeed.Speed;

                _playback.MoveToTime(_playTime);
                _playback.Finished += (s, e) => { CloseFile(); };

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(_playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnSongTick;
                PlaybackCurrentTimeWatcher.Instance.Start();

                _playback.EventPlayed += OnNoteEvent;
                _reloadPlayback       =  false;
            }

            if (_playback.IsRunning)
            {
                PlayPauseIcon = PlayIcon;
                _playback.Stop();
            }
            else if (PlayPauseIcon == PauseIcon)
            {
                PlayPauseIcon = PlayIcon;
                _playTimer.Dispose();
            }
            else
            {
                PlayPauseIcon = PauseIcon;

                LyrePlayer.EnsureWindowOnTop();
                _playTimer         =  new Timer {Interval = 100};
                _playTimer.Elapsed += PlayTimerElapsed;
                _playTimer.Start();
            }
        }

        public void OnSongTick(object sender, PlaybackCurrentTimeChangedEventArgs e)
        {
            foreach (var playbackTime in e.Times)
            {
                TimeSpan time = (MetricTimeSpan) playbackTime.Time;

                UpdateSlider(time.TotalSeconds);
                CurrentTime = time.ToString("m\\:ss");
            }
        }

        private void OnNoteEvent(object sender, MidiEventPlayedEventArgs e)
        {
            if (e.Event.EventType == MidiEventType.NoteOn)
                PlayNote(e.Event as NoteOnEvent);
        }

        private void OnNoteEvent(object sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event.EventType == MidiEventType.NoteOn)
                PlayNote(e.Event as NoteOnEvent);
        }

        private void PlayNote(NoteOnEvent note)
        {
            if (note != null && note.Velocity <= 0) return;

            if (!LyrePlayer.PlayNote(note, TransposeNotes, KeyOffset))
                PlayPause();
        }

        private void PlayTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (LyrePlayer.IsWindowFocused(LyrePlayer.GenshinWindowName))
            {
                _playback.Start();
                _playTimer.Dispose();
            }
        }

        private void UpdateSlider(double value)
        {
            _ignoreSliderChange = true;
            SongSlider          = value;
        }

        public void RefreshDevices()
        {
            MidiInputs.Clear();
            MidiInputs.Add(new MidiInputModel("None"));

            foreach (var device in InputDevice.GetAll())
            {
                MidiInputs.Add(new MidiInputModel(device.Name));
            }

            SelectedMidiInput = MidiInputs[0];
        }
    }
}