using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;
using Windows.Media.Playback;
using GenshinLyreMidiPlayer.Data.Entities;
using GenshinLyreMidiPlayer.Data.Midi;
using GenshinLyreMidiPlayer.Data.Notification;
using GenshinLyreMidiPlayer.Data.Properties;
using GenshinLyreMidiPlayer.WPF.Core;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Errors;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tools;
using ModernWpf.Controls;
using Stylet;
using StyletIoC;
using static GenshinLyreMidiPlayer.WPF.ViewModels.SettingsPageViewModel;
using MidiFile = GenshinLyreMidiPlayer.Data.Midi.MidiFile;

namespace GenshinLyreMidiPlayer.WPF.ViewModels;

public class LyrePlayerViewModel : Screen,
    IHandle<MidiFile>, IHandle<MidiTrack>,
    IHandle<SettingsPageViewModel>,
    IHandle<MergeNotesNotification>,
    IHandle<PlayTimerNotification>
{
    private static readonly Settings Settings = Settings.Default;
    private readonly IEventAggregator _events;
    private readonly MainWindowViewModel _main;
    private readonly MediaPlayer? _player;
    private readonly OutputDevice? _speakers;
    private readonly PlaybackCurrentTimeWatcher _timeWatcher;
    private bool _ignoreSliderChange;
    private InputDevice? _inputDevice;
    private TimeSpan _songPosition;

    public LyrePlayerViewModel(IContainer ioc, MainWindowViewModel main)
    {
        _main        = main;
        _timeWatcher = PlaybackCurrentTimeWatcher.Instance;

        _events = ioc.Get<IEventAggregator>();
        _events.Subscribe(this);

        SelectedMidiInput = MidiInputs[0];

        _timeWatcher.CurrentTimeChanged += OnSongTick;

        // SystemMediaTransportControls is only supported on Windows 10 and later
        // https://docs.microsoft.com/en-us/uwp/api/windows.media.systemmediatransportcontrols
        if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
            Environment.OSVersion.Version.Major >= 10)
        {
            _player = ioc.Get<MediaPlayer>();

            _player!.CommandManager.NextReceived     += (_, _) => Next();
            _player!.CommandManager.PreviousReceived += (_, _) => Previous();

            _player!.CommandManager.PlayReceived  += async (_, _) => await PlayPause();
            _player!.CommandManager.PauseReceived += async (_, _) => await PlayPause();
        }

        try
        {
            _speakers = OutputDevice.GetByName("Microsoft GS Wavetable Synth");
        }
        catch (ArgumentException e)
        {
            new ErrorContentDialog(e, closeText: "Ignore").ShowAsync();

            SettingsPage.CanUseSpeakers = false;
            Settings.UseSpeakers        = false;
        }
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
            if (Playlist.Loop is not PlaylistViewModel.LoopMode.Playlist)
                return true;

            var last = Playlist.GetPlaylist().LastOrDefault();
            return Playlist.OpenedFile != last;
        }
    }

    public bool CanHitPlayPause
    {
        get
        {
            var hasNotes = MidiTracks
                .Where(t => t.IsChecked)
                .Any(t => t.CanBePlayed);

            return Playback is not null
                && hasNotes
                && MaximumTime > TimeSpan.Zero;
        }
    }

    public bool CanHitPrevious => CurrentTime > TimeSpan.FromSeconds(3) || Playlist.History.Count > 1;

    public double SongPosition
    {
        get => _songPosition.TotalSeconds;
        set => SetAndNotify(ref _songPosition, TimeSpan.FromSeconds(value));
    }

    public MidiInput? SelectedMidiInput { get; set; }

    public Playback? Playback { get; private set; }

    public PlaylistViewModel Playlist => _main.PlaylistView;

    public string PlayPauseIcon => Playback?.IsRunning ?? false ? PauseIcon : PlayIcon;

    public TimeSpan CurrentTime => _songPosition;

    public TimeSpan MaximumTime => Playlist.OpenedFile?.Duration ?? TimeSpan.Zero;

    private MusicDisplayProperties? Display =>
        _player?.SystemMediaTransportControls.DisplayUpdater.MusicProperties;

    private SettingsPageViewModel SettingsPage => _main.SettingsView;

    private static string PauseIcon => "\xEDB4";

    private static string PlayIcon => "\xF5B0";

    private SystemMediaTransportControls? Controls =>
        _player?.SystemMediaTransportControls;

    public async void Handle(MergeNotesNotification message)
    {
        if (!message.Merge)
        {
            Playlist.OpenedFile?.InitializeMidi();
            InitializeTracks();
        }

        await InitializePlayback();
    }

    public async void Handle(MidiFile file)
    {
        CloseFile();
        Playlist.OpenedFile = file;
        Playlist.History.Push(file);

        InitializeTracks();
        await InitializePlayback();
    }

    public async void Handle(MidiTrack track) => await InitializePlayback();

    public async void Handle(PlayTimerNotification message)
    {
        if (!(Playback?.IsRunning ?? false) && CanHitPlayPause) await PlayPause();
    }

    public async void Handle(SettingsPageViewModel message) => await InitializePlayback();

    public async Task OpenFile()
    {
        await Playlist.OpenFile();
        UpdateButtons();
    }

    public async Task PlayPause()
    {
        if (Playback is null)
            await InitializePlayback();

        if (Playback!.IsRunning)
            Playback.Stop();
        else
        {
            var time = new MetricTimeSpan(CurrentTime);
            Playback.PlaybackStart = time;
            Playback.MoveToTime(time);

            if (Settings.UseSpeakers)
                Playback.Start();
            else
            {
                WindowHelper.EnsureGameOnTop();
                await Task.Delay(100);

                if (WindowHelper.IsGameFocused())
                {
                    Playback.PlaybackStart = Playback.GetCurrentTime(TimeSpanType.Midi);
                    Playback.Start();
                }
            }
        }
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
        MoveSlider(TimeSpan.Zero);

        Playback               = null;
        Playlist.OpenedFile    = null;
        SettingsPage.Transpose = null;
    }

    public async void Next()
    {
        var next = Playlist.Next();
        if (next is null)
        {
            if (Playback is not null)
            {
                Playback.PlaybackStart = null;
                Playback.MoveToStart();
            }

            MoveSlider(TimeSpan.Zero);
            return;
        }

        Handle(next);

        if (Playback is not null)
            await PlayPause();
    }

    public void OnSelectedMidiInputChanged()
    {
        _inputDevice?.Dispose();

        if (SelectedMidiInput?.DeviceName is not null
            && SelectedMidiInput.DeviceName != "None")
        {
            _inputDevice = InputDevice.GetByName(SelectedMidiInput.DeviceName);

            _inputDevice!.EventReceived += OnNoteEvent;
            _inputDevice!.StartEventsListening();
        }
    }

    public void OnSongPositionChanged()
    {
        NotifyOfPropertyChange(() => CurrentTime);

        if (Playback is null) Next();

        if (!_ignoreSliderChange && Playback is not null)
        {
            var isRunning = Playback.IsRunning;
            Playback.Stop();
            Playback.MoveToTime(new MetricTimeSpan(_songPosition));

            if (Settings.UseSpeakers && isRunning)
                Playback.Start();
        }

        _ignoreSliderChange = false;
    }

    public void OnSongTick(object? sender, PlaybackCurrentTimeChangedEventArgs e)
    {
        foreach (var playbackTime in e.Times)
        {
            TimeSpan time = (MetricTimeSpan) playbackTime.Time;
            MoveSlider(time);

            UpdateButtons();
        }
    }

    public void Previous()
    {
        if (CurrentTime > TimeSpan.FromSeconds(3))
        {
            Playback?.Stop();
            Playback?.MoveToStart();

            MoveSlider(TimeSpan.Zero);
            Playback?.Start();
        }
        else
            Playlist.Previous();
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

    public void UpdateButtons()
    {
        NotifyOfPropertyChange(() => CanHitNext);
        NotifyOfPropertyChange(() => CanHitPrevious);
        NotifyOfPropertyChange(() => CanHitPlayPause);

        NotifyOfPropertyChange(() => PlayPauseIcon);
        NotifyOfPropertyChange(() => MaximumTime);
        NotifyOfPropertyChange(() => CurrentTime);

        if (Controls is not null && Display is not null)
        {
            Controls.IsPlayEnabled  = CanHitPlayPause;
            Controls.IsPauseEnabled = CanHitPlayPause;

            Controls.IsNextEnabled     = CanHitNext;
            Controls.IsPreviousEnabled = CanHitPrevious;

            Controls.PlaybackStatus =
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

            Controls.DisplayUpdater.Update();
        }
    }

    private int ApplyNoteSettings(Keyboard.Instrument instrument, int noteId)
    {
        noteId -= Playlist.OpenedFile?.History.Key ?? SettingsPage.KeyOffset;
        return Settings.TransposeNotes && SettingsPage.Transpose is not null
            ? LyrePlayer.TransposeNote(instrument, ref noteId, SettingsPage.Transpose.Value.Key)
            : noteId;
    }

    private async Task InitializePlayback()
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
            midi.MergeObjects(ObjectType.Note, new()
            {
                VelocityMergingPolicy = VelocityMergingPolicy.Average,
                Tolerance = new MetricTimeSpan(0, 0, 0, (int) Settings.MergeMilliseconds)
            });
        }

        // Check for notes that cannot be played even after transposing.
        var outOfRange = midi.GetNotes().Where(n => !LyrePlayer.TryGetKey(
            SettingsPage.SelectedLayout.Key,
            SettingsPage.SelectedInstrument.Key,
            ApplyNoteSettings(SettingsPage.SelectedInstrument.Key, n.NoteNumber), out _));

        if (Playlist.OpenedFile.History.Transpose is null && outOfRange.Any())
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                var options = new Enum[] { Transpose.Up, Transpose.Down };
                var exceptionDialog = new ErrorContentDialog(
                    new MissingNotesException(
                        "Some notes cannot be played by this instrument, and may play incorrectly. " +
                        "This can be solved by snapping to the nearest semitone. " +
                        "You can choose to shift the notes up or down, or ignore the missing notes."),
                    options, "Ignore");

                var result = await exceptionDialog.ShowAsync();

                SettingsPage.Transpose = result switch
                {
                    ContentDialogResult.Primary   => TransposeNames.ElementAt(1),
                    ContentDialogResult.Secondary => TransposeNames.ElementAt(2),
                    _                             => TransposeNames.ElementAt(0)
                };
            });
        }

        var playback = midi.GetPlayback();

        Playback                      =  playback;
        playback.Speed                =  SettingsPage.SelectedSpeed.Speed;
        playback.InterruptNotesOnStop =  true;
        playback.Finished             += (_, _) => { Next(); };
        playback.EventPlayed          += OnNoteEvent;

        playback.Started += (_, _) =>
        {
            _timeWatcher.RemoveAllPlaybacks();
            _timeWatcher.AddPlayback(playback, TimeSpanType.Metric);
            _timeWatcher.Start();

            UpdateButtons();
        };

        playback.Stopped += (_, _) =>
        {
            _timeWatcher.Stop();

            UpdateButtons();
        };

        UpdateButtons();
    }

    private void InitializeTracks()
    {
        if (Playlist.OpenedFile?.Midi is null)
            return;

        MidiTracks.Clear();
        MidiTracks.AddRange(Playlist.OpenedFile
            .Midi.GetTrackChunks()
            .Select(t => new MidiTrack(_events, t)));
    }

    private void MoveSlider(TimeSpan value)
    {
        _ignoreSliderChange = true;
        SongPosition        = value.TotalSeconds;
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
        var layout = SettingsPage.SelectedLayout.Key;
        var instrument = SettingsPage.SelectedInstrument.Key;
        var note = ApplyNoteSettings(instrument, noteEvent.NoteNumber);

        if (Settings.UseSpeakers)
        {
            noteEvent.NoteNumber = new((byte) note);
            _speakers?.SendEvent(noteEvent);
            return;
        }

        if (!WindowHelper.IsGameFocused())
        {
            Playback?.Stop();
            return;
        }

        switch (noteEvent.EventType)
        {
            case MidiEventType.NoteOff:
                LyrePlayer.NoteUp(note, layout, instrument);
                break;
            case MidiEventType.NoteOn when noteEvent.Velocity <= 0:
                return;
            case MidiEventType.NoteOn when Settings.HoldNotes:
                LyrePlayer.NoteDown(note, layout, instrument);
                break;
            case MidiEventType.NoteOn:
                LyrePlayer.PlayNote(note, layout, instrument);
                break;
        }
    }
}