using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GenshinLyreMidiPlayer.Core
{
    public static class WindowHelper
    {
        public const string GenshinProcessName = "GenshinImpact";

        private static IntPtr? FindWindowByProcessName(string processName)
        {
            var process = Process.GetProcessesByName(processName);
            return process.FirstOrDefault()?.MainWindowHandle;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fUnknown);

        public static bool EnsureGameOnTop()
        {
            var genshinWindow = FindWindowByProcessName(GenshinProcessName);

            if (genshinWindow is null)
                return false;

            SwitchToThisWindow((IntPtr) genshinWindow, true);

            return !genshinWindow.Equals(IntPtr.Zero) &&
                   GetForegroundWindow().Equals(genshinWindow);
        }

        private static bool IsWindowFocused(IntPtr windowPtr)
        {
            var hWnd = GetForegroundWindow();
            return hWnd.Equals(windowPtr);
        }

        public static bool IsGameFocused()
        {
            var genshinWindow = FindWindowByProcessName(GenshinProcessName);
            return genshinWindow != null &&
                   IsWindowFocused((IntPtr) genshinWindow);
        }
    }
}