using System;
using System.Collections.Generic;
using System.Linq;
using GenshinLyreMidiPlayer.Data.Entities;
using WindowsInput;
using WindowsInput.Native;
using static GenshinLyreMidiPlayer.WPF.Core.Keyboard;

namespace GenshinLyreMidiPlayer.WPF.Core;

public static class LyrePlayer
{
    private static readonly IInputSimulator Input = new InputSimulator();

    public static int TransposeNote(
        Instrument instrument, ref int noteId,
        Transpose direction = Transpose.Ignore)
    {
        if (direction is Transpose.Ignore) return noteId;
        var notes = GetNotes(instrument);
        while (true)
        {
            if (notes.Contains(noteId))
                return noteId;

            if (noteId < notes.First())
                noteId += 12;
            else if (noteId > notes.Last())
                noteId -= 12;
            else
            {
                return direction switch
                {
                    Transpose.Up   => ++noteId,
                    Transpose.Down => --noteId,
                    _              => noteId
                };
            }
        }
    }

    public static void NoteDown(int noteId, Layout layout, Instrument instrument)
        => InteractNote(noteId, layout, instrument, Input.Keyboard.KeyDown);

    public static void NoteUp(int noteId, Layout layout, Instrument instrument)
        => InteractNote(noteId, layout, instrument, Input.Keyboard.KeyUp);

    public static void PlayNote(int noteId, Layout layout, Instrument instrument)
        => InteractNote(noteId, layout, instrument, Input.Keyboard.KeyPress);

    public static bool TryGetKey(Layout layout, Instrument instrument, int noteId, out VirtualKeyCode key)
    {
        var keys = GetLayout(layout);
        var notes = GetNotes(instrument);
        return TryGetKey(keys, notes, noteId, out key);
    }

    private static bool TryGetKey(
        this IEnumerable<VirtualKeyCode> keys, IList<int> notes,
        int noteId, out VirtualKeyCode key)
    {
        var keyIndex = notes.IndexOf(noteId);
        key = keys.ElementAtOrDefault(keyIndex);

        return keyIndex != -1;
    }

    private static void InteractNote(
        int noteId, Layout layout, Instrument instrument,
        Func<VirtualKeyCode, IKeyboardSimulator> action)
    {
        if (TryGetKey(layout, instrument, noteId, out var key))
            action.Invoke(key);
    }
}