using System;
using Karibes.App.Data.Repositories;
using Karibes.App.Models;
using Microsoft.Data.Sqlite;

namespace Karibes.App.Data.Sqlite
{
    public class ExcelToSqliteMigrationService
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public ExcelToSqliteMigrationService(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void MigrateIfEmpty()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            if (CountRows(connection, "Produtos") == 0)
                MigrateProdutos(connection);

            if (CountRows(connection, "Clientes") == 0)
                MigrateClientes(connection);

            if (CountRows(connection, "Vendas") == 0)
                MigrateVendas(connection);

            if (CountRows(connection, "LancamentosFinanceiros") == 0)
                MigrateLancamentosFinanceiros(connection);
        }

        private static int CountRows(SqliteConnection connection, string table)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {table}";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static void MigrateProdutos(SqliteConnection connection)
        {
            var produtos = new ExcelProdutoRepository().ObterTodos();
            foreach (var produto in produtos)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR IGNORE INTO Produtos
                    (Id, Codigo, Nome, Descricao, Categoria, PrecoVenda, PrecoCusto, EstoqueAtual, EstoqueMinimo,
                     Unidade, DataCadastro, DataUltimaAtualizacao, Ativo)
                    VALUES
                    ($id, $codigo, $nome, $descricao, $categoria, $precoVenda, $precoCusto, $estoqueAtual, $estoqueMinimo,
                     $unidade, $dataCadastro, $dataUltimaAtualizacao, $ativo)";
                command.Parameters.AddWithValue("$id", produto.Id);
                command.Parameters.AddWithValue("$codigo", produto.Codigo);
                command.Parameters.AddWithValue("$nome", produto.Nome);
                command.Parameters.AddWithValue("$descricao", produto.Descricao ?? string.Empty);
                command.Parameters.AddWithValue("$categoria", produto.Categoria ?? string.Empty);
                command.Parameters.AddWithValue("$precoVenda", produto.Preco);
                command.Parameters.AddWithValue("$precoCusto", produto.Custo);
                command.Parameters.AddWithValue("$estoqueAtual", produto.Estoque);
                command.Parameters.AddWithValue("$estoqueMinimo", produto.EstoqueMinimo);
                command.Parameters.AddWithValue("$unidade", produto.Unidade ?? "UN");
                command.Parameters.AddWithValue("$dataCadastro", produto.DataCadastro.ToString("O"));
                command.Parameters.AddWithValue("$dataUltimaAtualizacao", produto.DataUltimaAtualizacao.ToString("O"));
                command.Parameters.AddWithValue("$ativo", produto.Ativo ? 1 : 0);
                command.ExecuteNonQuery();
            }
        }

        private static void MigrateClientes(SqliteConnection connection)
        {
            var clientes = new ExcelClienteRepository().ObterTodos();
            foreach (var cliente in clientes)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR IGNORE INTO Clientes
                    (Id, Codigo, Nome, TipoDocumento, Documento, Email, Telefone, Celular,
                     CEP, Endereco, Numero, Complemento, Bairro, Cidade, Estado,
                     LimiteCredito, SaldoDevedor, TotalPago, DataVencimentoCredito, DataCadastro, DataUltimaAtualizacao, Ativo, Observacoes)
                    VALUES
                    ($id, $codigo, $nome, $tipoDocumento, $documento, $email, $telefone, $celular,
                     $cep, $endereco, $numero, $complemento, $bairro, $cidade, $estado,
                     $limiteCredito, $saldoDevedor, $totalPago, $dataVencimentoCredito, $dataCadastro, $dataUltimaAtualizacao, $ativo, $observacoes)";
                command.Parameters.AddWithValue("$id", cliente.Id);
                command.Parameters.AddWithValue("$codigo", cliente.Codigo);
                command.Parameters.AddWithValue("$nome", cliente.Nome);
                command.Parameters.AddWithValue("$tipoDocumento", cliente.TipoDocumento ?? string.Empty);
                command.Parameters.AddWithValue("$documento", cliente.Documento ?? string.Empty);
                command.Parameters.AddWithValue("$email", cliente.Email ?? string.Empty);
                command.Parameters.AddWithValue("$telefone", cliente.Telefone ?? string.Empty);
                command.Parameters.AddWithValue("$celular", cliente.Celular ?? string.Empty);
                command.Parameters.AddWithValue("$cep", cliente.CEP ?? string.Empty);
                command.Parameters.AddWithValue("$endereco", cliente.Endereco ?? string.Empty);
                command.Parameters.AddWithValue("$numero", cliente.Numero ?? string.Empty);
                command.Parameters.AddWithValue("$complemento", cliente.Complemento ?? string.Empty);
                command.Parameters.AddWithValue("$bairro", cliente.Bairro ?? string.Empty);
                command.Parameters.AddWithValue("$cidade", cliente.Cidade ?? string.Empty);
                command.Parameters.AddWithValue("$estado", cliente.Estado ?? string.Empty);
                command.Parameters.AddWithValue("$limiteCredito", cliente.LimiteCredito);
                command.Parameters.AddWithValue("$saldoDevedor", cliente.SaldoDevedor);
                command.Parameters.AddWithValue("$totalPago", cliente.TotalPago);
                command.Parameters.AddWithValue("$dataVencimentoCredito", cliente.DataVencimentoCredito?.ToString("O") ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$dataCadastro", cliente.DataCadastro.ToString("O"));
                command.Parameters.AddWithValue("$dataUltimaAtualizacao", cliente.DataUltimaAtualizacao.ToString("O"));
                command.Parameters.AddWithValue("$ativo", cliente.Ativo ? 1 : 0);
                command.Parameters.AddWithValue("$observacoes", cliente.Observacoes ?? string.Empty);
                command.ExecuteNonQuery();
            }
        }

        private static void MigrateVendas(SqliteConnection connection)
        {
            var vendas = new ExcelVendaRepository().ObterTodas();
            foreach (var venda in vendas)
            {
                using var transaction = connection.BeginTransaction();
                using (var vendaCommand = connection.CreateCommand())
                {
                    vendaCommand.Transaction = transaction;
                    vendaCommand.CommandText = @"
                        INSERT OR IGNORE INTO Vendas
                        (Id, NumeroVenda, ClienteId, DataVenda, ValorSubtotal, ValorDesconto, ValorTotal,
                         FormaPagamento, Status, Vendedor, Observacoes)
                        VALUES
                        ($id, $numeroVenda, $clienteId, $dataVenda, $valorSubtotal, $valorDesconto, $valorTotal,
                         $formaPagamento, $status, $vendedor, $observacoes)";
                    vendaCommand.Parameters.AddWithValue("$id", venda.Id);
                    vendaCommand.Parameters.AddWithValue("$numeroVenda", venda.NumeroVenda);
                    vendaCommand.Parameters.AddWithValue("$clienteId", venda.ClienteId > 0 ? venda.ClienteId : (object)DBNull.Value);
                    vendaCommand.Parameters.AddWithValue("$dataVenda", venda.DataVenda.ToString("O"));
                    vendaCommand.Parameters.AddWithValue("$valorSubtotal", venda.ValorSubtotal);
                    vendaCommand.Parameters.AddWithValue("$valorDesconto", venda.Desconto);
                    vendaCommand.Parameters.AddWithValue("$valorTotal", venda.ValorTotal);
                    vendaCommand.Parameters.AddWithValue("$formaPagamento", venda.FormaPagamento ?? string.Empty);
                    vendaCommand.Parameters.AddWithValue("$status", venda.Status ?? string.Empty);
                    vendaCommand.Parameters.AddWithValue("$vendedor", venda.Vendedor ?? string.Empty);
                    vendaCommand.Parameters.AddWithValue("$observacoes", venda.Observacoes ?? string.Empty);
                    vendaCommand.ExecuteNonQuery();
                }

                foreach (var item in venda.Itens)
                {
                    using var itemCommand = connection.CreateCommand();
                    itemCommand.Transaction = transaction;
                    itemCommand.CommandText = @"
                        INSERT OR IGNORE INTO ItensVenda
                        (Id, VendaId, ProdutoId, Quantidade, PrecoUnitario, Desconto, ValorTotal)
                        VALUES
                        ($id, $vendaId, $produtoId, $quantidade, $precoUnitario, $desconto, $valorTotal)";
                    itemCommand.Parameters.AddWithValue("$id", item.Id);
                    itemCommand.Parameters.AddWithValue("$vendaId", venda.Id);
                    itemCommand.Parameters.AddWithValue("$produtoId", item.ProdutoId);
                    itemCommand.Parameters.AddWithValue("$quantidade", item.Quantidade);
                    itemCommand.Parameters.AddWithValue("$precoUnitario", item.PrecoUnitario);
                    itemCommand.Parameters.AddWithValue("$desconto", item.Desconto);
                    itemCommand.Parameters.AddWithValue("$valorTotal", item.ValorTotal);
                    itemCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static void MigrateLancamentosFinanceiros(SqliteConnection connection)
        {
            var lancamentos = new ExcelFinanceiroRepository()
                .ObterLancamentos(DateTime.MinValue, DateTime.MaxValue);
            foreach (var lancamento in lancamentos)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR IGNORE INTO LancamentosFinanceiros
                    (Id, Tipo, Categoria, Descricao, Valor, DataLancamento, DataVencimento, DataPagamento,
                     Status, FormaPagamento, Origem, OrigemId, Observacoes)
                    VALUES
                    ($id, $tipo, $categoria, $descricao, $valor, $dataLancamento, $dataVencimento, $dataPagamento,
                     $status, $formaPagamento, $origem, $origemId, $observacoes)";
                command.Parameters.AddWithValue("$id", lancamento.Id);
                command.Parameters.AddWithValue("$tipo", lancamento.Tipo ?? string.Empty);
                command.Parameters.AddWithValue("$categoria", lancamento.Categoria ?? string.Empty);
                command.Parameters.AddWithValue("$descricao", lancamento.Descricao ?? string.Empty);
                command.Parameters.AddWithValue("$valor", lancamento.Valor);
                command.Parameters.AddWithValue("$dataLancamento", lancamento.DataLancamento.ToString("O"));
                command.Parameters.AddWithValue("$dataVencimento", lancamento.DataVencimento?.ToString("O") ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$dataPagamento", lancamento.DataPagamento?.ToString("O") ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$status", lancamento.Status ?? string.Empty);
                command.Parameters.AddWithValue("$formaPagamento", lancamento.FormaPagamento ?? string.Empty);
                command.Parameters.AddWithValue("$origem", lancamento.Origem ?? string.Empty);
                command.Parameters.AddWithValue("$origemId", lancamento.OrigemId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$observacoes", lancamento.Observacoes ?? string.Empty);
                command.ExecuteNonQuery();
            }
        }
    }
}
