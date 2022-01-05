using System;
using System.Globalization;
using System.Windows.Data;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF;

public class TrackGroupKeyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        ((string) value).Substring(0, 1).ToUpper();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}