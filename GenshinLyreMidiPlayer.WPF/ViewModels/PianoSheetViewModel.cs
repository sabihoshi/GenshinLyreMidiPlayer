using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenshinLyreMidiPlayer.WPF.Core;
using Melanchall.DryWetMidi.Interaction;
using PropertyChanged;
using Stylet;

namespace GenshinLyreMidiPlayer.WPF.ViewModels;

public class PianoSheetViewModel : Screen
{
    private readonly MainWindowViewModel _main;
    private uint _bars = 1;
    private uint _beats;
    private uint _shorten = 1;

    public PianoSheetViewModel(MainWindowViewModel main) { _main = main; }

    [OnChangedMethod(nameof(Update))] public char Delimiter { get; set; } = '.';

    [OnChangedMethod(nameof(Update))]
    public KeyValuePair<Keyboard.Layout, string> SelectedLayout
    {
        get => SettingsPage.SelectedLayout;
        set => SettingsPage.SelectedLayout = value;
    }

    public PlaylistViewModel PlaylistView => _main.PlaylistView;

    public SettingsPageViewModel SettingsPage => _main.SettingsView;

    public string Result { get; private set; } = string.Empty;

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
                var offset = note.NoteNumber - SettingsPage.KeyOffset;
                var transpose = SettingsPage.Transpose.Key;
                var id = LyrePlayer.TransposeNote(offset, transpose);

                if (!layout.TryGetKey(id, out var key)) continue;

                var difference = note.Time - last;
                var dotCount = difference / Shorten;

                sb.Append(new string(Delimiter, (int) dotCount));
                sb.Append(key.ToString().Last());

                last = (int) note.Time;
            }

            sb.AppendLine();
        }

        Result = sb.ToString();
    }

    protected override void OnActivate() => Update();
}