using System;

namespace Karibes.App.Models
{
    public class LancamentoFinanceiro
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // Receita, Despesa
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataLancamento { get; set; } = DateTime.Now;
        public DateTime? DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = "Pendente"; // Pendente, Pago, Cancelado
        public string FormaPagamento { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty; // Venda, Manual, etc
        public int? OrigemId { get; set; } // ID da origem (ex: VendaId)
        public string Observacoes { get; set; } = string.Empty;
    }
}

