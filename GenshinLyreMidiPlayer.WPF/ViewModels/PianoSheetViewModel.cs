using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenshinLyreMidiPlayer.WPF.Core;
using Melanchall.DryWetMidi.Interaction;
using PropertyChanged;
using Stylet;

namespace GenshinLyreMidiPlayer.WPF.ViewModels
{
    public class PianoSheetViewModel : Screen
    {
        private uint _bars = 1;
        private uint _beats;
        private uint _shorten = 1;

        public PianoSheetViewModel(SettingsPageViewModel settingsPage,
            PlaylistViewModel playlistView)
        {
            SettingsPage = settingsPage;
            PlaylistView = playlistView;
        }

        [OnChangedMethod(nameof(Update))] public char Delimiter { get; set; } = '.';

        public PlaylistViewModel PlaylistView { get; }

        public SettingsPageViewModel SettingsPage { get; }

        public string Result { get; private set; }

        [OnChangedMethod(nameof(Update))]
        public uint Bars
        {
            get => _bars;
            set => SetAndNotify(ref _bars, Math.Max(value, 0));
        }

        [OnChangedMethod(nameof(Update))]
        public uint Beats
        {
            get => _beats;
            set => SetAndNotify(ref _beats, Math.Max(value, 0));
        }

        [OnChangedMethod(nameof(Update))]
        public uint Shorten
        {
            get => _shorten;
            set => SetAndNotify(ref _shorten, Math.Max(value, 1));
        }

        [OnChangedMethod(nameof(Update))]
        public KeyValuePair<Keyboard.Layout, string> SelectedLayout
        {
            get => SettingsPage.SelectedLayout; 
            set => SettingsPage.SelectedLayout = value;
        }

        protected override void OnActivate() { Update(); }

        public void Update()
        {
            if (PlaylistView.OpenedFile is null)
                return;

            if (Bars == 0 && Beats == 0)
                return;

            var layout = SettingsPage.SelectedLayout.Key;

            // Ticks is too small so it is not included
            var split = PlaylistView.OpenedFile.Split(Bars, Beats, 0);

            var sb = new StringBuilder();
            foreach (var bar in split)
            {
                var notes = bar.GetNotes();
                if (notes.Count == 0)
                    continue;

                var last = 0;

                foreach (var note in notes)
                {
                    var id = LyrePlayer.TransposeNote(note.NoteNumber);
                    if (layout.TryGetKey(id, out var key))
                    {
                        var difference = note.Time - last;
                        var dotCount = difference / Shorten;

                        sb.Append(new string(Delimiter, (int) dotCount));
                        sb.Append(key.ToString().Last());

                        last = (int) note.Time;
                    }
                }

                sb.AppendLine();
            }

            Result = sb.ToString();
        }
    }
}