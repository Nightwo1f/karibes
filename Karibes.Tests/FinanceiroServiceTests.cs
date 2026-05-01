using System.Reflection;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;

namespace Karibes.Tests;

public class FinanceiroServiceTests : IDisposable
{
    private readonly string _dataPath;
    private readonly ExcelService _excelService;
    private readonly FinanceiroService _financeiroService;

    public FinanceiroServiceTests()
    {
        var baseDirectory = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExcelService))!.Location)!;
        _dataPath = Path.Combine(baseDirectory, Constants.DataFolder);
        if (Directory.Exists(_dataPath))
            Directory.Delete(_dataPath, recursive: true);

        _excelService = new ExcelService();
        _financeiroService = new FinanceiroService(_excelService);
    }

    [Fact]
    public void ObterLancamentos_FiltraPorDataLancamentoENaoPorValor()
    {
        _financeiroService.RegistrarReceita(new LancamentoFinanceiro
        {
            Categoria = "Teste",
            Descricao = "Receita de janeiro",
            Valor = 123.45m,
            DataLancamento = new DateTime(2026, 1, 10),
            Status = Constants.StatusPago
        });

        var janeiro = _financeiroService.ObterLancamentos(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31, 23, 59, 59));
        var fevereiro = _financeiroService.ObterLancamentos(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 28, 23, 59, 59));

        Assert.Single(janeiro);
        Assert.Empty(fevereiro);
    }

    [Fact]
    public void AtualizarStatus_GravaStatusNaColunaCorretaEPreservaVencimento()
    {
        var vencimento = new DateTime(2026, 3, 20);
        _financeiroService.RegistrarDespesa(new LancamentoFinanceiro
        {
            Categoria = "Teste",
            Descricao = "Despesa pendente",
            Valor = 50m,
            DataLancamento = new DateTime(2026, 3, 1),
            DataVencimento = vencimento,
            Status = Constants.StatusPendente
        });

        _financeiroService.AtualizarStatus(1, Constants.StatusPago);

        using var package = _excelService.GetPackage(Constants.FinanceiroFile);
        var worksheet = package!.Workbook.Worksheets["Lancamentos"];

        Assert.Equal(vencimento, worksheet.Cells[2, 7].GetValue<DateTime>());
        Assert.NotNull(worksheet.Cells[2, 8].GetValue<DateTime?>());
        Assert.Equal(Constants.StatusPago, worksheet.Cells[2, 9].GetValue<string>());
    }

    [Fact]
    public void RegistrarPagamentoFiado_BaixaSaldoDoClienteEGeraReceitaNoFinanceiro()
    {
        var clienteService = new ClienteService();
        var cliente = new Cliente
        {
            Codigo = "CLI-001",
            Nome = "Cliente Teste",
            TipoDocumento = "CPF",
            Documento = "00000000000",
            LimiteCredito = 500m,
            SaldoDevedor = 200m,
            Ativo = true
        };
        clienteService.Salvar(cliente);

        _financeiroService.RegistrarPagamentoFiado(
            cliente,
            75m,
            "Pagamento teste",
            new DateTime(2026, 4, 5));

        var clienteAtualizado = clienteService.ObterPorId(cliente.Id);
        var lancamentos = _financeiroService.ObterLancamentos(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 30, 23, 59, 59));

        Assert.NotNull(clienteAtualizado);
        Assert.Equal(125m, clienteAtualizado.SaldoDevedor);

        var receita = Assert.Single(lancamentos);
        Assert.Equal(Constants.TipoReceita, receita.Tipo);
        Assert.Equal("Recebimento de Crédito", receita.Categoria);
        Assert.Equal(75m, receita.Valor);
        Assert.Equal(Constants.StatusPago, receita.Status);
        Assert.Equal(new DateTime(2026, 4, 5), receita.DataPagamento);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataPath))
            Directory.Delete(_dataPath, recursive: true);
    }
}
