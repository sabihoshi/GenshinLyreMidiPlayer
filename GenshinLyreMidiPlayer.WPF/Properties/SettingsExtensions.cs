using System;

namespace GenshinLyreMidiPlayer.WPF.Properties
{
    internal static class SettingsExtensions
    {
        public static void Modify(this Settings settings, Action<Settings> action)
        {
            action.Invoke(settings);
            settings.Save();
        }
    }
}