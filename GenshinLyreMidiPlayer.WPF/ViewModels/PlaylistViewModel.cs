using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using GenshinLyreMidiPlayer.Data;
using GenshinLyreMidiPlayer.Data.Entities;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Errors;
using Melanchall.DryWetMidi.Core;
using Microsoft.Win32;
using ModernWpf;
using Stylet;
using StyletIoC;
using MidiFile = GenshinLyreMidiPlayer.Data.Midi.MidiFile;

namespace GenshinLyreMidiPlayer.WPF.ViewModels;

public class PlaylistViewModel : Screen
{
    public enum LoopMode
    {
        Once,
        Track,
        Playlist,
        All
    }

    private readonly IContainer _ioc;
    private readonly IEventAggregator _events;
    private readonly MainWindowViewModel _main;

    public PlaylistViewModel(IContainer ioc, MainWindowViewModel main)
    {
        _ioc    = ioc;
        _events = ioc.Get<IEventAggregator>();
        _main   = main;
    }

    public BindableCollection<MidiFile> FilteredTracks => string.IsNullOrWhiteSpace(FilterText)
        ? Tracks
        : new(Tracks.Where(t => t.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

    public BindableCollection<MidiFile> Tracks { get; } = new();

    public bool Shuffle { get; set; }

    public IEnumerable<string> TrackTitles => Tracks.Select(t => t.Title);

    public LoopMode Loop { get; set; } = LoopMode.All;

    public MidiFile? OpenedFile { get; set; }

    public MidiFile? SelectedFile { get; set; }

    public SolidColorBrush ShuffleStateColor => Shuffle
        ? new(ThemeManager.Current.ActualAccentColor)
        : Brushes.Gray;

    public Stack<MidiFile> History { get; } = new();

    public string LoopStateString =>
        Loop switch
        {
            LoopMode.Once     => "\xF5E7",
            LoopMode.Track    => "\xE8ED",
            LoopMode.Playlist => "\xEBE7",
            LoopMode.All      => "\xE8EE",
            _                 => string.Empty
        };

    public string? FilterText { get; set; }

    private BindableCollection<MidiFile> ShuffledTracks { get; set; } = new();

    public BindableCollection<MidiFile> GetPlaylist() => Shuffle ? ShuffledTracks : Tracks;

    public MidiFile? Next()
    {
        var playlist = GetPlaylist().ToList();
        if (OpenedFile is null) return playlist.FirstOrDefault();

        switch (Loop)
        {
            case LoopMode.Once:
                return null;
            case LoopMode.Track:
                return OpenedFile;
        }

        var next = playlist.IndexOf(OpenedFile) + 1;
        if (Loop is LoopMode.All)
            next %= playlist.Count;

        return playlist.ElementAtOrDefault(next);
    }

    public async Task AddFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            await AddFile(file);
        }

        ShuffledTracks = new(Tracks.OrderBy(_ => Guid.NewGuid()));
        RefreshPlaylist();

        var next = Next();
        if (OpenedFile is null && Tracks.Count > 0 && next is not null)
            _events.Publish(Next());
    }

    public async Task AddFiles(IEnumerable<History> files)
    {
        foreach (var file in files)
        {
            await AddFile(file);
        }

        RefreshPlaylist();
    }

    public async Task ClearPlaylist()
    {
        await using var db = _ioc.Get<LyreContext>();
        db.History.RemoveRange(db.History);
        await db.SaveChangesAsync();

        Tracks.Clear();
        FilteredTracks.Clear();
        History.Clear();

        
        OpenedFile   = null;
        SelectedFile = null;
    }

    public async Task OpenFile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter      = "MIDI file|*.mid;*.midi|All files (*.*)|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        await AddFiles(openFileDialog.FileNames);
    }

    public async Task RemoveTrack()
    {
        if (SelectedFile is not null)
        {
            await using var db = _ioc.Get<LyreContext>();
            db.History.Remove(SelectedFile.History);
            await db.SaveChangesAsync();

            OpenedFile = OpenedFile == SelectedFile ? null : OpenedFile;
            Tracks.Remove(SelectedFile);

            RefreshPlaylist();
        }
    }

    public async Task UpdateHistory()
    {
        await using var db = _ioc.Get<LyreContext>();
        db.UpdateRange(Tracks.Select(t => t.History));
        await db.SaveChangesAsync();
    }

    public void OnFileChanged(object sender, EventArgs e)
    {
        if (SelectedFile is not null)
            _events.Publish(SelectedFile);
    }

    public void OnOpenedFileChanged()
    {
        if (OpenedFile is null) return;

        var transpose = SettingsPageViewModel.TransposeNames
            .FirstOrDefault(e => e.Key == OpenedFile.History.Transpose);

        if (OpenedFile.History.Transpose is not null)
            _main.SettingsView.Transpose = transpose;
        _main.SettingsView.KeyOffset = OpenedFile.History.Key;
    }

    public void Previous()
    {
        History.Pop();
        _events.Publish(History.Pop());
    }

    public void ToggleLoop()
    {
        var loopState = (int) Loop;
        var loopStates = Enum.GetValues(typeof(LoopMode)).Length;

        var newState = (loopState + 1) % loopStates;
        Loop = (LoopMode) newState;
    }

    public void ToggleShuffle()
    {
        Shuffle = !Shuffle;

        if (Shuffle)
            ShuffledTracks = new(Tracks.OrderBy(_ => Guid.NewGuid()));

        RefreshPlaylist();
    }

    private async Task AddFile(History history, ReadingSettings? settings = null)
    {
        try
        {
            Tracks.Add(new(history, settings));
        }
        catch (Exception e)
        {
            settings ??= new();
            if (await ExceptionHandler.TryHandleException(e, settings))
                await AddFile(history, settings);
        }
    }

    private async Task AddFile(string fileName)
    {
        var history = new History(fileName, _main.SettingsView.KeyOffset);

        await AddFile(history);

        await using var db = _ioc.Get<LyreContext>();
        db.History.Add(history);
        await db.SaveChangesAsync();
    }

    private void RefreshPlaylist()
    {
        var playlist = GetPlaylist();
        foreach (var file in playlist)
        {
            file.Position = playlist.IndexOf(file);
        }
    }
}