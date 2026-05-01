using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço único de cálculo financeiro. Contém apenas métodos puros para evitar
    /// duplicação de lógica entre Vendas, Financeiro, Dashboard e Relatórios.
    /// </summary>
    public class CalculoFinanceiroService
    {
        /// <summary>
        /// Calcula o total de uma venda: subtotal dos itens menos desconto.
        /// Se a venda tiver itens carregados, usa a soma dos itens; senão usa ValorTotal já preenchido.
        /// </summary>
        public decimal CalcularTotalVenda(Venda venda)
        {
            if (venda == null) return 0;
            if (venda.Itens != null && venda.Itens.Count > 0)
                return Math.Max(0, CalcularSubtotalItens(venda.Itens) - venda.Desconto);
            return venda.ValorTotal;
        }

        /// <summary>
        /// Soma o ValorTotal de uma lista de itens de venda.
        /// </summary>
        public decimal CalcularSubtotalItens(IEnumerable<ItemVenda>? itens)
        {
            if (itens == null) return 0;
            return itens.Sum(i => i?.ValorTotal ?? 0);
        }

        /// <summary>
        /// Total com desconto aplicado: subtotal - desconto (mínimo zero).
        /// </summary>
        public decimal CalcularTotalComDesconto(decimal subtotal, decimal desconto)
        {
            return Math.Max(0, subtotal - desconto);
        }

        /// <summary>
        /// Retorna o saldo devedor do cliente (fonte única de leitura).
        /// </summary>
        public decimal CalcularSaldoDevedorCliente(Cliente? cliente)
        {
            return cliente?.SaldoDevedor ?? 0;
        }

        /// <summary>
        /// Calcula totalizadores a partir de lançamentos: total receitas, total despesas e saldo do caixa.
        /// Saldo = receitas pagas - despesas pagas.
        /// </summary>
        public (decimal totalReceitas, decimal totalDespesas, decimal saldoCaixa) CalcularTotalizadoresLancamentos(
            IEnumerable<LancamentoFinanceiro>? lancamentos)
        {
            if (lancamentos == null)
                return (0, 0, 0);

            var lista = lancamentos.ToList();
            decimal totalReceitas = lista
                .Where(l => l.Tipo == Constants.TipoReceita)
                .Sum(l => l.Valor);
            decimal totalDespesas = lista
                .Where(l => l.Tipo == Constants.TipoDespesa)
                .Sum(l => l.Valor);

            var (receitasPagas, despesasPagas) = CalcularReceitasEDespesasPagas(lista);
            decimal saldoCaixa = receitasPagas - despesasPagas;

            return (totalReceitas, totalDespesas, saldoCaixa);
        }

        /// <summary>
        /// Calcula o saldo do caixa (receitas pagas - despesas pagas) a partir dos lançamentos.
        /// </summary>
        public decimal CalcularSaldoCaixa(IEnumerable<LancamentoFinanceiro>? lancamentos)
        {
            if (lancamentos == null) return 0;
            var (receitasPagas, despesasPagas) = CalcularReceitasEDespesasPagas(lancamentos);
            return receitasPagas - despesasPagas;
        }

        /// <summary>
        /// Retorna totais de receitas e despesas com status Pago (para balanço/saldo).
        /// </summary>
        public (decimal receitasPagas, decimal despesasPagas) CalcularReceitasEDespesasPagas(
            IEnumerable<LancamentoFinanceiro>? lancamentos)
        {
            if (lancamentos == null) return (0, 0);
            var lista = lancamentos.ToList();
            decimal receitasPagas = lista
                .Where(l => l.Tipo == Constants.TipoReceita && l.Status == Constants.StatusPago)
                .Sum(l => l.Valor);
            decimal despesasPagas = lista
                .Where(l => l.Tipo == Constants.TipoDespesa && l.Status == Constants.StatusPago)
                .Sum(l => l.Valor);
            return (receitasPagas, despesasPagas);
        }

        /// <summary>
        /// Valor proporcional de um item para devolução: (ValorTotal / Quantidade) * quantidadeDevolver.
        /// </summary>
        public decimal CalcularValorProporcionalDevolucao(ItemVenda item, int quantidadeDevolver)
        {
            if (item == null || item.Quantidade <= 0 || quantidadeDevolver <= 0) return 0;
            return (item.ValorTotal / item.Quantidade) * Math.Min(quantidadeDevolver, item.Quantidade);
        }

        /// <summary>
        /// Preenche um resumo financeiro consolidado para o período com base em vendas, lançamentos e histórico de crédito.
        /// Método puro: não acessa persistência.
        /// </summary>
        public void CalcularResumoFinanceiro(
            DateTime inicio,
            DateTime fim,
            IEnumerable<Venda> vendas,
            IEnumerable<LancamentoFinanceiro> lancamentos,
            IEnumerable<HistoricoCredito> historicoCredito,
            RelatorioFinanceiroConsolidado relatorio)
        {
            if (relatorio == null) return;

            var vendasList = (vendas ?? Array.Empty<Venda>()).ToList();
            var lancamentosList = (lancamentos ?? Array.Empty<LancamentoFinanceiro>()).ToList();
            var historicoList = (historicoCredito ?? Array.Empty<HistoricoCredito>()).ToList();

            relatorio.PeriodoInicio = inicio;
            relatorio.PeriodoFim = fim;
            relatorio.QuantidadeVendas = vendasList.Count;
            relatorio.TotalVendas = vendasList.Sum(v => CalcularTotalVenda(v));

            var vendasAVista = vendasList
                .Where(v => v.FormaPagamento != Constants.PagamentoCredito && v.FormaPagamento != "Credito" && v.Status == "Finalizada")
                .Sum(v => CalcularTotalVenda(v));

            var creditosPagos = historicoList
                .Where(h => h.TipoMovimento == "Pagamento")
                .Sum(h => h.Valor);

            relatorio.TotalRecebido = vendasAVista + creditosPagos;

            var vendasCredito = vendasList
                .Where(v => v.FormaPagamento == Constants.PagamentoCredito || v.FormaPagamento == "Credito")
                .Sum(v => CalcularTotalVenda(v));

            relatorio.TotalReceber = Math.Max(0, vendasCredito - creditosPagos);
            relatorio.TotalDespesas = lancamentosList
                .Where(l => l.Tipo == Constants.TipoDespesa)
                .Sum(l => l.Valor);
            relatorio.TotalCreditoConcedido = vendasCredito;
            relatorio.TotalCreditoPago = creditosPagos;

            relatorio.SaldoFinal = CalcularSaldoCaixa(lancamentosList);
        }

        /// <summary>
        /// Soma o total de várias vendas usando CalcularTotalVenda para cada uma.
        /// </summary>
        public decimal CalcularTotalVendas(IEnumerable<Venda>? vendas)
        {
            if (vendas == null) return 0;
            return vendas.Sum(v => CalcularTotalVenda(v));
        }
    }
}
