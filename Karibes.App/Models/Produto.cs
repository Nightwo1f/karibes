using System;

namespace Karibes.App.Models
{
    /// <summary>
    /// Modelo de dados para Produto
    /// </summary>
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public decimal Custo { get; set; }
        public int Estoque { get; set; }
        public int EstoqueMinimo { get; set; }
        public string Unidade { get; set; } = "UN";
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public DateTime DataUltimaAtualizacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
    }
}
