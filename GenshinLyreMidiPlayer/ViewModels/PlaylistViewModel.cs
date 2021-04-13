using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GenshinLyreMidiPlayer.Models;
using Microsoft.Win32;
using ModernWpf;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class PlaylistViewModel : Screen
    {
        public enum LoopState
        {
            None,
            Single,
            All
        }

        private readonly IEventAggregator _events;

        public PlaylistViewModel(IEventAggregator events)
        {
            _events = events;
        }

        public BindableCollection<MidiFileModel> Tracks { get; set; } = new();

        public BindableCollection<MidiFileModel>? ShuffledTracks { get; set; }

        public bool Shuffle { get; set; }

        public LoopState Loop { get; set; }

        public MidiFileModel? SelectedFile { get; set; }

        public MidiFileModel? OpenedFile { get; set; }

        public SolidColorBrush ShuffleStateColor => Shuffle
            ? new SolidColorBrush(ThemeManager.Current.ActualAccentColor)
            : Brushes.Gray;

        public Stack<MidiFileModel> History { get; } = new();

        public string LoopStateString =>
            Loop switch
            {
                LoopState.None   => "\xF5E7",
                LoopState.Single => "\xE8ED",
                LoopState.All    => "\xE8EE"
            };

        public void ToggleShuffle()
        {
            Shuffle = !Shuffle;

            if (Shuffle)
                ShuffledTracks = new BindableCollection<MidiFileModel>(Tracks.OrderBy(_ => Guid.NewGuid()));

            RefreshPlaylist();
        }

        public void ToggleLoop()
        {
            var loopState = (int) Loop;
            var loopStates = Enum.GetValues(typeof(LoopState)).Length;

            var newState = (loopState + 1) % loopStates;
            Loop = (LoopState) newState;
        }

        public void Next()
        {
            var playlist = GetPlaylist().ToList();
            var next = playlist.FirstOrDefault();

            if (SelectedFile is not null)
            {
                var current = playlist.IndexOf(SelectedFile);

                if (Loop is LoopState.All or LoopState.None)
                    current += 1 % playlist.Count;

                next = playlist.ElementAtOrDefault(current);
            }

            if (next is not null)
                _events.Publish(next);
        }

        public BindableCollection<MidiFileModel> GetPlaylist() => (Shuffle ? ShuffledTracks : Tracks)!;

        public void AddFiles()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter      = "MIDI file|*.mid;*.midi|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            foreach (var fileName in openFileDialog.FileNames)
            {
                try
                {
                    var file = new MidiFileModel(fileName);
                    Tracks.Add(file);
                }
                catch
                {
                    // Skipped
                }
            }

            ShuffledTracks = new BindableCollection<MidiFileModel>(Tracks.OrderBy(_ => Guid.NewGuid()));
            RefreshPlaylist();

            if (OpenedFile is null && Tracks.Count > 0)
                Next();
        }

        public void RemoveTrack()
        {
            if (SelectedFile is not null)
            {
                Tracks.Remove(SelectedFile);
                RefreshPlaylist();
            }
        }

        public void ClearPlaylist()
        {
            Tracks.Clear();
        }

        public void RefreshPlaylist()
        {
            var playlist = GetPlaylist();
            foreach (var file in playlist)
            {
                file.Position = playlist.IndexOf(file);
            }
        }

        public void OnFileChanged(object sender, EventArgs e)
        {
            _events.Publish(SelectedFile);
        }

        public void Previous()
        {
            History.Pop();
            _events.Publish(History.Pop());
        }
    }
}