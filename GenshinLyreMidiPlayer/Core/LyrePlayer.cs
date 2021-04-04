using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput;
using Melanchall.DryWetMidi.Core;

namespace GenshinLyreMidiPlayer.Core
{
    public class LyrePlayer
    {
        private static readonly IInputSimulator Input = new InputSimulator();

        private static readonly List<int> LyreNotes = new List<int>
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

        /// <summary>
        ///     Play a MIDI note.
        /// </summary>
        /// <param name="note"> The note to be played.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        /// <param name="keyOffset">Set how much the scale is offset</param>
        /// <param name="selectedLayout"></param>
        public static bool PlayNote(NoteOnEvent note, bool transposeNotes, int keyOffset,
            Keyboard.Layout selectedLayout)
        {
            if (!WindowHelper.IsGameFocused())
                return false;

            var noteId = note.NoteNumber - keyOffset;
            if (!LyreNotes.Contains(noteId))
            {
                if (transposeNotes)
                    noteId = TransposeNote(noteId);
                else
                {
                    Console.WriteLine($"Missing note: {noteId}");
                    return true;
                }
            }

            PlayNote(noteId, selectedLayout);
            return true;
        }

        private static int TransposeNote(int noteId)
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
                    noteId++;
            }
        }

        public static void PlayNote(int noteId, Keyboard.Layout selectedLayout)
        {
            var keyIndex = LyreNotes.IndexOf(noteId);
            var key = Keyboard.GetLayout(selectedLayout)[keyIndex];
            Input.Keyboard.KeyPress(key);
        }
    }
}