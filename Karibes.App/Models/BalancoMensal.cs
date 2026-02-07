using System;

namespace Karibes.App.Models
{
    public class BalancoMensal
    {
        public int Id { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public decimal ReceitaTotal { get; set; }
        public decimal DespesaTotal { get; set; }
        public decimal Lucro { get; set; }
        public int TotalVendas { get; set; }
        public DateTime DataGeracao { get; set; } = DateTime.Now;
    }
}

