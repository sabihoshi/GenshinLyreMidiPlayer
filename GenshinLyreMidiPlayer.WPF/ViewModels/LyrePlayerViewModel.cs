using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using GenshinLyreMidiPlayer.Data.Midi;
using GenshinLyreMidiPlayer.Data.Notification;
using GenshinLyreMidiPlayer.Data.Properties;
using GenshinLyreMidiPlayer.WPF.Core;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using Stylet;
using StyletIoC;
using static Windows.Media.MediaPlaybackAutoRepeatMode;
using MidiFile = GenshinLyreMidiPlayer.Data.Midi.MidiFile;

namespace GenshinLyreMidiPlayer.WPF.ViewModels
{
    public class LyrePlayerViewModel : Screen,
        IHandle<MidiFile>, IHandle<MidiTrack>,
        IHandle<SettingsPageViewModel>,
        IHandle<MergeNotesNotification>
    {
        private static readonly Settings Settings = Settings.Default;
        private readonly IEventAggregator _events;
        private readonly MediaPlayer _player;
        private readonly SettingsPageViewModel _settings;
        private readonly OutputDevice _speakers;
        private readonly PlaybackCurrentTimeWatcher _timeWatcher;
        private bool _ignoreSliderChange;
        private InputDevice? _inputDevice;
        private MidiInput? _selectedMidiInput;
        private double _songSlider;

        public LyrePlayerViewModel(IContainer ioc,
            SettingsPageViewModel settings, PlaylistViewModel playlist)
        {
            _speakers    = OutputDevice.GetByName("Microsoft GS Wavetable Synth");
            _timeWatcher = PlaybackCurrentTimeWatcher.Instance;

            _events = ioc.Get<IEventAggregator>();
            _events.Subscribe(this);

            _settings = settings;
            Playlist  = playlist;

            SelectedMidiInput = MidiInputs[0];

            _timeWatcher.CurrentTimeChanged += OnSongTick;

            _player = ioc.Get<MediaPlayer>();

            _player.CommandManager.NextReceived     += (_, _) => Next();
            _player.CommandManager.PreviousReceived += (_, _) => Previous();

            _player.CommandManager.PlayReceived  += (_, _) => PlayPause();
            _player.CommandManager.PauseReceived += (_, _) => PlayPause();
        }

        public BindableCollection<MidiInput> MidiInputs { get; } = new()
        {
            new("None")
        };

        public BindableCollection<MidiTrack> MidiTracks { get; } = new();

        public bool CanHitNext
        {
            get
            {
                if (Playlist.Loop is List or Track)
                    return true;

                var last = Playlist.GetPlaylist().LastOrDefault();
                return Playlist.OpenedFile != last;
            }
        }

        public bool CanHitPlayPause
        {
            get
            {
                var hasNotes = MidiTracks?
                    .Where(t => t.IsChecked)
                    .Any(t => t.CanBePlayed) ?? false;

                return Playback is not null
                       && hasNotes
                       && MaximumTime > TimeSpan.Zero;
            }
        }

        public bool CanHitPrevious => CurrentTime > TimeSpan.FromSeconds(3) || Playlist.History.Count > 1;

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

        public MidiInput? SelectedMidiInput
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

        private MusicDisplayProperties Display => _player.SystemMediaTransportControls.DisplayUpdater.MusicProperties;

        public Playback? Playback { get; private set; }

        public PlaylistViewModel Playlist { get; }

        private static string PauseIcon => "\xEDB4";

        private static string PlayIcon => "\xF5B0";

        public string PlayPauseIcon => Playback?.IsRunning ?? false ? PauseIcon : PlayIcon;

        private SystemMediaTransportControls Controls => _player.SystemMediaTransportControls;

        public TimeSpan CurrentTime => TimeSpan.FromSeconds(SongSlider);

        public TimeSpan MaximumTime => Playlist.OpenedFile?.Duration ?? TimeSpan.Zero;

        public void Handle(MergeNotesNotification message)
        {
            if (!message.Merge)
                InitializeTracks();

            InitializePlayback();
        }

        public void Handle(MidiFile file)
        {
            CloseFile();
            Playlist.OpenedFile = file;
            Playlist.History.Push(file);

            InitializeTracks();
            InitializePlayback();
        }

        public void Handle(MidiTrack track) { InitializePlayback(); }

        public void Handle(SettingsPageViewModel message) { InitializePlayback(); }

        public void UpdateButtons()
        {
            NotifyOfPropertyChange(() => CanHitNext);
            NotifyOfPropertyChange(() => CanHitPrevious);
            NotifyOfPropertyChange(() => CanHitPlayPause);

            NotifyOfPropertyChange(() => PlayPauseIcon);
            NotifyOfPropertyChange(() => MaximumTime);

            var controls = Controls;

            controls.IsPlayEnabled  = CanHitPlayPause;
            controls.IsPauseEnabled = CanHitPlayPause;

            controls.IsNextEnabled     = CanHitNext;
            controls.IsPreviousEnabled = CanHitPrevious;

            controls.PlaybackStatus =
                Playlist.OpenedFile is null ? MediaPlaybackStatus.Closed :
                Playback is null ? MediaPlaybackStatus.Stopped :
                Playback.IsRunning ? MediaPlaybackStatus.Playing :
                MediaPlaybackStatus.Paused;

            var file = Playlist.OpenedFile;
            if (file is not null)
            {
                var position = $"{file.Position}/{Playlist.GetPlaylist().Count}";

                Display.Title  = file.Title;
                Display.Artist = $"Playing {position} {CurrentTime:mm\\:ss}";
            }

            controls.DisplayUpdater.Update();
        }

        private void InitializeTracks()
        {
            MidiTracks.Clear();
            MidiTracks.AddRange(Playlist.OpenedFile?
                .Midi.GetTrackChunks()
                .Select(t => new MidiTrack(_events, t)));
        }

        public async Task OpenFile()
        {
            await Playlist.OpenFile();
            UpdateButtons();
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
            Playback?.Stop();
            Playback?.Dispose();

            if (Playlist.OpenedFile is null)
                return;

            var midi = Playlist.OpenedFile.Midi;

            midi.Chunks.Clear();
            midi.Chunks.AddRange(MidiTracks
                .Where(t => t.IsChecked)
                .Select(t => t.Track));

            if (Settings.MergeNotes)
            {
                midi.MergeNotes(new()
                {
                    Tolerance = new MetricTimeSpan(0, 0, 0, (int) Settings.MergeMilliseconds)
                });
            }

            Playback       = midi.GetPlayback();
            Playback.Speed = _settings.SelectedSpeed.Speed;

            Playback.InterruptNotesOnStop = true;

            Playback.Finished    += (_, _) => { Next(); };
            Playback.EventPlayed += OnNoteEvent;

            Playback.Started += (_, _) =>
            {
                _timeWatcher.RemoveAllPlaybacks();
                _timeWatcher.AddPlayback(Playback, TimeSpanType.Metric);
                _timeWatcher.Start();

                UpdateButtons();
            };

            Playback.Stopped += (_, _) =>
            {
                _timeWatcher.Stop();

                UpdateButtons();
            };

            UpdateButtons();
        }

        public void Previous()
        {
            if (CurrentTime > TimeSpan.FromSeconds(3))
            {
                Playback!.MoveToStart();
                MoveSlider(0);
            }
            else
                Playlist.Previous();
        }

        public void Next()
        {
            var next = Playlist.Next();
            if (next is null)
                return;

            if (next == Playlist.OpenedFile && Playlist.Loop == Track)
                Handle(next);
            else if (next != Playlist.OpenedFile)
                Handle(next);

            if (Playback is not null)
                PlayPause();
        }

        public void PlayPause()
        {
            if (Playback is null) InitializePlayback();

            if (Playback!.IsRunning)
                Playback.Stop();
            else
            {
                Playback.Loop = Playlist.Loop == Track;

                var time = (MetricTimeSpan) TimeSpan.FromSeconds(SongSlider);
                Playback.PlaybackStart = time;
                Playback.MoveToTime(time);

                if (Settings.UseSpeakers)
                    Playback.Start();
                else
                {
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
        }

        public void OnSongTick(object? sender, PlaybackCurrentTimeChangedEventArgs e)
        {
            foreach (var playbackTime in e.Times)
            {
                TimeSpan time = (MetricTimeSpan) playbackTime.Time;
                MoveSlider(time.TotalSeconds);

                UpdateButtons();
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
            if (Settings.UseSpeakers)
            {
                _speakers.SendEvent(noteEvent);
                return;
            }

            if (!WindowHelper.IsGameFocused())
            {
                Playback?.Stop();
                return;
            }

            var layout = _settings.SelectedLayout.Key;
            var note = noteEvent.NoteNumber - Settings.KeyOffset;
            if (Settings.TransposeNotes)
                note = LyrePlayer.TransposeNote(note);

            switch (noteEvent.EventType)
            {
                case MidiEventType.NoteOff:
                    LyrePlayer.NoteUp(note, layout);
                    break;
                case MidiEventType.NoteOn when noteEvent.Velocity <= 0:
                    return;
                case MidiEventType.NoteOn when Settings.HoldNotes:
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
            MidiInputs.Add(new("None"));

            foreach (var device in InputDevice.GetAll())
            {
                MidiInputs.Add(new(device.Name));
            }

            SelectedMidiInput = MidiInputs[0];
        }
    }
}