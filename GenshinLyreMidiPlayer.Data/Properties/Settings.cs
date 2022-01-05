using System.ComponentModel;
using System.Configuration;

namespace GenshinLyreMidiPlayer.Data.Properties;

// This class allows you to handle specific events on the settings class:
//  The SettingChanging event is raised before a setting's value is changed.
//  The PropertyChanged event is raised after a setting's value is changed.
//  The SettingsLoaded event is raised after the setting values are loaded.
//  The SettingsSaving event is raised before the setting values are saved.
public sealed partial class Settings
{
    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => Save();

    protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
    {
        if (Default.UpgradeRequired)
        {
            Default.Upgrade();
            Default.UpgradeRequired = false;
            Default.Save();
        }
    }
}