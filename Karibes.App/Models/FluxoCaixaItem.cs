using System;

namespace Karibes.App.Models
{
    /// <summary>
    /// Item do fluxo de caixa (entrada ou saída efetiva).
    /// </summary>
    public class FluxoCaixaItem
    {
        /// <summary>Data efetiva do movimento (data de pagamento).</summary>
        public DateTime Data { get; set; }

        /// <summary>Tipo: Entrada ou Saída.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Origem: Venda, PagamentoCliente ou Despesa.</summary>
        public string Origem { get; set; } = string.Empty;

        /// <summary>Valor (sempre positivo).</summary>
        public decimal Valor { get; set; }

        /// <summary>Referência opcional (ex: Nº Venda, nome do cliente).</summary>
        public string? Referencia { get; set; }
    }
}
