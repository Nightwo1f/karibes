using Karibes.App.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço de exportação de relatórios para Excel. Apenas formatação e gravação; nenhuma regra de negócio.
    /// </summary>
    public class ExportacaoRelatorioService
    {
        private const string FormatoData = "dd/MM/yyyy";
        private const string FormatoMoeda = "#,##0.00";

        static ExportacaoRelatorioService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Exporta RelatorioFinanceiroConsolidado para arquivo Excel (.xlsx).
        /// Cabeçalho claro; datas e valores formatados.
        /// </summary>
        public void ExportarRelatorioFinanceiro(RelatorioFinanceiroConsolidado relatorio, string caminhoArquivo)
        {
            if (relatorio == null)
                throw new ArgumentNullException(nameof(relatorio));
            if (string.IsNullOrWhiteSpace(caminhoArquivo))
                throw new ArgumentException("Caminho do arquivo é obrigatório.", nameof(caminhoArquivo));

            EnsureDirectory(caminhoArquivo);

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("RelatorioConsolidado");

            // Cabeçalho
            ws.Cells[1, 1].Value = "Relatório Financeiro Consolidado";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[2, 1].Value = $"Período: {relatorio.PeriodoInicio.ToString(FormatoData, CultureInfo.InvariantCulture)} a {relatorio.PeriodoFim.ToString(FormatoData, CultureInfo.InvariantCulture)}";
            ws.Cells[2, 1].Style.Font.Italic = true;

            int row = 4;
            ws.Cells[row, 1].Value = "Campo";
            ws.Cells[row, 2].Value = "Valor";
            ws.Cells[row, 1, row, 2].Style.Font.Bold = true;
            ws.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            row++;

            AddLinha(ws, ref row, "Data Início", relatorio.PeriodoInicio.ToString(FormatoData, CultureInfo.InvariantCulture));
            AddLinha(ws, ref row, "Data Fim", relatorio.PeriodoFim.ToString(FormatoData, CultureInfo.InvariantCulture));
            AddLinhaMoeda(ws, ref row, "Total Vendas", relatorio.TotalVendas);
            AddLinha(ws, ref row, "Quantidade de Vendas", relatorio.QuantidadeVendas.ToString(CultureInfo.InvariantCulture));
            AddLinhaMoeda(ws, ref row, "Total Recebido", relatorio.TotalRecebido);
            AddLinhaMoeda(ws, ref row, "Total a Receber", relatorio.TotalReceber);
            AddLinhaMoeda(ws, ref row, "Total Despesas", relatorio.TotalDespesas);
            AddLinhaMoeda(ws, ref row, "Crédito Concedido", relatorio.TotalCreditoConcedido);
            AddLinhaMoeda(ws, ref row, "Crédito Pago", relatorio.TotalCreditoPago);
            AddLinhaMoeda(ws, ref row, "Saldo Final", relatorio.SaldoFinal);

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            package.SaveAs(new FileInfo(caminhoArquivo));
        }

        /// <summary>
        /// Exporta lista de FluxoCaixaItem para arquivo Excel (.xlsx).
        /// Cabeçalho claro; datas e valores formatados.
        /// </summary>
        public void ExportarFluxoCaixa(IEnumerable<FluxoCaixaItem> itens, string caminhoArquivo)
        {
            if (itens == null)
                throw new ArgumentNullException(nameof(itens));
            if (string.IsNullOrWhiteSpace(caminhoArquivo))
                throw new ArgumentException("Caminho do arquivo é obrigatório.", nameof(caminhoArquivo));

            EnsureDirectory(caminhoArquivo);

            var lista = itens.ToList();
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("FluxoCaixa");

            // Cabeçalho
            ws.Cells[1, 1].Value = "Data";
            ws.Cells[1, 2].Value = "Tipo";
            ws.Cells[1, 3].Value = "Origem";
            ws.Cells[1, 4].Value = "Valor";
            ws.Cells[1, 5].Value = "Referência";
            ws.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            ws.Cells[1, 1, 1, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, 1, 1, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            int row = 2;
            foreach (var item in lista)
            {
                ws.Cells[row, 1].Value = item.Data;
                ws.Cells[row, 1].Style.Numberformat.Format = FormatoData;
                ws.Cells[row, 2].Value = item.Tipo;
                ws.Cells[row, 3].Value = item.Origem;
                ws.Cells[row, 4].Value = (double)item.Valor;
                ws.Cells[row, 4].Style.Numberformat.Format = FormatoMoeda;
                ws.Cells[row, 5].Value = item.Referencia ?? string.Empty;
                row++;
            }

            if (lista.Count > 0)
            {
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }
            package.SaveAs(new FileInfo(caminhoArquivo));
        }

        private static void EnsureDirectory(string caminhoArquivo)
        {
            var dir = Path.GetDirectoryName(caminhoArquivo);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static void AddLinha(ExcelWorksheet ws, ref int row, string campo, string valor)
        {
            ws.Cells[row, 1].Value = campo;
            ws.Cells[row, 2].Value = valor;
            row++;
        }

        private static void AddLinhaMoeda(ExcelWorksheet ws, ref int row, string campo, decimal valor)
        {
            ws.Cells[row, 1].Value = campo;
            ws.Cells[row, 2].Value = (double)valor;
            ws.Cells[row, 2].Style.Numberformat.Format = FormatoMoeda;
            row++;
        }
    }
}
