using System;

namespace Karibes.App.Models
{
    public class MovimentoEstoque
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }
        public string Tipo { get; set; } = string.Empty; // Entrada, Saída
        public int Quantidade { get; set; }
        public DateTime DataMovimento { get; set; } = DateTime.Now;
        public string Motivo { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
    }
}

