using System;

namespace Karibes.App.Models
{
    /// <summary>
    /// Modelo para histórico de crédito do cliente
    /// </summary>
    public class HistoricoCredito
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime DataMovimento { get; set; } = DateTime.Now;
        public string TipoMovimento { get; set; } = string.Empty; // Compra, Pagamento, Ajuste
        public decimal Valor { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoAtual { get; set; }
        public int? VendaId { get; set; } // ID da venda relacionada (se aplicável)
        public string Observacoes { get; set; } = string.Empty;
    }
}

