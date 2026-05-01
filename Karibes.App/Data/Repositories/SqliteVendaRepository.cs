using System;
using System.Collections.Generic;
using System.Linq;
using Karibes.App.Data.Sqlite;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;
using Microsoft.Data.Sqlite;

namespace Karibes.App.Data.Repositories
{
    public class SqliteVendaRepository : IVendaRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IFinanceiroRepository _financeiroRepository;
        private readonly CalculoFinanceiroService _calculoFinanceiro = new();

        public SqliteVendaRepository(
            SqliteConnectionFactory connectionFactory,
            IProdutoRepository produtoRepository,
            IClienteRepository clienteRepository,
            IFinanceiroRepository financeiroRepository)
        {
            _connectionFactory = connectionFactory;
            _produtoRepository = produtoRepository;
            _clienteRepository = clienteRepository;
            _financeiroRepository = financeiroRepository;
        }

        public void RegistrarVendaAVista(Venda venda)
        {
            ValidarVenda(venda);
            if (venda.FormaPagamento != Constants.PagamentoDinheiro &&
                venda.FormaPagamento != Constants.PagamentoCartao &&
                venda.FormaPagamento != Constants.PagamentoPix)
                throw new ArgumentException("Forma de pagamento inválida para venda à vista.");

            ValidarEstoqueDisponivel(venda.Itens);
            venda.Status = "Finalizada";
            venda.DataVenda = DateTime.Now;
            if (string.IsNullOrWhiteSpace(venda.NumeroVenda))
                venda.NumeroVenda = GerarNumeroVenda();

            SalvarVendaComItens(venda);
            BaixarEstoque(venda.Itens);

            _financeiroRepository.RegistrarReceita(new LancamentoFinanceiro
            {
                Categoria = "Vendas",
                Descricao = $"Venda {venda.NumeroVenda} - {venda.FormaPagamento}",
                Valor = venda.ValorTotal,
                DataLancamento = venda.DataVenda,
                DataPagamento = venda.DataVenda,
                Status = Constants.StatusPago,
                FormaPagamento = venda.FormaPagamento,
                Origem = "Venda",
                OrigemId = venda.Id,
                Observacoes = $"Venda à vista - {venda.NumeroVenda}"
            });
        }

        public void RegistrarVendaFiada(Venda venda)
        {
            ValidarVenda(venda);
            if (venda.FormaPagamento != Constants.PagamentoCredito && venda.FormaPagamento != "Credito")
                throw new ArgumentException("Forma de pagamento deve ser Crédito para venda fiada.");
            if (venda.Cliente == null)
                throw new ArgumentException("Cliente é obrigatório para venda fiada.");
            if (venda.Cliente.SaldoDevedor + venda.ValorTotal > venda.Cliente.LimiteCredito)
                throw new InvalidOperationException("Cliente não pode comprar no crédito. Limite insuficiente.");

            ValidarEstoqueDisponivel(venda.Itens);
            venda.Status = "Pendente";
            venda.DataVenda = DateTime.Now;
            if (string.IsNullOrWhiteSpace(venda.NumeroVenda))
                venda.NumeroVenda = GerarNumeroVenda();

            SalvarVendaComItens(venda);
            BaixarEstoque(venda.Itens);

              venda.Cliente.SaldoDevedor += venda.ValorTotal;
              venda.Cliente.DataVencimentoCredito ??= venda.DataVenda.Date.AddDays(30);
              venda.Cliente.DataUltimaAtualizacao = DateTime.Now;
              _clienteRepository.Salvar(venda.Cliente);
        }

        public List<Venda> ObterTodas()
        {
            var vendas = new List<Venda>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Vendas ORDER BY DataVenda DESC";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                vendas.Add(ReadVenda(reader));
            return vendas;
        }

        public List<Venda> ObterVendasPorCliente(int clienteId)
        {
            return ObterTodas().Where(v => v.ClienteId == clienteId).ToList();
        }

        public Venda? ObterPorId(int id)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Vendas WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadVenda(reader) : null;
        }

        public Venda? ObterPorNumeroVenda(string numeroVenda)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Vendas WHERE NumeroVenda = $numero";
            command.Parameters.AddWithValue("$numero", numeroVenda ?? string.Empty);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadVenda(reader) : null;
        }

        public void CancelarVenda(int vendaId, string motivo = "")
        {
            var venda = ObterPorId(vendaId) ?? throw new KeyNotFoundException($"Venda com ID {vendaId} não encontrada.");
            if (venda.Status == "Cancelada")
                throw new InvalidOperationException("Venda já está cancelada.");

            foreach (var item in venda.Itens)
            {
                var produto = _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto == null) continue;
                produto.Estoque += item.Quantidade;
                _produtoRepository.Atualizar(produto);
            }

            if (venda.Status == "Finalizada" && venda.FormaPagamento != Constants.PagamentoCredito)
            {
                _financeiroRepository.RegistrarDespesa(new LancamentoFinanceiro
                {
                    Categoria = "Estorno",
                    Descricao = $"Estorno - Venda cancelada {venda.NumeroVenda}",
                    Valor = venda.ValorTotal,
                    DataLancamento = DateTime.Now,
                    DataPagamento = DateTime.Now,
                    Status = Constants.StatusPago,
                    FormaPagamento = venda.FormaPagamento,
                    Origem = "Cancelamento de Venda",
                    OrigemId = venda.Id,
                    Observacoes = $"Estorno da venda {venda.NumeroVenda}. Motivo: {motivo}"
                });
            }

            if ((venda.FormaPagamento == Constants.PagamentoCredito || venda.FormaPagamento == "Credito") && venda.Cliente != null)
            {
                  venda.Cliente.SaldoDevedor = Math.Max(0, venda.Cliente.SaldoDevedor - venda.ValorTotal);
                  if (venda.Cliente.SaldoDevedor == 0)
                      venda.Cliente.DataVencimentoCredito = null;
                  _clienteRepository.Salvar(venda.Cliente);
              }

            AtualizarStatusVenda(vendaId, "Cancelada");
        }

        public void DevolverVenda(int vendaId, Dictionary<int, int> itensDevolver, string observacao = "")
        {
            var venda = ObterPorId(vendaId) ?? throw new KeyNotFoundException($"Venda com ID {vendaId} não encontrada.");
            if (venda.Status == "Cancelada")
                throw new InvalidOperationException("Não é possível devolver uma venda cancelada.");

            decimal valorDevolvido = 0;
            foreach (var itemDevolver in itensDevolver)
            {
                var item = venda.Itens.FirstOrDefault(i => i.Id == itemDevolver.Key);
                if (item == null) continue;
                var quantidade = Math.Min(itemDevolver.Value, item.Quantidade);
                if (quantidade <= 0) continue;

                var produto = _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto != null)
                {
                    produto.Estoque += quantidade;
                    _produtoRepository.Atualizar(produto);
                }

                valorDevolvido += _calculoFinanceiro.CalcularValorProporcionalDevolucao(item, quantidade);
            }

            if (valorDevolvido <= 0)
                throw new InvalidOperationException("Nenhum item válido para devolução.");

            if (venda.Status == "Finalizada" && venda.FormaPagamento != Constants.PagamentoCredito)
            {
                _financeiroRepository.RegistrarDespesa(new LancamentoFinanceiro
                {
                    Categoria = "Devolução",
                    Descricao = $"Devolução - Venda {venda.NumeroVenda}",
                    Valor = valorDevolvido,
                    DataLancamento = DateTime.Now,
                    DataPagamento = DateTime.Now,
                    Status = Constants.StatusPago,
                    FormaPagamento = venda.FormaPagamento,
                    Origem = "Devolução de Venda",
                    OrigemId = venda.Id,
                    Observacoes = string.IsNullOrWhiteSpace(observacao)
                        ? $"Devolução parcial da venda {venda.NumeroVenda}"
                        : observacao
                });
            }

            if ((venda.FormaPagamento == Constants.PagamentoCredito || venda.FormaPagamento == "Credito") && venda.Cliente != null)
            {
                venda.Cliente.SaldoDevedor = Math.Max(0, venda.Cliente.SaldoDevedor - valorDevolvido);
                if (venda.Cliente.SaldoDevedor == 0)
                    venda.Cliente.DataVencimentoCredito = null;
                _clienteRepository.Salvar(venda.Cliente);
            }
        }

        public void TrocarVenda(int vendaId, Dictionary<int, int> itensDevolver, List<ItemVenda> itensNovos, string observacao = "")
        {
            DevolverVenda(vendaId, itensDevolver, observacao);
            var venda = ObterPorId(vendaId) ?? throw new KeyNotFoundException($"Venda com ID {vendaId} não encontrada.");
            ValidarEstoqueDisponivel(itensNovos);

            var valorDevolvido = itensDevolver.Sum(par =>
            {
                var item = venda.Itens.FirstOrDefault(i => i.Id == par.Key);
                return item == null ? 0 : _calculoFinanceiro.CalcularValorProporcionalDevolucao(item, par.Value);
            });
            var valorNovos = _calculoFinanceiro.CalcularSubtotalItens(itensNovos);
            var diferenca = valorNovos - valorDevolvido;

            BaixarEstoque(itensNovos);
            if (diferenca > 0)
            {
                if ((venda.FormaPagamento == Constants.PagamentoCredito || venda.FormaPagamento == "Credito") && venda.Cliente != null)
                {
                    venda.Cliente.SaldoDevedor += diferenca;
                    venda.Cliente.DataVencimentoCredito ??= DateTime.Now.Date.AddDays(30);
                    _clienteRepository.Salvar(venda.Cliente);
                }
                else
                {
                    _financeiroRepository.RegistrarReceita(new LancamentoFinanceiro
                    {
                        Categoria = "Troca",
                        Descricao = $"Troca - Venda {venda.NumeroVenda}",
                        Valor = diferenca,
                        DataLancamento = DateTime.Now,
                        DataPagamento = DateTime.Now,
                        Status = Constants.StatusPago,
                        FormaPagamento = venda.FormaPagamento,
                        Origem = "Troca de Venda",
                        OrigemId = venda.Id
                    });
                }
            }
        }

        public decimal ObterCustoTotalVendasPeriodo(DateTime inicio, DateTime fim)
        {
            return ObterTodas()
                .Where(v => v.Status == "Finalizada" && v.DataVenda >= inicio && v.DataVenda <= fim)
                .SelectMany(v => v.Itens)
                .Sum(i => (_produtoRepository.ObterPorId(i.ProdutoId)?.Custo ?? 0) * i.Quantidade);
        }

        private void SalvarVendaComItens(Venda venda)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO Vendas
                    (NumeroVenda, ClienteId, DataVenda, ValorSubtotal, ValorDesconto, ValorTotal,
                     FormaPagamento, Status, Vendedor, Observacoes)
                    VALUES
                    ($numeroVenda, $clienteId, $dataVenda, $valorSubtotal, $desconto, $valorTotal,
                     $formaPagamento, $status, $vendedor, $observacoes);
                    SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("$numeroVenda", venda.NumeroVenda);
                command.Parameters.AddWithValue("$clienteId", venda.ClienteId > 0 ? venda.ClienteId : (object)DBNull.Value);
                command.Parameters.AddWithValue("$dataVenda", venda.DataVenda.ToString("O"));
                command.Parameters.AddWithValue("$valorSubtotal", venda.ValorSubtotal);
                command.Parameters.AddWithValue("$desconto", venda.Desconto);
                command.Parameters.AddWithValue("$valorTotal", venda.ValorTotal);
                command.Parameters.AddWithValue("$formaPagamento", venda.FormaPagamento);
                command.Parameters.AddWithValue("$status", venda.Status);
                command.Parameters.AddWithValue("$vendedor", venda.Vendedor ?? string.Empty);
                command.Parameters.AddWithValue("$observacoes", venda.Observacoes ?? string.Empty);
                venda.Id = Convert.ToInt32(command.ExecuteScalar());
            }

            foreach (var item in venda.Itens)
            {
                using var itemCommand = connection.CreateCommand();
                itemCommand.Transaction = transaction;
                itemCommand.CommandText = @"
                    INSERT INTO ItensVenda
                    (VendaId, ProdutoId, Quantidade, PrecoUnitario, Desconto, ValorTotal)
                    VALUES ($vendaId, $produtoId, $quantidade, $precoUnitario, $desconto, $valorTotal);
                    SELECT last_insert_rowid();";
                itemCommand.Parameters.AddWithValue("$vendaId", venda.Id);
                itemCommand.Parameters.AddWithValue("$produtoId", item.ProdutoId);
                itemCommand.Parameters.AddWithValue("$quantidade", item.Quantidade);
                itemCommand.Parameters.AddWithValue("$precoUnitario", item.PrecoUnitario);
                itemCommand.Parameters.AddWithValue("$desconto", item.Desconto);
                itemCommand.Parameters.AddWithValue("$valorTotal", item.ValorTotal);
                item.Id = Convert.ToInt32(itemCommand.ExecuteScalar());
                item.VendaId = venda.Id;
            }

            transaction.Commit();
        }

        private void ValidarVenda(Venda venda)
        {
            if (venda == null)
                throw new ArgumentNullException(nameof(venda));
            if (venda.Itens == null || venda.Itens.Count == 0)
                throw new ArgumentException("Venda deve ter pelo menos um item.");
            if (venda.ValorTotal <= 0)
                throw new ArgumentException("Valor total da venda deve ser maior que zero.");
            if (venda.ValorSubtotal < venda.Desconto)
                throw new ArgumentException("Desconto não pode ser maior que o subtotal.");
        }

        private void ValidarEstoqueDisponivel(List<ItemVenda> itens)
        {
            foreach (var item in itens)
            {
                var produto = _produtoRepository.ObterPorId(item.ProdutoId)
                    ?? throw new ArgumentException($"Produto com ID {item.ProdutoId} não encontrado.");
                if (!produto.Ativo)
                    throw new ArgumentException($"Produto {produto.Nome} está inativo.");
                if (produto.Estoque < item.Quantidade)
                    throw new InvalidOperationException($"Estoque insuficiente para o produto {produto.Nome}.");
            }
        }

        private void BaixarEstoque(List<ItemVenda> itens)
        {
            foreach (var item in itens)
            {
                var produto = _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto == null) continue;
                produto.Estoque = Math.Max(0, produto.Estoque - item.Quantidade);
                _produtoRepository.Atualizar(produto);
            }
        }

        private Venda ReadVenda(SqliteDataReader reader)
        {
            var id = reader.GetInt32(reader.GetOrdinal("Id"));
            var clienteIdOrdinal = reader.GetOrdinal("ClienteId");
            var clienteId = reader.IsDBNull(clienteIdOrdinal) ? 0 : reader.GetInt32(clienteIdOrdinal);
            var venda = new Venda
            {
                Id = id,
                NumeroVenda = reader.GetString(reader.GetOrdinal("NumeroVenda")),
                ClienteId = clienteId,
                Cliente = clienteId > 0 ? _clienteRepository.ObterPorId(clienteId) : null,
                DataVenda = DateTime.Parse(reader.GetString(reader.GetOrdinal("DataVenda"))),
                ValorSubtotal = reader.GetDecimal(reader.GetOrdinal("ValorSubtotal")),
                Desconto = reader.GetDecimal(reader.GetOrdinal("ValorDesconto")),
                ValorTotal = reader.GetDecimal(reader.GetOrdinal("ValorTotal")),
                FormaPagamento = reader.GetString(reader.GetOrdinal("FormaPagamento")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Vendedor = reader.GetString(reader.GetOrdinal("Vendedor")),
                Observacoes = reader.GetString(reader.GetOrdinal("Observacoes")),
                Itens = ObterItensVenda(id)
            };
            return venda;
        }

        private List<ItemVenda> ObterItensVenda(int vendaId)
        {
            var itens = new List<ItemVenda>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ItensVenda WHERE VendaId = $vendaId";
            command.Parameters.AddWithValue("$vendaId", vendaId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var produtoId = reader.GetInt32(reader.GetOrdinal("ProdutoId"));
                var item = new ItemVenda
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    VendaId = vendaId,
                    ProdutoId = produtoId,
                    Produto = _produtoRepository.ObterPorId(produtoId),
                    Quantidade = reader.GetInt32(reader.GetOrdinal("Quantidade")),
                    PrecoUnitario = reader.GetDecimal(reader.GetOrdinal("PrecoUnitario")),
                    Desconto = reader.GetDecimal(reader.GetOrdinal("Desconto"))
                };
                itens.Add(item);
            }
            return itens;
        }

        private void AtualizarStatusVenda(int vendaId, string novoStatus)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Vendas SET Status = $status WHERE Id = $id";
            command.Parameters.AddWithValue("$status", novoStatus);
            command.Parameters.AddWithValue("$id", vendaId);
            command.ExecuteNonQuery();
        }

        private SqliteConnection OpenConnection()
        {
            var connection = _connectionFactory.CreateConnection();
            connection.Open();
            return connection;
        }

        private static string GerarNumeroVenda()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"VENDA-{timestamp}-{random}";
        }
    }
}
