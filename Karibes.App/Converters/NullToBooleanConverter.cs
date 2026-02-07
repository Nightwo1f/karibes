using System;
using System.Globalization;
using System.Windows.Data;

namespace Karibes.App.Converters
{
    /// <summary>
    /// Converte null para boolean (null = false, não null = true)
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}





