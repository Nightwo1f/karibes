using System;
using System.Collections.Generic;

namespace Karibes.App.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty; // CPF ou CNPJ
        public string Documento { get; set; } = string.Empty; // CPF ou CNPJ
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public decimal SaldoDevedor { get; set; }
        public decimal TotalPago { get; set; }
        public DateTime? DataVencimentoCredito { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public DateTime DataUltimaAtualizacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
        public string Observacoes { get; set; } = string.Empty;
        public List<PagamentoCliente> Pagamentos { get; set; } = new List<PagamentoCliente>();

        public bool EstaDevendo => SaldoDevedor > 0;
        public bool DividaVencida => EstaDevendo && DataVencimentoCredito.HasValue && DataVencimentoCredito.Value.Date < DateTime.Today;
        public bool DividaProximaVencimento => EstaDevendo &&
                                               DataVencimentoCredito.HasValue &&
                                               DataVencimentoCredito.Value.Date >= DateTime.Today &&
                                               DataVencimentoCredito.Value.Date <= DateTime.Today.AddDays(3);
    }
}
