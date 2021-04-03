using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;
using Melanchall.DryWetMidi.Core;

namespace GenshinLyreMidiPlayer
{
    public class LyrePlayer
    {
        public const string GenshinWindowName = "Genshin Impact";

        private static readonly Dictionary<int, VirtualKeyCode> LyreNotes = new Dictionary<int, VirtualKeyCode>
        {
            {48, VirtualKeyCode.VK_Z}, // C3
            {50, VirtualKeyCode.VK_X}, // D3
            {52, VirtualKeyCode.VK_C}, // E3
            {53, VirtualKeyCode.VK_V}, // F3
            {55, VirtualKeyCode.VK_B}, // G3
            {57, VirtualKeyCode.VK_N}, // A3
            {59, VirtualKeyCode.VK_M}, // B3

            {60, VirtualKeyCode.VK_A}, // C4
            {62, VirtualKeyCode.VK_S}, // D4
            {64, VirtualKeyCode.VK_D}, // E4
            {65, VirtualKeyCode.VK_F}, // F4
            {67, VirtualKeyCode.VK_G}, // G4
            {69, VirtualKeyCode.VK_H}, // A4
            {71, VirtualKeyCode.VK_J}, // B4

            {72, VirtualKeyCode.VK_Q}, // C5
            {74, VirtualKeyCode.VK_W}, // D5
            {76, VirtualKeyCode.VK_E}, // E5
            {77, VirtualKeyCode.VK_R}, // F5
            {79, VirtualKeyCode.VK_T}, // G5
            {81, VirtualKeyCode.VK_Y}, // A5
            {83, VirtualKeyCode.VK_U}  // B5
        };

        private static readonly IInputSimulator Input = new InputSimulator();

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        public static IntPtr FindWindow(string lpWindowName)
        {
            return FindWindow(null, lpWindowName);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fUnknown);

        /// <summary>
        ///     Play a MIDI note.
        /// </summary>
        /// <param name="note"> The note to be played.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        /// <param name="keyOffset">Set how much the scale is offset</param>
        public static bool PlayNote(NoteOnEvent note, bool transposeNotes, int keyOffset)
        {
            if (!IsWindowFocused(GenshinWindowName))
                return false;

            var noteId = note.NoteNumber - keyOffset;
            if (!LyreNotes.ContainsKey(noteId))
            {
                if (transposeNotes)
                    noteId = TransposeNote(noteId);
                else
                {
                    Console.WriteLine($"Missing note: {noteId}");
                    return true;
                }
            }

            PlayNote(noteId);
            return true;
        }

        private static int TransposeNote(int noteId)
        {
            while (true)
            {
                if (LyreNotes.ContainsKey(noteId))
                    return noteId;

                if (noteId < LyreNotes.Keys.First())
                    noteId += 12;
                else if (noteId > LyreNotes.Keys.Last())
                    noteId -= 12;
                else
                    noteId++;
            }
        }

        public static void PlayNote(int noteId)
        {
            var key = LyreNotes[noteId];
            Input.Keyboard.KeyPress(key);
        }

        public static bool EnsureWindowOnTop()
        {
            var genshinWindow = FindWindow(GenshinWindowName);
            SwitchToThisWindow(genshinWindow, true);

            return !genshinWindow.Equals(IntPtr.Zero) &&
                   GetForegroundWindow().Equals(genshinWindow);
        }

        public static bool IsWindowFocused(IntPtr windowPtr)
        {
            var hWnd = GetForegroundWindow();
            return hWnd.Equals(windowPtr)
                   && !windowPtr.Equals(IntPtr.Zero);
        }

        public static bool IsWindowFocused(string windowName)
        {
            var windowPtr = FindWindow(windowName);
            return IsWindowFocused(windowPtr);
        }
    }
}