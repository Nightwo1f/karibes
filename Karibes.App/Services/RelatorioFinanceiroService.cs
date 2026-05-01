using Karibes.App.Data.Repositories;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço centralizador para geração de relatórios financeiros consolidados
    /// </summary>
    public class RelatorioFinanceiroService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IFinanceiroRepository _financeiroRepository;
        private readonly CalculoFinanceiroService _calculoFinanceiro;

        public RelatorioFinanceiroService()
        {
            _vendaRepository = RepositoryFactory.CriarVendaRepository();
            _financeiroRepository = RepositoryFactory.CriarFinanceiroRepository();
            _calculoFinanceiro = new CalculoFinanceiroService();
        }

        /// <summary>
        /// Gera relatório financeiro consolidado para um período
        /// </summary>
        public RelatorioFinanceiroConsolidado GerarRelatorio(DateTime inicio, DateTime fim)
        {
            var relatorio = new RelatorioFinanceiroConsolidado
            {
                PeriodoInicio = inicio,
                PeriodoFim = fim
            };

            try
            {
                var vendas = _vendaRepository.ObterTodas()
                    .Where(v => v.DataVenda >= inicio && v.DataVenda <= fim)
                    .ToList();
                var lancamentos = _financeiroRepository.ObterLancamentos(inicio, fim);
                var historicoCredito = ObterHistoricoCreditoPeriodo(lancamentos);

                _calculoFinanceiro.CalcularResumoFinanceiro(inicio, fim, vendas, lancamentos, historicoCredito, relatorio);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
                // Retorna relatório com valores zerados em caso de erro
            }

            // Aplicar validações e correções
            ValidarECorrigirRelatorio(relatorio);

            return relatorio;
        }

        /// <summary>
        /// Obtém lucro estimado do período (TotalVendas - custo das vendas).
        /// Delega cálculo de custo ao VendaService.
        /// </summary>
        public decimal ObterLucroEstimadoPeriodo(DateTime inicio, DateTime fim)
        {
            try
            {
                var relatorio = GerarRelatorio(inicio, fim);
                var custo = _vendaRepository.ObterCustoTotalVendasPeriodo(inicio, fim);
                return Math.Max(0, relatorio.TotalVendas - custo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao obter lucro estimado: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtém histórico de crédito de todos os clientes no período
        /// </summary>
        private static List<HistoricoCredito> ObterHistoricoCreditoPeriodo(IEnumerable<LancamentoFinanceiro> lancamentos)
        {
            return lancamentos
                .Where(l => l.Origem == "Pagamento de fiado")
                .Select(l => new HistoricoCredito
                {
                    ClienteId = l.OrigemId ?? 0,
                    DataMovimento = l.DataPagamento ?? l.DataLancamento,
                    TipoMovimento = "Pagamento",
                    Valor = l.Valor,
                    Observacoes = l.Observacoes
                })
                .ToList();
        }

        /// <summary>
        /// Valida e corrige valores do relatório internamente
        /// </summary>
        private void ValidarECorrigirRelatorio(RelatorioFinanceiroConsolidado relatorio)
        {
            // Nenhum valor pode ser negativo
            if (relatorio.TotalVendas < 0)
            {
                Debug.WriteLine($"AVISO: TotalVendas negativo detectado ({relatorio.TotalVendas}). Corrigindo para 0.");
                relatorio.TotalVendas = 0;
            }

            if (relatorio.TotalRecebido < 0)
            {
                Debug.WriteLine($"AVISO: TotalRecebido negativo detectado ({relatorio.TotalRecebido}). Corrigindo para 0.");
                relatorio.TotalRecebido = 0;
            }

            if (relatorio.TotalReceber < 0)
            {
                Debug.WriteLine($"AVISO: TotalReceber negativo detectado ({relatorio.TotalReceber}). Corrigindo para 0.");
                relatorio.TotalReceber = 0;
            }

            if (relatorio.TotalDespesas < 0)
            {
                Debug.WriteLine($"AVISO: TotalDespesas negativo detectado ({relatorio.TotalDespesas}). Corrigindo para 0.");
                relatorio.TotalDespesas = 0;
            }

            if (relatorio.TotalCreditoConcedido < 0)
            {
                Debug.WriteLine($"AVISO: TotalCreditoConcedido negativo detectado ({relatorio.TotalCreditoConcedido}). Corrigindo para 0.");
                relatorio.TotalCreditoConcedido = 0;
            }

            if (relatorio.TotalCreditoPago < 0)
            {
                Debug.WriteLine($"AVISO: TotalCreditoPago negativo detectado ({relatorio.TotalCreditoPago}). Corrigindo para 0.");
                relatorio.TotalCreditoPago = 0;
            }

            // TotalRecebido ≤ TotalVendas
            if (relatorio.TotalRecebido > relatorio.TotalVendas)
            {
                Debug.WriteLine($"AVISO: TotalRecebido ({relatorio.TotalRecebido}) > TotalVendas ({relatorio.TotalVendas}). Ajustando TotalRecebido.");
                relatorio.TotalRecebido = relatorio.TotalVendas;
            }

            // TotalCreditoPago ≤ TotalCreditoConcedido
            if (relatorio.TotalCreditoPago > relatorio.TotalCreditoConcedido)
            {
                Debug.WriteLine($"AVISO: TotalCreditoPago ({relatorio.TotalCreditoPago}) > TotalCreditoConcedido ({relatorio.TotalCreditoConcedido}). Ajustando TotalCreditoPago.");
                relatorio.TotalCreditoPago = relatorio.TotalCreditoConcedido;
            }
        }
    }
}

