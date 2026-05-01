using System;
using System.Collections.Generic;
using Karibes.App.Data.Sqlite;
using Karibes.App.Models;
using Microsoft.Data.Sqlite;

namespace Karibes.App.Data.Repositories
{
    public class SqliteClienteRepository : IClienteRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteClienteRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Cliente> ObterTodos()
        {
            var clientes = new List<Cliente>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Clientes ORDER BY Nome";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                clientes.Add(ReadCliente(reader));
            return clientes;
        }

        public Cliente? ObterPorId(int id)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Clientes WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadCliente(reader) : null;
        }

        public List<Cliente> Buscar(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return ObterTodos();

            var clientes = new List<Cliente>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Clientes
                WHERE Nome LIKE $termo OR Codigo LIKE $termo OR Documento LIKE $termo
                ORDER BY Nome";
            command.Parameters.AddWithValue("$termo", $"%{termo}%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
                clientes.Add(ReadCliente(reader));
            return clientes;
        }

        public void Salvar(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));
            if (string.IsNullOrWhiteSpace(cliente.Nome))
                throw new ArgumentException("O nome é obrigatório.", nameof(cliente));

            using var connection = OpenConnection();
            if (cliente.Id <= 0)
            {
                cliente.DataCadastro = DateTime.Now;
                cliente.DataUltimaAtualizacao = DateTime.Now;
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Clientes
                    (Codigo, Nome, TipoDocumento, Documento, Email, Telefone, Celular,
                     CEP, Endereco, Numero, Complemento, Bairro, Cidade, Estado, LimiteCredito,
                     SaldoDevedor, TotalPago, DataVencimentoCredito, DataCadastro, DataUltimaAtualizacao, Ativo, Observacoes)
                    VALUES
                    ($codigo, $nome, $tipoDocumento, $documento, $email, $telefone, $celular,
                     $cep, $endereco, $numero, $complemento, $bairro, $cidade, $estado, $limiteCredito,
                     $saldoDevedor, $totalPago, $dataVencimentoCredito, $dataCadastro, $dataUltimaAtualizacao, $ativo, $observacoes);
                    SELECT last_insert_rowid();";
                AddParameters(command, cliente);
                cliente.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            else
            {
                cliente.DataUltimaAtualizacao = DateTime.Now;
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Clientes SET
                        Codigo = $codigo,
                        Nome = $nome,
                        TipoDocumento = $tipoDocumento,
                        Documento = $documento,
                        Email = $email,
                        Telefone = $telefone,
                        Celular = $celular,
                        CEP = $cep,
                        Endereco = $endereco,
                        Numero = $numero,
                        Complemento = $complemento,
                        Bairro = $bairro,
                        Cidade = $cidade,
                        Estado = $estado,
                        LimiteCredito = $limiteCredito,
                        SaldoDevedor = $saldoDevedor,
                        TotalPago = $totalPago,
                        DataVencimentoCredito = $dataVencimentoCredito,
                        DataCadastro = $dataCadastro,
                        DataUltimaAtualizacao = $dataUltimaAtualizacao,
                        Ativo = $ativo,
                        Observacoes = $observacoes
                    WHERE Id = $id";
                command.Parameters.AddWithValue("$id", cliente.Id);
                AddParameters(command, cliente);
                command.ExecuteNonQuery();
            }
        }

        private SqliteConnection OpenConnection()
        {
            var connection = _connectionFactory.CreateConnection();
            connection.Open();
            return connection;
        }

        private static void AddParameters(SqliteCommand command, Cliente cliente)
        {
            command.Parameters.AddWithValue("$codigo", cliente.Codigo ?? string.Empty);
            command.Parameters.AddWithValue("$nome", cliente.Nome ?? string.Empty);
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
        }

        private static Cliente ReadCliente(SqliteDataReader reader)
        {
            return new Cliente
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Codigo = reader.GetString(reader.GetOrdinal("Codigo")),
                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                TipoDocumento = reader.GetString(reader.GetOrdinal("TipoDocumento")),
                Documento = reader.GetString(reader.GetOrdinal("Documento")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Telefone = reader.GetString(reader.GetOrdinal("Telefone")),
                Celular = reader.GetString(reader.GetOrdinal("Celular")),
                CEP = reader.GetString(reader.GetOrdinal("CEP")),
                Endereco = reader.GetString(reader.GetOrdinal("Endereco")),
                Numero = reader.GetString(reader.GetOrdinal("Numero")),
                Complemento = reader.GetString(reader.GetOrdinal("Complemento")),
                Bairro = reader.GetString(reader.GetOrdinal("Bairro")),
                Cidade = reader.GetString(reader.GetOrdinal("Cidade")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                LimiteCredito = reader.GetDecimal(reader.GetOrdinal("LimiteCredito")),
                SaldoDevedor = reader.GetDecimal(reader.GetOrdinal("SaldoDevedor")),
                TotalPago = reader.GetDecimal(reader.GetOrdinal("TotalPago")),
                DataVencimentoCredito = ReadNullableDate(reader, "DataVencimentoCredito"),
                DataCadastro = DateTime.Parse(reader.GetString(reader.GetOrdinal("DataCadastro"))),
                DataUltimaAtualizacao = DateTime.Parse(reader.GetString(reader.GetOrdinal("DataUltimaAtualizacao"))),
                Ativo = reader.GetInt32(reader.GetOrdinal("Ativo")) == 1,
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
