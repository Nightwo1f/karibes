using Karibes.App.Data.Repositories;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço de Fluxo de Caixa (entradas e saídas efetivas por data de pagamento).
    /// Usa dados existentes do FinanceiroService e CalculoFinanceiroService.
    /// </summary>
    public class FluxoCaixaService
    {
        private readonly IFinanceiroRepository _financeiroRepository;
        private readonly CalculoFinanceiroService _calculoFinanceiro;

        private const string TipoEntrada = "Entrada";
        private const string TipoSaida = "Saída";
        private const string OrigemVenda = "Venda";
        private const string OrigemPagamentoCliente = "PagamentoCliente";
        private const string OrigemDespesa = "Despesa";
        private const string CategoriaVendas = "Vendas";
        private const string CategoriaRecebimentoCredito = "Recebimento de Crédito";

        public FluxoCaixaService()
        {
            _financeiroRepository = RepositoryFactory.CriarFinanceiroRepository();
            _calculoFinanceiro = new CalculoFinanceiroService();
        }

        /// <summary>
        /// Gera o fluxo de caixa do período: entradas (vendas e pagamentos de clientes) e saídas (despesas pagas).
        /// Datas reais de pagamento; itens ordenados por data; nenhum valor negativo.
        /// </summary>
        /// <param name="inicio">Data inicial (inclusive)</param>
        /// <param name="fim">Data final (inclusive)</param>
        /// <returns>Itens do fluxo ordenados por data</returns>
        public IEnumerable<FluxoCaixaItem> GerarFluxoCaixa(DateTime inicio, DateTime fim)
        {
            var lancamentos = _financeiroRepository.ObterLancamentosComPagamentoNoPeriodo(inicio, fim);
            var itens = new List<FluxoCaixaItem>();

            foreach (var lancamento in lancamentos)
            {
                var valor = Math.Max(0, lancamento.Valor);
                if (valor == 0) continue;

                var dataPagamento = lancamento.DataPagamento ?? lancamento.DataLancamento;
                var referencia = ObterReferencia(lancamento);

                if (lancamento.Tipo == Constants.TipoReceita)
                {
                    var origem = string.Equals(lancamento.Categoria, CategoriaVendas, StringComparison.OrdinalIgnoreCase)
                        ? OrigemVenda
                        : (string.Equals(lancamento.Categoria, CategoriaRecebimentoCredito, StringComparison.OrdinalIgnoreCase)
                            ? OrigemPagamentoCliente
                            : OrigemVenda); // outras receitas (ex.: Troca) como Venda para simplificar

                    itens.Add(new FluxoCaixaItem
                    {
                        Data = dataPagamento,
                        Tipo = TipoEntrada,
                        Origem = origem,
                        Valor = valor,
                        Referencia = referencia
                    });
                }
                else if (lancamento.Tipo == Constants.TipoDespesa)
                {
                    itens.Add(new FluxoCaixaItem
                    {
                        Data = dataPagamento,
                        Tipo = TipoSaida,
                        Origem = OrigemDespesa,
                        Valor = valor,
                        Referencia = referencia
                    });
                }
            }

            // Ordenar por data
            itens = itens.OrderBy(i => i.Data).ToList();

            // Validação: soma entradas − saídas = saldo do período (usando CalculoFinanceiroService)
            var totalEntradas = itens.Where(i => i.Tipo == TipoEntrada).Sum(i => i.Valor);
            var totalSaidas = itens.Where(i => i.Tipo == TipoSaida).Sum(i => i.Valor);
            var saldoCalculado = _calculoFinanceiro.CalcularSaldoCaixa(lancamentos);
            var saldoItens = totalEntradas - totalSaidas;

            if (Math.Abs(saldoItens - saldoCalculado) > 0.001m)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"FluxoCaixaService: saldo dos itens ({saldoItens:N2}) difere do saldo calculado ({saldoCalculado:N2}).");
            }

            return itens;
        }

        /// <summary>
        /// Extrai referência legível do lançamento (ex.: Nº venda, cliente).
        /// </summary>
        private static string? ObterReferencia(LancamentoFinanceiro lancamento)
        {
            if (!string.IsNullOrWhiteSpace(lancamento.Descricao))
                return lancamento.Descricao.Length > 80 ? lancamento.Descricao.Substring(0, 80) : lancamento.Descricao;
            if (lancamento.OrigemId.HasValue && lancamento.OrigemId.Value > 0)
                return lancamento.OrigemId.ToString();
            return null;
        }
    }
}
