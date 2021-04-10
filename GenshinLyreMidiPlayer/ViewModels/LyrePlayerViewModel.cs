using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Core;
using GenshinLyreMidiPlayer.Models;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using Microsoft.Win32;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class LyrePlayerViewModel : Screen, IHandle<MidiTrackModel>, IHandle<SettingsPageViewModel>
    {
        private readonly IEventAggregator _events;
        private readonly SettingsPageViewModel _settings;
        private bool _ignoreSliderChange;
        private InputDevice? _inputDevice;
        private MidiFile? _midiFile;
        private Playback? _playback;
        private ITimeSpan _playTime = new MidiTimeSpan();
        private bool _reloadPlayback;
        private MidiInputModel? _selectedMidiInput;
        private double _songSlider;

        public LyrePlayerViewModel(IEventAggregator events, SettingsPageViewModel settings)
        {
            _settings         = settings;
            SelectedMidiInput = MidiInputs[0];

            _events = events;
            _events.Subscribe(this);
        }

        public BindableCollection<MidiInputModel> MidiInputs { get; set; } = new()
        {
            new MidiInputModel("None")
        };

        public bool CanPlayPause { get; set; }

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
                    _playback.MoveToTime((MetricTimeSpan) time);
                }

                _ignoreSliderChange = false;
            }
        }

        public List<MidiTrackModel> MidiTracks { get; set; } = new();

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

        public string SongName { get; set; } = "Open MIDI file...";

        public TimeSpan Duration { get; set; }

        public TimeSpan CurrentTime => TimeSpan.FromSeconds(SongSlider);

        public void Handle(MidiTrackModel message)
        {
            _reloadPlayback = true;
        }

        public void Handle(SettingsPageViewModel message)
        {
            if (_playback != null)
            {
                _playback.Speed = message.SelectedSpeed.Speed;
                _reloadPlayback = true;
            }
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

            MoveSlider(0);

            Duration    = _midiFile.GetDuration<MetricTimeSpan>();
            MaximumTime = Duration.TotalSeconds;

            MidiTracks = _midiFile
                .GetTrackChunks()
                .Select(t => new MidiTrackModel(_events, t))
                .ToList();
            MidiTracks.First().IsChecked = true;

            InitializePlayback();
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

            CanPlayPause  = false;
            PlayPauseIcon = PlayIcon;

            SongName    = string.Empty;
            MaximumTime = 1;
        }

        private void InitializePlayback()
        {
            _midiFile.Chunks.Clear();
            _midiFile.Chunks.AddRange(MidiTracks
                .Where(t => t.IsChecked)
                .Select(t => t.Track));

            if (_settings.MergeNotes)
            {
                _midiFile.MergeNotes(new NotesMergingSettings
                {
                    Tolerance = new MetricTimeSpan(0, 0, 0, (int) _settings.MergeMilliseconds)
                });
            }

            _playback       = _midiFile.GetPlayback();
            _playback.Speed = _settings.SelectedSpeed.Speed;

            _playback.Finished    += (_, _) => { CloseFile(); };
            _playback.EventPlayed += OnNoteEvent;

            _reloadPlayback = false;
            CanPlayPause    = true;
        }

        public void Previous()
        {
            if (_playback != null)
            {
                _playback.MoveToStart();
                MoveSlider(0);
            }
        }

        public void Next()
        {
            CloseFile();
        }

        public void PlayPause()
        {
            if (_playback is null)
                InitializePlayback();

            if (_reloadPlayback)
            {
                _playTime = _playback!.GetCurrentTime(TimeSpanType.Midi);
                _playback.Stop();
                _playback.Dispose();

                InitializePlayback();
                _playback!.MoveToTime(_playTime);
            }

            if (_playback!.IsRunning)
            {
                PlayPauseIcon = PlayIcon;
                _playback.Stop();
            }
            else
            {
                PlayPauseIcon = PauseIcon;

                var watcher = PlaybackCurrentTimeWatcher.Instance;
                watcher.AddPlayback(_playback, TimeSpanType.Metric);
                watcher.CurrentTimeChanged += OnSongTick;
                watcher.Start();

                Task.Run(async () =>
                {
                    WindowHelper.EnsureGameOnTop();
                    await Task.Delay(100);

                    if (WindowHelper.IsGameFocused())
                    {
                        _playback.PlaybackStart = _playback.GetCurrentTime(TimeSpanType.Midi);
                        _playback.Start();
                    }
                });
            }
        }

        public void OnSongTick(object? sender, PlaybackCurrentTimeChangedEventArgs e)
        {
            foreach (var playbackTime in e.Times)
            {
                TimeSpan time = (MetricTimeSpan) playbackTime.Time;
                MoveSlider(time.TotalSeconds);
            }
        }

        private void OnNoteEvent(object? sender, MidiEventPlayedEventArgs e)
        {
            if (e.Event is not NoteEvent noteEvent)
                return;

            PlayNote(noteEvent);
        }

        private void OnNoteEvent(object? sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event is not NoteEvent noteEvent)
                return;

            PlayNote(noteEvent);
        }

        private void PlayNote(NoteEvent noteEvent)
        {
            if (!WindowHelper.IsGameFocused())
            {
                PlayPause();
                return;
            }

            var layout = _settings.SelectedLayout.Key;
            var note = noteEvent.NoteNumber - _settings.KeyOffset;
            if (_settings.TransposeNotes)
                note = LyrePlayer.TransposeNote(note);

            if (noteEvent.EventType == MidiEventType.NoteOff)
                LyrePlayer.NoteUp(note, layout);
            else if (noteEvent.EventType == MidiEventType.NoteOn)
            {
                if (noteEvent.Velocity <= 0)
                    return;

                if (_settings.HoldNotes)
                    LyrePlayer.NoteDown(note, layout);
                else
                    LyrePlayer.PlayNote(note, layout);
            }
        }

        private void MoveSlider(double value)
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