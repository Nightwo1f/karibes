using System;
using System.Globalization;
using System.Windows.Data;

namespace Karibes.App.Converters
{
    /// <summary>
    /// Converte valor financeiro em largura de barra para gráficos simples (sem cálculo na UI).
    /// Parameter = valor máximo da escala (ex: 100000). Largura máxima = 300.
    /// </summary>
    public class ValueToBarWidthConverter : IValueConverter
    {
        private const double MaxWidth = 300;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal valor = 0;
            if (value is decimal d) valor = d;
            else if (value != null && decimal.TryParse(value.ToString(), out var parsed)) valor = parsed;

            decimal max = 100000;
            if (parameter != null && decimal.TryParse(parameter.ToString(), NumberStyles.Any, culture, out var maxParsed))
                max = maxParsed > 0 ? maxParsed : 100000;

            if (max <= 0) return 0.0;
            var ratio = (double)(valor / max);
            var width = ratio * MaxWidth;
            return Math.Min(MaxWidth, Math.Max(0, width));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
