using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Core;
using GenshinLyreMidiPlayer.Models;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using Stylet;
using static GenshinLyreMidiPlayer.ViewModels.PlaylistViewModel;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class LyrePlayerViewModel : Screen,
        IHandle<MidiFileModel>,
        IHandle<MidiTrackModel>,
        IHandle<SettingsPageViewModel>
    {
        private readonly IEventAggregator _events;
        private readonly PlaybackCurrentTimeWatcher _playbackWatcher = PlaybackCurrentTimeWatcher.Instance;
        private readonly SettingsPageViewModel _settings;
        private bool _ignoreSliderChange;
        private InputDevice? _inputDevice;
        private ITimeSpan _playTime = new MidiTimeSpan();
        private MidiInputModel? _selectedMidiInput;
        private double _songSlider;

        public LyrePlayerViewModel(IEventAggregator events,
            SettingsPageViewModel settings, PlaylistViewModel playlist)
        {
            SelectedMidiInput = MidiInputs[0];

            _settings = settings;
            Playlist  = playlist;

            _events = events;
            _events.Subscribe(this);
        }

        public BindableCollection<MidiInputModel> MidiInputs { get; set; } = new()
        {
            new MidiInputModel("None")
        };

        public bool CanHitPlayPause => Playback is not null;

        public bool CanHitPrevious => CurrentTime > TimeSpan.FromSeconds(3) || Playlist.History.Count > 1;

        public bool CanHitNext
        {
            get
            {
                if (Playlist.Loop == LoopState.All)
                    return true;

                var last = Playlist.GetPlaylist().LastOrDefault();
                return Playlist.OpenedFile != last;
            }
        }

        public double SongSlider
        {
            get => _songSlider;
            set
            {
                SetAndNotify(ref _songSlider, value);

                if (!_ignoreSliderChange && Playback != null)
                {
                    if (Playback.IsRunning)
                        Playback.Stop();

                    var time = TimeSpan.FromSeconds(_songSlider);
                    Playback.MoveToTime((MetricTimeSpan) time);
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

        public Playback? Playback { get; private set; }

        public PlaylistViewModel Playlist { get; }

        private static string PlayIcon => "\xF5B0";

        private static string PauseIcon => "\xEDB4";

        public string PlayPauseIcon => Playback?.IsRunning ?? false ? PauseIcon : PlayIcon;

        public TimeSpan MaximumTime => Playlist.OpenedFile?.Duration ?? TimeSpan.Zero;

        public TimeSpan CurrentTime => TimeSpan.FromSeconds(SongSlider);

        public void Handle(MidiFileModel file)
        {
            CloseFile();
            Playlist.OpenedFile = file;
            Playlist.History.Push(file);

            MidiTracks = file.Midi
                .GetTrackChunks()
                .Select(t => new MidiTrackModel(_events, t))
                .ToList();

            InitializePlayback();

            NotifyOfPropertyChange(() => MaximumTime);
            NotifyOfPropertyChange(() => CanHitNext);
            NotifyOfPropertyChange(() => CanHitPrevious);
        }

        public void Handle(MidiTrackModel track)
        {
            ReloadPlayback();
        }

        public void Handle(SettingsPageViewModel message)
        {
            ReloadPlayback();
        }

        public void ToggleShuffle()
        {
            Playlist.ToggleShuffle();
        }

        public void OpenFile()
        {
            Playlist.AddFiles();
        }

        public void CloseFile()
        {
            if (Playback != null)
            {
                _playbackWatcher.RemovePlayback(Playback);

                Playback.Stop();
                Playback.Dispose();
            }

            MidiTracks.Clear();
            MoveSlider(0);

            Playback            = null;
            Playlist.OpenedFile = null;
        }

        private void InitializePlayback()
        {
            var midi = Playlist.OpenedFile!.Midi;
            midi.Chunks.Clear();
            midi.Chunks.AddRange(MidiTracks
                .Where(t => t.IsChecked)
                .Select(t => t.Track));

            if (_settings.MergeNotes)
            {
                midi.MergeNotes(new NotesMergingSettings
                {
                    Tolerance = new MetricTimeSpan(0, 0, 0, (int) _settings.MergeMilliseconds)
                });
            }

            Playback       = midi.GetPlayback();
            Playback.Speed = _settings.SelectedSpeed.Speed;

            Playback.Finished    += (_, _) => { Next(); };
            Playback.EventPlayed += OnNoteEvent;

            Playback.Started += (_, _) => { NotifyOfPropertyChange(() => PlayPauseIcon); };
            Playback.Stopped += (_, _) => { NotifyOfPropertyChange(() => PlayPauseIcon); };
        }

        private void ReloadPlayback()
        {
            if (Playback is null)
                return;

            _playTime = Playback.GetCurrentTime(TimeSpanType.Midi);
            Playback.Stop();
            Playback.Dispose();

            InitializePlayback();
            Playback.MoveToTime(_playTime);
        }

        public void Previous()
        {
            if (CurrentTime > TimeSpan.FromSeconds(3))
            {
                Playback.MoveToStart();
                MoveSlider(0);
            }
            else
                Playlist.Previous();
        }

        public void Next()
        {
            Playlist.Next();

            if (Playback is not null)
                PlayPause();

            NotifyOfPropertyChange(() => PlayPauseIcon);
        }

        public void PlayPause()
        {
            if (Playback is null)
                InitializePlayback();

            if (Playback.IsRunning)
                Playback.Stop();
            else
            {
                _playbackWatcher.AddPlayback(Playback, TimeSpanType.Metric);
                _playbackWatcher.CurrentTimeChanged += OnSongTick;
                _playbackWatcher.Start();

                Task.Run(async () =>
                {
                    WindowHelper.EnsureGameOnTop();
                    await Task.Delay(100);

                    if (WindowHelper.IsGameFocused())
                    {
                        Playback.PlaybackStart = Playback.GetCurrentTime(TimeSpanType.Midi);
                        Playback.Start();
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

            NotifyOfPropertyChange(() => CanHitPrevious);
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
                Playback?.Stop();
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