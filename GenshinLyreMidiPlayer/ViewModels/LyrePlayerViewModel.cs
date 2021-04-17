using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        IHandle<MidiFileModel>, IHandle<MidiTrackModel>,
        IHandle<SettingsPageViewModel>
    {
        private readonly IEventAggregator _events;
        private readonly SettingsPageViewModel _settings;
        private readonly OutputDevice _speakers = OutputDevice.GetByName("Microsoft GS Wavetable Synth");
        private readonly PlaybackCurrentTimeWatcher _timeWatcher = PlaybackCurrentTimeWatcher.Instance;
        private bool _ignoreSliderChange;
        private InputDevice? _inputDevice;
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

            _timeWatcher.CurrentTimeChanged += OnSongTick;
        }

        public BindableCollection<MidiInputModel> MidiInputs { get; set; } = new()
        {
            new MidiInputModel("None")
        };

        public bool CanHitPlayPause => Playback is not null && Playlist.OpenedFile?.Midi.Chunks.Count > 0;

        public bool CanHitPrevious => CurrentTime > TimeSpan.FromSeconds(3) || Playlist.History.Count > 1;

        public bool CanHitNext
        {
            get
            {
                if (Playlist.Loop is LoopState.All or LoopState.Single)
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
                if (!_ignoreSliderChange && Playback is { IsRunning: true })
                    Playback.Stop();

                SetAndNotify(ref _songSlider, value);

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
            InitializePlayback();
        }

        public void Handle(SettingsPageViewModel message)
        {
            InitializePlayback();
        }

        public void CloseFile()
        {
            if (Playback != null)
            {
                _timeWatcher.RemovePlayback(Playback);

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
            if (Playlist.OpenedFile is null)
                return;

            var midi = Playlist.OpenedFile.Midi;
            midi.Chunks.Clear();
            midi.Chunks.AddRange(MidiTracks
                .Where(t => t.IsChecked)
                .Select(t => t.Track));

            if (_settings.MergeNotes)
                midi.MergeNotes(new NotesMergingSettings
                {
                    Tolerance = new MetricTimeSpan(0, 0, 0, (int) _settings.MergeMilliseconds)
                });

            var oldPlayback = Playback;

            Playback       = midi.GetPlayback();
            Playback.Speed = _settings.SelectedSpeed.Speed;

            Playback.Finished    += (_, _) => { Next(); };
            Playback.EventPlayed += OnNoteEvent;

            Playback.Started += (_, _) =>
            {
                _timeWatcher.RemoveAllPlaybacks();
                _timeWatcher.AddPlayback(Playback, TimeSpanType.Metric);
                _timeWatcher.Start();

                NotifyOfPropertyChange(() => PlayPauseIcon);
            };

            Playback.Stopped += (_, _) =>
            {
                _timeWatcher.Stop();

                NotifyOfPropertyChange(() => PlayPauseIcon);
            };

            if (_settings.UseSpeakers)
                Playback.OutputDevice = _speakers;
        }

        public void Previous()
        {
            if (CurrentTime > TimeSpan.FromSeconds(3))
            {
                Playback!.MoveToStart();
                MoveSlider(0);
            }
            else
            {
                Playlist.Previous();
            }
        }

        public void Next()
        {
            var next = Playlist.Next();
            if (next is null)
                CloseFile();
            else if (next == Playlist.OpenedFile && Playlist.Loop == LoopState.Single)
                Handle(next);
            else if (next != Playlist.OpenedFile)
                Handle(next);

            if (Playback is not null)
                PlayPause();

            NotifyOfPropertyChange(() => PlayPauseIcon);
        }

        public void PlayPause()
        {
            if (Playback is null) InitializePlayback();

            if (Playback!.IsRunning)
            {
                Playback.Stop();
            }
            else
            {
                Playback.Loop = Playlist.Loop == LoopState.Single;

                var time = (MetricTimeSpan) TimeSpan.FromSeconds(SongSlider);
                Playback.PlaybackStart = time;
                Playback.MoveToTime(time);

                if (_settings.UseSpeakers)
                    Playback.Start();
                else
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
                Debug.WriteLine(time);
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
            if (_settings.UseSpeakers)
                return;

            if (!WindowHelper.IsGameFocused())
            {
                Playback?.Stop();
                return;
            }

            var layout = _settings.SelectedLayout.Key;
            var note = noteEvent.NoteNumber - _settings.KeyOffset;
            if (_settings.TransposeNotes)
                note = LyrePlayer.TransposeNote(note);

            switch (noteEvent.EventType)
            {
                case MidiEventType.NoteOff:
                    LyrePlayer.NoteUp(note, layout);
                    break;
                case MidiEventType.NoteOn when noteEvent.Velocity <= 0:
                    return;
                case MidiEventType.NoteOn when _settings.HoldNotes:
                    LyrePlayer.NoteDown(note, layout);
                    break;
                case MidiEventType.NoteOn:
                    LyrePlayer.PlayNote(note, layout);
                    break;
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