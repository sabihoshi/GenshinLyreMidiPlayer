using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using GenshinLyreMidiPlayer.Core;
using GenshinLyreMidiPlayer.Models;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Win32;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class LyrePlayerViewModel : Screen, IHandle<MidiTrackModel>
    {
        private readonly SettingsPageViewModel _settings;
        private bool _ignoreSliderChange;
        private InputDevice? _inputDevice;
        private MidiFile? _midiFile;
        private Playback? _playback;
        private ITimeSpan _playTime = new MidiTimeSpan();
        private Timer _playTimer = new Timer();
        private bool _reloadPlayback;
        private MidiInputModel? _selectedMidiInput;
        private double _songSlider;

        public LyrePlayerViewModel(SettingsPageViewModel settings)
        {
            _settings         = settings;
            SelectedMidiInput = MidiInputs[0];
        }

        public BindableCollection<MidiInputModel> MidiInputs { get; set; } = new BindableCollection<MidiInputModel>
        {
            new MidiInputModel("None")
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

        public List<MidiTrackModel> MidiTracks { get; set; } = new List<MidiTrackModel>();

        public MidiInputModel? SelectedMidiInput
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

        private static string PlayIcon => "\xEDB5";

        private static string PauseIcon => "\xEDB4";

        public string PlayPauseIcon { get; set; } = PlayIcon;

        public string TotalTime { get; set; } = "0:00";

        public string CurrentTime { get; set; } = "0:00";

        public string SongName { get; set; } = "Open MIDI file...";

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
                .Select(t => new MidiTrackModel(t))
                .ToList();
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

            _midiFile = null;
            MidiTracks.Clear();

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
                _playback.Speed = _settings.SelectedSpeed.Speed;

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

                WindowHelper.EnsureGameOnTop();
                _playTimer         =  new Timer {Interval = 100};
                _playTimer.Elapsed += PlayTimerElapsed;
                _playTimer.Start();
            }
        }

        public void OnSongTick(object? sender, PlaybackCurrentTimeChangedEventArgs e)
        {
            foreach (var playbackTime in e.Times)
            {
                TimeSpan time = (MetricTimeSpan) playbackTime.Time;

                UpdateSlider(time.TotalSeconds);
                CurrentTime = time.ToString("m\\:ss");
            }
        }

        private void OnNoteEvent(object? sender, MidiEventPlayedEventArgs e)
        {
            if (e.Event.EventType == MidiEventType.NoteOn)
                PlayNote(e.Event as NoteOnEvent);
        }

        private void OnNoteEvent(object? sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event.EventType == MidiEventType.NoteOn)
                PlayNote(e.Event as NoteOnEvent);
        }

        private void PlayNote(NoteOnEvent? note)
        {
            if (note is null || note.Velocity <= 0)
                return;

            if (!LyrePlayer.PlayNote(note, _settings.TransposeNotes, _settings.KeyOffset,
                _settings.SelectedLayout.Key))
                PlayPause();
        }

        private void PlayTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (WindowHelper.IsGameFocused())
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