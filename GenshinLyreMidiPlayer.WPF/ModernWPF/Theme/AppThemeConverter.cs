using System;
using System.Globalization;
using System.Windows.Data;
using ModernWpf;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Theme;

public class AppThemeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        ApplicationTheme.Light => AppTheme.Light,
        ApplicationTheme.Dark  => AppTheme.Dark,
        _                      => AppTheme.Default
    };

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppTheme appTheme)
            return appTheme.Value;

        return AppTheme.Default;
    }
}