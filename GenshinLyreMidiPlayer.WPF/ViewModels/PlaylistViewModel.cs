using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Media;
using GenshinLyreMidiPlayer.Data;
using GenshinLyreMidiPlayer.Data.Entities;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Errors;
using Melanchall.DryWetMidi.Core;
using Microsoft.Win32;
using ModernWpf;
using Stylet;
using StyletIoC;
using static Windows.Media.MediaPlaybackAutoRepeatMode;
using MidiFile = GenshinLyreMidiPlayer.Data.Midi.MidiFile;

namespace GenshinLyreMidiPlayer.WPF.ViewModels
{
    public class PlaylistViewModel : Screen
    {
        private readonly IEventAggregator _events;
        private readonly IContainer _ioc;

        public PlaylistViewModel(IContainer ioc, IEventAggregator events)
        {
            _ioc    = ioc;
            _events = events;
        }

        public BindableCollection<MidiFile> Tracks { get; set; } = new();

        public BindableCollection<MidiFile>? ShuffledTracks { get; set; }

        public bool Shuffle { get; set; }

        public MediaPlaybackAutoRepeatMode Loop { get; set; }

        public MidiFile? OpenedFile { get; set; }

        public MidiFile? SelectedFile { get; set; }

        public SolidColorBrush ShuffleStateColor => Shuffle
            ? new(ThemeManager.Current.ActualAccentColor)
            : Brushes.Gray;

        public Stack<MidiFile> History { get; } = new();

        public string LoopStateString =>
            Loop switch
            {
                None  => "\xF5E7",
                Track => "\xE8ED",
                List  => "\xE8EE"
            };

        public void ToggleShuffle()
        {
            Shuffle = !Shuffle;

            if (Shuffle)
                ShuffledTracks = new(Tracks.OrderBy(_ => Guid.NewGuid()));

            RefreshPlaylist();
        }

        public void ToggleLoop()
        {
            var loopState = (int) Loop;
            var loopStates = Enum.GetValues(typeof(MediaPlaybackAutoRepeatMode)).Length;

            var newState = (loopState + 1) % loopStates;
            Loop = (MediaPlaybackAutoRepeatMode) newState;
        }

        public MidiFile? Next()
        {
            var playlist = GetPlaylist().ToList();

            if (Loop == Track)
                return OpenedFile ?? playlist.FirstOrDefault();

            var next = playlist.FirstOrDefault();

            if (OpenedFile is not null)
            {
                var current = playlist.IndexOf(OpenedFile) + 1;

                if (Loop is List)
                    current %= playlist.Count;

                next = playlist.ElementAtOrDefault(current);
            }

            return next;
        }

        public BindableCollection<MidiFile> GetPlaylist() => (Shuffle ? ShuffledTracks : Tracks)!;

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

        public async Task AddFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                await AddFile(file);
            }

            ShuffledTracks = new(Tracks.OrderBy(_ => Guid.NewGuid()));
            RefreshPlaylist();
            await UpdateHistory();

            if (OpenedFile is null && Tracks.Count > 0)
                _events.Publish(Next());
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

        public void RemoveTrack()
        {
            if (SelectedFile is not null)
            {
                Tracks.Remove(SelectedFile);
                RefreshPlaylist();
            }
        }

        public async Task ClearPlaylist()
        {
            Tracks.Clear();
            await UpdateHistory();
        }

        public async Task UpdateHistory()
        {
            await using var db = _ioc.Get<LyreContext>();
            db.History.RemoveRange(db.History);
            db.History.AddRange(Tracks.Select(t => new History(t.Path)));
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

        public void OnFileChanged(object sender, EventArgs e)
        {
            if (SelectedFile is not null)
                _events.Publish(SelectedFile);
        }

        public void Previous()
        {
            History.Pop();
            _events.Publish(History.Pop());
        }
    }
}