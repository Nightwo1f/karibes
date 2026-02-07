using Karibes.App.Models;
using Karibes.App.Utils;
using OfficeOpenXml;
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
        private readonly VendaService _vendaService;
        private readonly FinanceiroService _financeiroService;
        private readonly CreditoService _creditoService;

        public RelatorioFinanceiroService()
        {
            var excelService = new ExcelService();
            _vendaService = new VendaService();
            _financeiroService = new FinanceiroService(excelService);
            _creditoService = new CreditoService();
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
                // Buscar vendas no período
                var vendas = _vendaService.ObterTodas()
                    .Where(v => v.DataVenda >= inicio && v.DataVenda <= fim)
                    .ToList();

                // Calcular TotalVendas
                relatorio.TotalVendas = vendas.Sum(v => v.ValorTotal);

                // Calcular TotalRecebido (vendas à vista finalizadas)
                var vendasAVista = vendas
                    .Where(v => v.FormaPagamento != Constants.PagamentoCredito && 
                                v.FormaPagamento != "Credito" &&
                                v.Status == "Finalizada")
                    .Sum(v => v.ValorTotal);

                // Buscar histórico de crédito para calcular pagamentos no período
                var historicoCredito = ObterHistoricoCreditoPeriodo(inicio, fim);
                var creditosPagos = historicoCredito
                    .Where(h => h.TipoMovimento == "Pagamento")
                    .Sum(h => h.Valor);

                relatorio.TotalRecebido = vendasAVista + creditosPagos;

                // Calcular TotalReceber (vendas a crédito pendentes)
                var vendasCredito = vendas
                    .Where(v => v.FormaPagamento == Constants.PagamentoCredito || v.FormaPagamento == "Credito")
                    .Sum(v => v.ValorTotal);

                // TotalReceber = vendas a crédito - créditos já pagos no período
                relatorio.TotalReceber = vendasCredito - creditosPagos;
                if (relatorio.TotalReceber < 0)
                    relatorio.TotalReceber = 0;

                // Buscar despesas/receitas no período
                var lancamentos = _financeiroService.ObterLancamentos(inicio, fim);
                relatorio.TotalDespesas = lancamentos
                    .Where(l => l.Tipo == Constants.TipoDespesa)
                    .Sum(l => l.Valor);

                // Calcular TotalCreditoConcedido (vendas a crédito no período)
                relatorio.TotalCreditoConcedido = vendasCredito;

                // Calcular TotalCreditoPago (pagamentos no período)
                relatorio.TotalCreditoPago = creditosPagos;

                // Calcular SaldoFinal: (TotalRecebido + Receitas Financeiras) - Despesas
                // TotalRecebido já inclui vendas à vista + créditos pagos
                var receitasFinanceiras = lancamentos
                    .Where(l => l.Tipo == Constants.TipoReceita)
                    .Sum(l => l.Valor);

                // SaldoFinal = (Vendas à vista + Créditos pagos + Receitas financeiras) - Despesas
                relatorio.SaldoFinal = (relatorio.TotalRecebido + receitasFinanceiras) - relatorio.TotalDespesas;
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
        /// Obtém histórico de crédito de todos os clientes no período
        /// </summary>
        private List<HistoricoCredito> ObterHistoricoCreditoPeriodo(DateTime inicio, DateTime fim)
        {
            var historico = new List<HistoricoCredito>();

            // Buscar via Excel diretamente
            var excelService = new ExcelService();
            if (!excelService.FileExists(Constants.HistoricoCreditoFile))
                return historico;

            using var package = excelService.GetPackage(Constants.HistoricoCreditoFile);
            if (package == null) return historico;

            var worksheet = package.Workbook.Worksheets["HistoricoCredito"];
            if (worksheet == null || worksheet.Dimension == null) return historico;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var dataMovimento = worksheet.Cells[row, 3].GetValue<DateTime>();
                    if (dataMovimento >= inicio && dataMovimento <= fim)
                    {
                        var item = new HistoricoCredito
                        {
                            Id = worksheet.Cells[row, 1].GetValue<int>(),
                            ClienteId = worksheet.Cells[row, 2].GetValue<int>(),
                            DataMovimento = dataMovimento,
                            TipoMovimento = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
                            Valor = worksheet.Cells[row, 5].GetValue<decimal>(),
                            SaldoAnterior = worksheet.Cells[row, 6].GetValue<decimal>(),
                            SaldoAtual = worksheet.Cells[row, 7].GetValue<decimal>(),
                            VendaId = worksheet.Cells[row, 8].GetValue<int?>(),
                            Observacoes = worksheet.Cells[row, 9].GetValue<string>() ?? string.Empty
                        };
                        historico.Add(item);
                    }
                }
                catch
                {
                    // Ignora linhas com erro
                }
            }

            return historico;
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

