using System.Collections.Generic;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Theme;

public class AppThemes : List<AppTheme>
{
    public AppThemes()
    {
        Add(AppTheme.Light);
        Add(AppTheme.Dark);
        Add(AppTheme.Default);
    }
}