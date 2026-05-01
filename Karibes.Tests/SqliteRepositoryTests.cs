using Karibes.App.Data.Repositories;
using Karibes.App.Data.Sqlite;
using Karibes.App.Models;
using Karibes.App.Utils;
using Microsoft.Data.Sqlite;

namespace Karibes.Tests;

public class SqliteRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly IVendaRepository _vendaRepository;

    public SqliteRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"karibes-tests-{Guid.NewGuid():N}.db");
        _connectionFactory = new SqliteConnectionFactory(_databasePath);
        new SqliteDatabaseInitializer(_connectionFactory).Initialize();

        _produtoRepository = new SqliteProdutoRepository(_connectionFactory);
        _clienteRepository = new SqliteClienteRepository(_connectionFactory);
        _financeiroRepository = new SqliteFinanceiroRepository(_connectionFactory);
        _vendaRepository = new SqliteVendaRepository(
            _connectionFactory,
            _produtoRepository,
            _clienteRepository,
            _financeiroRepository);
    }

    [Fact]
    public void ProdutoRepository_CriaAtualizaEConsultaProduto()
    {
        var produto = CriarProduto("P-001", estoque: 10);

        _produtoRepository.Criar(produto);
        produto.Preco = 25m;
        produto.Estoque = 7;
        _produtoRepository.Atualizar(produto);

        var salvo = _produtoRepository.ObterPorCodigo("P-001");

        Assert.NotNull(salvo);
        Assert.Equal(25m, salvo.Preco);
        Assert.Equal(7, salvo.Estoque);
    }

    [Fact]
    public void VendaAVista_BaixaEstoqueEGeraReceita()
    {
        var produto = CriarProduto("P-002", estoque: 5);
        _produtoRepository.Criar(produto);

        var venda = new Venda
        {
            NumeroVenda = "V-001",
            FormaPagamento = Constants.PagamentoDinheiro,
            ValorSubtotal = 30m,
            ValorTotal = 30m,
            Itens =
            [
                new ItemVenda
                {
                    ProdutoId = produto.Id,
                    Produto = produto,
                    Quantidade = 2,
                    PrecoUnitario = 15m
                }
            ]
        };

        _vendaRepository.RegistrarVendaAVista(venda);

        var produtoAtualizado = _produtoRepository.ObterPorId(produto.Id);
        var lancamentos = _financeiroRepository.ObterLancamentos(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

        Assert.NotNull(produtoAtualizado);
        Assert.Equal(3, produtoAtualizado.Estoque);
        var receita = Assert.Single(lancamentos);
        Assert.Equal(Constants.TipoReceita, receita.Tipo);
        Assert.Equal("Venda", receita.Origem);
        Assert.Equal(venda.Id, receita.OrigemId);
    }

    [Fact]
    public void PagamentoFiado_BaixaSaldoClienteEGeraLancamentoPago()
    {
        var cliente = new Cliente
        {
            Codigo = "C-001",
            Nome = "Cliente SQLite",
            TipoDocumento = "CPF",
            Documento = "00000000000",
            CEP = "01000-000",
            Endereco = "Rua Teste",
            Numero = "123",
            Complemento = "Sala 1",
            Bairro = "Centro",
            Cidade = "Sao Paulo",
            Estado = "SP",
            LimiteCredito = 500m,
            SaldoDevedor = 120m,
            Ativo = true
        };
        _clienteRepository.Salvar(cliente);

        _financeiroRepository.RegistrarPagamentoFiado(
            cliente,
            50m,
            "Pagamento teste",
            new DateTime(2026, 5, 1));

        var clienteAtualizado = _clienteRepository.ObterPorId(cliente.Id);
        var lancamentos = _financeiroRepository.ObterLancamentos(
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 1, 23, 59, 59));

        Assert.NotNull(clienteAtualizado);
        Assert.Equal(70m, clienteAtualizado.SaldoDevedor);
        Assert.Equal(50m, clienteAtualizado.TotalPago);
        Assert.Equal("Rua Teste", clienteAtualizado.Endereco);
        Assert.Equal("SP", clienteAtualizado.Estado);
        var lancamento = Assert.Single(lancamentos);
        Assert.Equal("Pagamento de fiado", lancamento.Origem);
        Assert.Equal(cliente.Id, lancamento.OrigemId);
        Assert.Equal(Constants.StatusPago, lancamento.Status);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    private static Produto CriarProduto(string codigo, int estoque)
    {
        return new Produto
        {
            Codigo = codigo,
            Nome = $"Produto {codigo}",
            Preco = 15m,
            Custo = 8m,
            Estoque = estoque,
            EstoqueMinimo = 1,
            Unidade = "UN",
            Ativo = true,
            DataCadastro = DateTime.Now,
            DataUltimaAtualizacao = DateTime.Now
        };
    }
}
