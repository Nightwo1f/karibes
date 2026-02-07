using System;
using System.Collections.Generic;

namespace Karibes.App.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public string NumeroVenda { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime DataVenda { get; set; } = DateTime.Now;
        public decimal ValorSubtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal ValorTotal { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendente";
        public string Vendedor { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    }
}

