using System;
using System.Collections.Generic;
using Karibes.App.Data.Sqlite;
using Karibes.App.Models;
using Microsoft.Data.Sqlite;

namespace Karibes.App.Data.Repositories
{
    public class SqliteProdutoRepository : IProdutoRepository
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteProdutoRepository(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Produto> ObterTodos()
        {
            var produtos = new List<Produto>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Produtos ORDER BY Nome";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                produtos.Add(ReadProduto(reader));
            return produtos;
        }

        public Produto? ObterPorId(int id)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Produtos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadProduto(reader) : null;
        }

        public Produto? ObterPorCodigo(string codigo)
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Produtos WHERE LOWER(Codigo) = LOWER($codigo)";
            command.Parameters.AddWithValue("$codigo", codigo ?? string.Empty);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadProduto(reader) : null;
        }

        public int Criar(Produto produto)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));
            if (string.IsNullOrWhiteSpace(produto.Nome))
                throw new ArgumentException("Nome do produto é obrigatório.");
            if (string.IsNullOrWhiteSpace(produto.Codigo))
                throw new ArgumentException("Código do produto é obrigatório.");
            if (ObterPorCodigo(produto.Codigo) != null)
                throw new ArgumentException("Código do produto já existe.");

            produto.DataCadastro = DateTime.Now;
            produto.DataUltimaAtualizacao = DateTime.Now;
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Produtos
                (Codigo, Nome, Descricao, Categoria, PrecoVenda, PrecoCusto, EstoqueAtual, EstoqueMinimo,
                 Unidade, DataCadastro, DataUltimaAtualizacao, Ativo)
                VALUES
                ($codigo, $nome, $descricao, $categoria, $preco, $custo, $estoque, $estoqueMinimo,
                 $unidade, $dataCadastro, $dataUltimaAtualizacao, $ativo);
                SELECT last_insert_rowid();";
            AddParameters(command, produto);
            produto.Id = Convert.ToInt32(command.ExecuteScalar());
            return produto.Id;
        }

        public void Atualizar(Produto produto)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto));
            if (produto.Id <= 0)
                throw new ArgumentException("ID do produto inválido.");

            produto.DataUltimaAtualizacao = DateTime.Now;
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Produtos SET
                    Codigo = $codigo,
                    Nome = $nome,
                    Descricao = $descricao,
                    Categoria = $categoria,
                    PrecoVenda = $preco,
                    PrecoCusto = $custo,
                    EstoqueAtual = $estoque,
                    EstoqueMinimo = $estoqueMinimo,
                    Unidade = $unidade,
                    DataCadastro = $dataCadastro,
                    DataUltimaAtualizacao = $dataUltimaAtualizacao,
                    Ativo = $ativo
                WHERE Id = $id";
            command.Parameters.AddWithValue("$id", produto.Id);
            AddParameters(command, produto);
            command.ExecuteNonQuery();
        }

        public void Excluir(int id)
        {
            var produto = ObterPorId(id) ?? throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");
            produto.Ativo = false;
            Atualizar(produto);
        }

        public void AtualizarEstoque(int produtoId, int quantidade)
        {
            var produto = ObterPorId(produtoId) ?? throw new KeyNotFoundException($"Produto com ID {produtoId} não encontrado.");
            produto.Estoque = quantidade;
            Atualizar(produto);
        }

        public List<Produto> ObterProdutosEstoqueCritico()
        {
            var produtos = new List<Produto>();
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Produtos WHERE Ativo = 1 AND EstoqueAtual <= EstoqueMinimo ORDER BY Nome";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                produtos.Add(ReadProduto(reader));
            return produtos;
        }

        private SqliteConnection OpenConnection()
        {
            var connection = _connectionFactory.CreateConnection();
            connection.Open();
            return connection;
        }

        private static void AddParameters(SqliteCommand command, Produto produto)
        {
            command.Parameters.AddWithValue("$codigo", produto.Codigo ?? string.Empty);
            command.Parameters.AddWithValue("$nome", produto.Nome ?? string.Empty);
            command.Parameters.AddWithValue("$descricao", produto.Descricao ?? string.Empty);
            command.Parameters.AddWithValue("$categoria", produto.Categoria ?? string.Empty);
            command.Parameters.AddWithValue("$preco", produto.Preco);
            command.Parameters.AddWithValue("$custo", produto.Custo);
            command.Parameters.AddWithValue("$estoque", produto.Estoque);
            command.Parameters.AddWithValue("$estoqueMinimo", produto.EstoqueMinimo);
            command.Parameters.AddWithValue("$unidade", produto.Unidade ?? "UN");
            command.Parameters.AddWithValue("$dataCadastro", produto.DataCadastro.ToString("O"));
            command.Parameters.AddWithValue("$dataUltimaAtualizacao", produto.DataUltimaAtualizacao.ToString("O"));
            command.Parameters.AddWithValue("$ativo", produto.Ativo ? 1 : 0);
        }

        private static Produto ReadProduto(SqliteDataReader reader)
        {
            return new Produto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Codigo = reader.GetString(reader.GetOrdinal("Codigo")),
                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                Descricao = reader.GetString(reader.GetOrdinal("Descricao")),
                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                Preco = reader.GetDecimal(reader.GetOrdinal("PrecoVenda")),
                Custo = reader.GetDecimal(reader.GetOrdinal("PrecoCusto")),
                Estoque = reader.GetInt32(reader.GetOrdinal("EstoqueAtual")),
                EstoqueMinimo = reader.GetInt32(reader.GetOrdinal("EstoqueMinimo")),
                Unidade = reader.GetString(reader.GetOrdinal("Unidade")),
                DataCadastro = DateTime.Parse(reader.GetString(reader.GetOrdinal("DataCadastro"))),
                DataUltimaAtualizacao = DateTime.Parse(reader.GetString(reader.GetOrdinal("DataUltimaAtualizacao"))),
                Ativo = reader.GetInt32(reader.GetOrdinal("Ativo")) == 1
            };
        }
    }
}
