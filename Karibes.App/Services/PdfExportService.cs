using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Karibes.App.Models;

namespace Karibes.App.Services
{
    public class PdfExportService
    {
        public void ExportarProdutos(IEnumerable<Produto> produtos, string caminhoArquivo)
        {
            var linhas = new List<string> { "Codigo | Nome | Categoria | Preco | Estoque" };
            linhas.AddRange(produtos.Select(p =>
                $"{p.Codigo} | {p.Nome} | {p.Categoria} | {p.Preco.ToString("C2", CultureInfo.CurrentCulture)} | {p.Estoque}"));
            WriteSimplePdf("Produtos", linhas, caminhoArquivo);
        }

        public void ExportarClientes(IEnumerable<Cliente> clientes, string caminhoArquivo)
        {
            var linhas = new List<string> { "Codigo | Nome | Telefone | Saldo | Vencimento" };
            linhas.AddRange(clientes.Select(c =>
                $"{c.Codigo} | {c.Nome} | {c.Telefone} | {c.SaldoDevedor.ToString("C2", CultureInfo.CurrentCulture)} | {c.DataVencimentoCredito:dd/MM/yyyy}"));
            WriteSimplePdf("Clientes", linhas, caminhoArquivo);
        }

        public void ExportarRelatorio(RelatorioFinanceiroConsolidado relatorio, IEnumerable<FluxoCaixaItem> fluxo, string caminhoArquivo)
        {
            var linhas = new List<string>
            {
                $"Periodo: {relatorio.PeriodoInicio:dd/MM/yyyy} a {relatorio.PeriodoFim:dd/MM/yyyy}",
                $"Total vendas: {relatorio.TotalVendas:C2}",
                $"Total recebido: {relatorio.TotalRecebido:C2}",
                $"Total a receber: {relatorio.TotalReceber:C2}",
                $"Total despesas: {relatorio.TotalDespesas:C2}",
                $"Saldo final: {relatorio.SaldoFinal:C2}",
                string.Empty,
                "Fluxo de caixa",
                "Data | Tipo | Origem | Valor"
            };
            linhas.AddRange(fluxo.Select(f => $"{f.Data:dd/MM/yyyy} | {f.Tipo} | {f.Origem} | {f.Valor:C2}"));
            WriteSimplePdf("Relatorio Financeiro", linhas, caminhoArquivo);
        }

        private static void WriteSimplePdf(string titulo, IReadOnlyList<string> linhas, string caminhoArquivo)
        {
            var dir = Path.GetDirectoryName(caminhoArquivo);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var content = BuildPageContent(titulo, linhas);
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream"
            };

            using var stream = new FileStream(caminhoArquivo, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.WriteLine("%PDF-1.4");
            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                writer.Flush();
                offsets.Add(stream.Position);
                writer.WriteLine($"{i + 1} 0 obj");
                writer.WriteLine(objects[i]);
                writer.WriteLine("endobj");
            }

            writer.Flush();
            var xref = stream.Position;
            writer.WriteLine("xref");
            writer.WriteLine($"0 {objects.Count + 1}");
            writer.WriteLine("0000000000 65535 f ");
            foreach (var offset in offsets.Skip(1))
                writer.WriteLine($"{offset:0000000000} 00000 n ");
            writer.WriteLine("trailer");
            writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            writer.WriteLine("startxref");
            writer.WriteLine(xref);
            writer.WriteLine("%%EOF");
        }

        private static string BuildPageContent(string titulo, IReadOnlyList<string> linhas)
        {
            var builder = new StringBuilder();
            builder.AppendLine("BT");
            builder.AppendLine("/F1 16 Tf");
            builder.AppendLine("50 800 Td");
            builder.AppendLine($"({Escape(titulo)}) Tj");
            builder.AppendLine("/F1 9 Tf");
            builder.AppendLine("0 -24 Td");
            foreach (var linha in linhas.Take(42))
            {
                builder.AppendLine($"({Escape(linha)}) Tj");
                builder.AppendLine("0 -16 Td");
            }
            builder.AppendLine("ET");
            return builder.ToString();
        }

        private static string Escape(string text)
        {
            var normalized = text
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
            return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(normalized));
        }
    }
}
