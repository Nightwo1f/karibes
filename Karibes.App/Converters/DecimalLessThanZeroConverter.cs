using System;
using System.Globalization;
using System.Windows.Data;

namespace Karibes.App.Converters
{
    /// <summary>
    /// Verifica se um decimal é menor que zero
    /// </summary>
    public class DecimalLessThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue < 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}





