using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;
using static GenshinLyreMidiPlayer.WPF.Core.Keyboard;

namespace GenshinLyreMidiPlayer.WPF.Core
{
    public static class LyrePlayer
    {
        public enum Transpose
        {
            Ignore,
            Up,
            Down
        }

        private static readonly IInputSimulator Input = new InputSimulator();

        private static readonly List<int> LyreNotes = new()
        {
            48, // C3
            50, // D3
            52, // E3
            53, // F3
            55, // G3
            57, // A3
            59, // B3

            60, // C4
            62, // D4
            64, // E4
            65, // F4
            67, // G4
            69, // A4
            71, // B4

            72, // C5
            74, // D5
            76, // E5
            77, // F5
            79, // G5
            81, // A5
            83  // B5
        };

        public static bool TryGetKey(this Layout layout, int noteId, out VirtualKeyCode key)
        {
            var keys = GetLayout(layout);
            return TryGetKey(keys, noteId, out key);
        }

        public static bool TryGetKey(this IEnumerable<VirtualKeyCode> keys, int noteId, out VirtualKeyCode key)
        {
            var keyIndex = LyreNotes.IndexOf(noteId);
            key = keys.ElementAtOrDefault(keyIndex);

            return keyIndex != -1;
        }

        public static int TransposeNote(int noteId,
            Transpose direction = Transpose.Ignore)
        {
            while (true)
            {
                if (LyreNotes.Contains(noteId))
                    return noteId;

                if (noteId < LyreNotes.First())
                    noteId += 12;
                else if (noteId > LyreNotes.Last())
                    noteId -= 12;
                else
                {
                    return direction switch
                    {
                        Transpose.Ignore => noteId,
                        Transpose.Up     => ++noteId,
                        Transpose.Down   => --noteId
                    };
                }
            }
        }

        public static void InteractNote(int noteId, Layout selectedLayout,
            Func<VirtualKeyCode, IKeyboardSimulator> action)
        {
            if (selectedLayout.TryGetKey(noteId, out var key))
                action.Invoke(key);
        }

        public static void NoteDown(int noteId, Layout selectedLayout)
        {
            InteractNote(noteId, selectedLayout, Input.Keyboard.KeyDown);
        }

        public static void NoteUp(int noteId, Layout selectedLayout)
        {
            InteractNote(noteId, selectedLayout, Input.Keyboard.KeyUp);
        }

        public static void PlayNote(int noteId, Layout selectedLayout)
        {
            InteractNote(noteId, selectedLayout, Input.Keyboard.KeyPress);
        }
    }
}