using System;
using System.Globalization;
using System.Windows.Data;

namespace Karibes.App.Converters
{
    /// <summary>
    /// Converte dois valores decimais subtraindo o segundo do primeiro
    /// </summary>
    public class SubtractConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return 0m;

            try
            {
                decimal valor1 = System.Convert.ToDecimal(values[0]);
                decimal valor2 = System.Convert.ToDecimal(values[1]);
                return valor1 - valor2;
            }
            catch
            {
                return 0m;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

