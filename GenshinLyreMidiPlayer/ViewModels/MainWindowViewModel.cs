using System;
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
        private TrackChunk _firstTrack;
        private bool _ignoreSliderChange;
        private MidiFile _midiFile;
        private Playback _playback;
        private ITimeSpan _playTime = new MidiTimeSpan();
        private Timer _playTimer;
        private bool _reloadPlayback;
        private double _songSlider;

        public MainWindowViewModel()
        {
            SelectedSpeed = MidiSpeeds[3];
        }

        public BindableCollection<MidiInputModel> MidiInputs { get; set; }

        public BindableCollection<MidiSpeedModel> MidiSpeeds { get; set; } = new BindableCollection<MidiSpeedModel>
        {
            new MidiSpeedModel("0.25", 0.25),
            new MidiSpeedModel("0.5", 0.5),
            new MidiSpeedModel("0.75", 0.75),
            new MidiSpeedModel("Normal", 1),
            new MidiSpeedModel("1.25", 1.25),
            new MidiSpeedModel("1.5", 1.5),
            new MidiSpeedModel("1.75", 1.75),
            new MidiSpeedModel("2", 2)
        };

        public BindableCollection<MidiTrackModel> MidiTracks { get; set; } = new BindableCollection<MidiTrackModel>();

        public bool TransposeNotes { get; set; } = true;

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

        public MidiInputModel SelectedMidiInput { get; set; }

        public MidiSpeedModel SelectedSpeed { get; set; }

        private static string PlayIcon => "\xEDB5";

        private static string PauseIcon => "\xEDB4";

        public string PlayPauseIcon { get; set; } = PlayIcon;

        public string TotalTime { get; set; } = "0:00";

        public string CurrentTime { get; set; } = "0:00";

        public string SongName { get; set; }

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
            MidiTracks.Clear();

            SongName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

            _midiFile = MidiFile.Read(openFileDialog.FileName);

            CurrentTime = "0:00";
            UpdateSlider(0);

            TimeSpan duration = _midiFile.GetDuration<MetricTimeSpan>();
            TotalTime = duration.ToString("m\\:ss");
            MaximumTime = duration.TotalSeconds;

            var chunks = _midiFile.GetTrackChunks().ToList();
            _firstTrack = chunks.FirstOrDefault();
            _midiFile.Chunks.Remove(_firstTrack);

            MidiTracks.AddRange(chunks.Select(t => new MidiTrackModel(t)));
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
            SongName = string.Empty;
            TotalTime = "0:00";
            CurrentTime = "0:00";
            MaximumTime = 1;
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
                    _playback = null;
                    PlayPauseIcon = PlayIcon;
                }

                _midiFile.Chunks.Clear();
                _midiFile.Chunks.Add(_firstTrack);
                _midiFile.Chunks.AddRange(MidiTracks
                    .Where(t => t.IsChecked)
                    .Select(t => t.Track));

                _playback = _midiFile.GetPlayback();
                _playback.Speed = SelectedSpeed.Speed;

                _playback.MoveToTime(_playTime);
                _playback.Finished += (s, e) => { CloseFile(); };

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(_playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnSongTick;
                PlaybackCurrentTimeWatcher.Instance.Start();

                _playback.EventPlayed += OnNoteEvent;
                _reloadPlayback = false;
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
                _playTimer = new Timer {Interval = 100};
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

        public void OnNoteEvent(object sender, MidiEventPlayedEventArgs e)
        {
            if (e.Event.EventType == MidiEventType.NoteOn)
            {
                var note = e.Event as NoteOnEvent;
                if (note != null && note.Velocity <= 0) return;

                if (!LyrePlayer.PlayNote(note, TransposeNotes))
                    PlayPause();
            }
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
            SongSlider = value;
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