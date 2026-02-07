using System;

namespace Karibes.App.Models
{
    /// <summary>
    /// DTO imutável para relatório financeiro consolidado
    /// </summary>
    public class RelatorioFinanceiroConsolidado
    {
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }
        public decimal TotalVendas { get; set; }
        public decimal TotalRecebido { get; set; }
        public decimal TotalReceber { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal TotalCreditoConcedido { get; set; }
        public decimal TotalCreditoPago { get; set; }
        public decimal SaldoFinal { get; set; }
    }
}

