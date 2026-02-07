using System;
using System.Globalization;
using System.Windows.Data;
using Karibes.App.Models;

namespace Karibes.App.Converters
{
    /// <summary>
    /// Converte Cliente para texto de crédito disponível
    /// </summary>
    public class CreditoDisponivelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Cliente cliente)
            {
                decimal disponivel = cliente.LimiteCredito - cliente.SaldoDevedor;
                return $"Crédito Disponível: R$ {disponivel:N2}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}





