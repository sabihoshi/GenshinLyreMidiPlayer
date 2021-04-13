using System;
using System.Globalization;
using System.Windows.Data;

namespace GenshinLyreMidiPlayer.ModernWPF
{
    public class ProductGroupKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((string) value).Substring(0, 1).ToUpper();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}