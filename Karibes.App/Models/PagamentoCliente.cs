using System;

namespace Karibes.App.Models
{
    /// <summary>
    /// Modelo para registro de pagamentos de clientes
    /// </summary>
    public class PagamentoCliente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public decimal ValorPago { get; set; }
        public DateTime DataPagamento { get; set; } = DateTime.Now;
        public string Observacao { get; set; } = string.Empty;
    }
}

