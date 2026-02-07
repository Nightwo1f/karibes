using OfficeOpenXml;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço para agregar dados do Dashboard
    /// </summary>
    public class DashboardService
    {
        private readonly ExcelService _excelService;
        private readonly ProdutoService _produtoService;

        public DashboardService()
        {
            _excelService = new ExcelService();
            _produtoService = new ProdutoService();
        }

        /// <summary>
        /// Obtém o total de vendas do dia atual
        /// </summary>
        public (decimal valorTotal, int quantidade) ObterVendasDoDia()
        {
            var hoje = DateTime.Now.Date;
            var vendas = ObterVendasPorPeriodo(hoje, hoje.AddDays(1).AddSeconds(-1));
            
            decimal valorTotal = vendas.Sum(v => v.ValorTotal);
            int quantidade = vendas.Count;

            return (valorTotal, quantidade);
        }

        /// <summary>
        /// Obtém o total de vendas do mês atual
        /// </summary>
        public (decimal valorTotal, int quantidade) ObterVendasDoMes()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddSeconds(-1);
            var vendas = ObterVendasPorPeriodo(inicioMes, fimMes);
            
            decimal valorTotal = vendas.Sum(v => v.ValorTotal);
            int quantidade = vendas.Count;

            return (valorTotal, quantidade);
        }

        /// <summary>
        /// Obtém produtos com estoque crítico
        /// </summary>
        public List<Produto> ObterProdutosEstoqueCritico()
        {
            return _produtoService.ObterProdutosEstoqueCritico();
        }

        /// <summary>
        /// Calcula o lucro estimado do mês
        /// </summary>
        public decimal CalcularLucroEstimadoMes()
        {
            try
            {
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fimMes = inicioMes.AddMonths(1).AddSeconds(-1);
                var vendas = ObterVendasPorPeriodo(inicioMes, fimMes);

                decimal receitaTotal = vendas.Sum(v => v.ValorTotal);
                decimal custoTotal = CalcularCustoTotalVendas(vendas);

                return receitaTotal - custoTotal;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Obtém vendas por período
        /// </summary>
        private List<Venda> ObterVendasPorPeriodo(DateTime dataInicio, DateTime dataFim)
        {
            var vendas = new List<Venda>();

            if (!_excelService.FileExists(Constants.VendasFile))
                return vendas;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return vendas;

            var worksheet = package.Workbook.Worksheets["Vendas"];
            if (worksheet == null || worksheet.Dimension == null) 
                return vendas;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var status = worksheet.Cells[row, 9].GetValue<string>();
                    if (status != "Finalizada") continue;

                    var dataVenda = worksheet.Cells[row, 4].GetValue<DateTime>();
                    if (dataVenda < dataInicio || dataVenda > dataFim) continue;

                    var venda = new Venda
                    {
                        Id = worksheet.Cells[row, 1].GetValue<int>(),
                        ClienteId = worksheet.Cells[row, 3].GetValue<int?>() ?? 0,
                        DataVenda = dataVenda,
                        ValorTotal = worksheet.Cells[row, 7].GetValue<decimal>(),
                        Desconto = worksheet.Cells[row, 6].GetValue<decimal>(),
                        FormaPagamento = worksheet.Cells[row, 8].GetValue<string>() ?? string.Empty,
                        Status = status ?? "Pendente"
                    };

                    vendas.Add(venda);
                }
                catch
                {
                    // Ignora linhas com erro
                    continue;
                }
            }

            return vendas;
        }

        /// <summary>
        /// Calcula o custo total das vendas (soma dos custos dos produtos vendidos)
        /// </summary>
        private decimal CalcularCustoTotalVendas(List<Venda> vendas)
        {
            decimal custoTotal = 0;

            if (!_excelService.FileExists(Constants.VendasFile))
                return custoTotal;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return custoTotal;

            var itensWorksheet = package.Workbook.Worksheets["ItensVenda"];
            if (itensWorksheet == null || itensWorksheet.Dimension == null) return custoTotal;

            var vendaIds = vendas.Select(v => v.Id).ToList();
            int rowCount = itensWorksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var vendaId = itensWorksheet.Cells[row, 2].GetValue<int>();
                    if (!vendaIds.Contains(vendaId)) continue;

                    var produtoId = itensWorksheet.Cells[row, 3].GetValue<int>();
                    var quantidade = itensWorksheet.Cells[row, 4].GetValue<int>();

                    // Obtém o custo do produto
                    var produto = _produtoService.ObterPorId(produtoId);
                    if (produto != null)
                    {
                        custoTotal += produto.Custo * quantidade;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return custoTotal;
        }
    }
}

