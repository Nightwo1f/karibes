using System;
using System.Collections.Generic;
using Karibes.App.Data.Sqlite;
using Karibes.App.Models;
using Karibes.App.Utils;
using Microsoft.Data.Sqlite;

namespace Karibes.App.Data.Repositories
{
    public class SqliteFinanceiroRepository : IFinanceiroRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteFinanceiroRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void RegistrarReceita(LancamentoFinanceiro lancamento)
        {
            if (lancamento == null)
                throw new ArgumentNullException(nameof(lancamento));
            if (lancamento.Valor <= 0)
                throw new ArgumentException("Valor da receita deve ser maior que zero.");

            lancamento.Tipo = Constants.TipoReceita;
            lancamento.Status = Constants.StatusPago;
            lancamento.DataLancamento = lancamento.DataLancamento == default ? DateTime.Now : lancamento.DataLancamento;
            lancamento.DataPagamento = lancamento.DataPagamento ?? lancamento.DataLancamento;
            SalvarLancamento(lancamento);
        }

        public void RegistrarDespesa(LancamentoFinanceiro lancamento)
        {
            if (lancamento == null)
                throw new ArgumentNullException(nameof(lancamento));
            if (lancamento.Valor <= 0)
                throw new ArgumentException("Valor da despesa deve ser maior que zero.");

            lancamento.Tipo = Constants.TipoDespesa;
            lancamento.DataLancamento = lancamento.DataLancamento == default ? DateTime.Now : lancamento.DataLancamento;
            if (string.IsNullOrWhiteSpace(lancamento.Status))
                lancamento.Status = Constants.StatusPendente;
            SalvarLancamento(lancamento);
        }

        public void RegistrarPagamentoFiado(Cliente cliente, decimal valor, string observacao = "", DateTime? dataPagamento = null, string formaPagamento = Constants.PagamentoDinheiro)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            if (valor <= 0)
                throw new ArgumentException("Valor do pagamento deve ser maior que zero.", nameof(valor));

            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            var saldoAnterior = cliente.SaldoDevedor;
            cliente.SaldoDevedor = Math.Max(0, cliente.SaldoDevedor - valor);
            cliente.TotalPago += valor;
            if (cliente.SaldoDevedor == 0)
                cliente.DataVencimentoCredito = null;
            cliente.DataUltimaAtualizacao = DateTime.Now;

            using (var updateCliente = connection.CreateCommand())
            {
                updateCliente.Transaction = transaction;
                updateCliente.CommandText = @"
                    UPDATE Clientes
                    SET SaldoDevedor = $saldoDevedor,
                        TotalPago = $totalPago,
                        DataVencimentoCredito = $dataVencimentoCredito,
                        DataUltimaAtualizacao = $dataUltimaAtualizacao
                    WHERE Id = $id";
                updateCliente.Parameters.AddWithValue("$saldoDevedor", cliente.SaldoDevedor);
                updateCliente.Parameters.AddWithValue("$totalPago", cliente.TotalPago);
                updateCliente.Parameters.AddWithValue("$dataVencimentoCredito", cliente.DataVencimentoCredito?.ToString("O") ?? (object)DBNull.Value);
                updateCliente.Parameters.AddWithValue("$dataUltimaAtualizacao", cliente.DataUltimaAtualizacao.ToString("O"));
                updateCliente.Parameters.AddWithValue("$id", cliente.Id);
                updateCliente.ExecuteNonQuery();
            }

            var dataEfetiva = dataPagamento ?? DateTime.Now;
            var lancamento = new LancamentoFinanceiro
            {
                Tipo = Constants.TipoReceita,
                Categoria = "Recebimento de Crédito",
                Descricao = $"Pagamento de fiado - Cliente: {cliente.Nome}",
                Valor = valor,
                DataLancamento = dataEfetiva,
                DataPagamento = dataEfetiva,
                Status = Constants.StatusPago,
                FormaPagamento = formaPagamento,
                Origem = "Pagamento de fiado",
                OrigemId = cliente.Id,
                Observacoes = string.IsNullOrWhiteSpace(observacao)
                    ? $"Pagamento de crédito do cliente {cliente.Nome}. Saldo anterior: {saldoAnterior:N2}"
                    : observacao
            };
            InsertLancamento(connection, transaction, lancamento);

            transaction.Commit();
        }

        public void AtualizarStatus(int lancamentoId, string novoStatus)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE LancamentosFinanceiros
                SET Status = $status,
                    DataPagamento = COALESCE(DataPagamento, $dataPagamento)
                WHERE Id = $id";
            command.Parameters.AddWithValue("$status", novoStatus);
            command.Parameters.AddWithValue("$dataPagamento", novoStatus == Constants.StatusPago ? DateTime.Now.ToString("O") : DBNull.Value);
            command.Parameters.AddWithValue("$id", lancamentoId);
            if (command.ExecuteNonQuery() == 0)
                throw new KeyNotFoundException($"Lançamento com ID {lancamentoId} não encontrado.");
        }

        public List<LancamentoFinanceiro> ObterLancamentos(DateTime inicio, DateTime fim)
        {
            return ObterLancamentosPorData("DataLancamento", inicio, fim, apenasPagos: false);
        }

        public List<LancamentoFinanceiro> ObterLancamentosComPagamentoNoPeriodo(DateTime inicio, DateTime fim)
        {
            return ObterLancamentosPorData("COALESCE(DataPagamento, DataLancamento)", inicio, fim, apenasPagos: true);
        }

        public decimal ObterSaldoCaixa(DateTime? ate = null)
        {
            var lancamentos = ObterLancamentos(DateTime.MinValue, ate ?? DateTime.MaxValue);
            decimal receitas = 0;
            decimal despesas = 0;
            foreach (var lancamento in lancamentos)
            {
                if (lancamento.Status != Constants.StatusPago)
                    continue;
                if (lancamento.Tipo == Constants.TipoReceita)
                    receitas += lancamento.Valor;
                else if (lancamento.Tipo == Constants.TipoDespesa)
                    despesas += lancamento.Valor;
            }
            return receitas - despesas;
        }

        private void SalvarLancamento(LancamentoFinanceiro lancamento)
        {
            using var connection = OpenConnection();
            InsertLancamento(connection, null, lancamento);
        }

        private static void InsertLancamento(SqliteConnection connection, SqliteTransaction? transaction, LancamentoFinanceiro lancamento)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT INTO LancamentosFinanceiros
                (Tipo, Categoria, Descricao, Valor, DataLancamento, DataVencimento, DataPagamento,
                 Status, FormaPagamento, Origem, OrigemId, Observacoes)
                VALUES
                ($tipo, $categoria, $descricao, $valor, $dataLancamento, $dataVencimento, $dataPagamento,
                 $status, $formaPagamento, $origem, $origemId, $observacoes);
                SELECT last_insert_rowid();";
            AddParameters(command, lancamento);
            lancamento.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        private List<LancamentoFinanceiro> ObterLancamentosPorData(string colunaData, DateTime inicio, DateTime fim, bool apenasPagos)
        {
            var lancamentos = new List<LancamentoFinanceiro>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT * FROM LancamentosFinanceiros
                WHERE {colunaData} >= $inicio AND {colunaData} <= $fim
                {(apenasPagos ? "AND Status = $statusPago" : string.Empty)}
                ORDER BY DataLancamento DESC";
            command.Parameters.AddWithValue("$inicio", inicio.ToString("O"));
            command.Parameters.AddWithValue("$fim", fim.ToString("O"));
            if (apenasPagos)
                command.Parameters.AddWithValue("$statusPago", Constants.StatusPago);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                lancamentos.Add(ReadLancamento(reader));
            return lancamentos;
        }

        private SqliteConnection OpenConnection()
        {
            var connection = _connectionFactory.CreateConnection();
            connection.Open();
            return connection;
        }

        private static void AddParameters(SqliteCommand command, LancamentoFinanceiro lancamento)
        {
            command.Parameters.AddWithValue("$tipo", lancamento.Tipo ?? string.Empty);
            command.Parameters.AddWithValue("$categoria", lancamento.Categoria ?? string.Empty);
            command.Parameters.AddWithValue("$descricao", lancamento.Descricao ?? string.Empty);
            command.Parameters.AddWithValue("$valor", lancamento.Valor);
            command.Parameters.AddWithValue("$dataLancamento", lancamento.DataLancamento.ToString("O"));
            command.Parameters.AddWithValue("$dataVencimento", lancamento.DataVencimento?.ToString("O") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$dataPagamento", lancamento.DataPagamento?.ToString("O") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$status", lancamento.Status ?? Constants.StatusPendente);
            command.Parameters.AddWithValue("$formaPagamento", lancamento.FormaPagamento ?? string.Empty);
            command.Parameters.AddWithValue("$origem", lancamento.Origem ?? string.Empty);
            command.Parameters.AddWithValue("$origemId", lancamento.OrigemId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$observacoes", lancamento.Observacoes ?? string.Empty);
        }

        private static LancamentoFinanceiro ReadLancamento(SqliteDataReader reader)
        {
            return new LancamentoFinanceiro
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Tipo = reader.GetString(reader.GetOrdinal("Tipo")),
                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                Descricao = reader.GetString(reader.GetOrdinal("Descricao")),
                Valor = reader.GetDecimal(reader.GetOrdinal("Valor")),
                DataLancamento = DateTime.Parse(reader.GetString(reader.GetOrdinal("DataLancamento"))),
                DataVencimento = ReadNullableDate(reader, "DataVencimento"),
                DataPagamento = ReadNullableDate(reader, "DataPagamento"),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                FormaPagamento = reader.GetString(reader.GetOrdinal("FormaPagamento")),
                Origem = reader.GetString(reader.GetOrdinal("Origem")),
                OrigemId = reader.IsDBNull(reader.GetOrdinal("OrigemId")) ? null : reader.GetInt32(reader.GetOrdinal("OrigemId")),
                Observacoes = reader.GetString(reader.GetOrdinal("Observacoes"))
            };
        }

        private static DateTime? ReadNullableDate(SqliteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : DateTime.Parse(reader.GetString(ordinal));
        }
    }
}
