using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Karibes.App.Converters
{
    /// <summary>
    /// Converte null para Visibility (null = Collapsed, não null = Visible)
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}





