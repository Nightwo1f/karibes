using System;
using System.Globalization;
using System.Windows.Data;

namespace Karibes.App.Converters
{
    public class ZeroToEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                decimal decimalValue when decimalValue == 0 => string.Empty,
                int intValue when intValue == 0 => string.Empty,
                double doubleValue when Math.Abs(doubleValue) < double.Epsilon => string.Empty,
                _ => value
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return targetType == typeof(int) ? 0 : 0m;

            if (targetType == typeof(int) || targetType == typeof(int?))
                return int.TryParse(text, NumberStyles.Integer, culture, out var intValue) ? intValue : 0;

            return decimal.TryParse(text, NumberStyles.Number, culture, out var decimalValue) ? decimalValue : 0m;
        }
    }
}
