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
using ModernWpf.Controls;
using Stylet;
using StyletIoC;
using MidiFile = GenshinLyreMidiPlayer.Data.Midi.MidiFile;

namespace GenshinLyreMidiPlayer.WPF.ViewModels
{
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

        public PlaylistViewModel(IContainer ioc, IEventAggregator events)
        {
            _ioc    = ioc;
            _events = events;
        }

        public BindableCollection<MidiFile> FilteredTracks => string.IsNullOrWhiteSpace(FilterText)
            ? Tracks
            : new(Tracks.Where(t => t.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        public BindableCollection<MidiFile> Tracks { get; } = new();

        public bool Shuffle { get; set; }

        public LoopMode Loop { get; set; }

        public MidiFile? OpenedFile { get; set; }

        public MidiFile? SelectedFile { get; set; }

        public SolidColorBrush ShuffleStateColor => Shuffle
            ? new(ThemeManager.Current.ActualAccentColor)
            : Brushes.Gray;

        public Stack<MidiFile> History { get; } = new();

        public string FilterText { get; set; }

        public string LoopStateString =>
            Loop switch
            {
                LoopMode.Once     => "\xF5E7",
                LoopMode.Track    => "\xE8ED",
                LoopMode.Playlist => "\xEBE7",
                LoopMode.All      => "\xE8EE"
            };

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
            await UpdateHistory();

            var next = Next();
            if (OpenedFile is null && Tracks.Count > 0 && next is not null)
                _events.Publish(Next());
        }

        public async Task ClearPlaylist()
        {
            Tracks.Clear();
            await UpdateHistory();
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

        public async Task UpdateHistory()
        {
            await using var db = _ioc.Get<LyreContext>();
            db.History.RemoveRange(db.History);
            db.History.AddRange(Tracks.Select(t => new History(t.Path)));
            await db.SaveChangesAsync();
        }

        public void OnFileChanged(object sender, EventArgs e)
        {
            if (SelectedFile is not null)
                _events.Publish(SelectedFile);
        }

        public void OnFilterTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            FilterText = sender.Text;
        }

        public void Previous()
        {
            History.Pop();
            _events.Publish(History.Pop());
        }

        public void RemoveTrack()
        {
            if (SelectedFile is not null)
            {
                Tracks.Remove(SelectedFile);
                RefreshPlaylist();
            }
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

        private async Task AddFile(string fileName, ReadingSettings? settings = null)
        {
            try
            {
                var file = new MidiFile(fileName, settings);
                Tracks.Add(file);
            }
            catch (Exception e)
            {
                settings ??= new();
                if (await ExceptionHandler.TryHandleException(e, settings))
                    await AddFile(fileName, settings);
            }
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
}