using OfficeOpenXml;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço para gerenciamento de produtos usando Excel como banco de dados
    /// </summary>
    public class ProdutoService
    {
        private readonly ExcelService _excelService;
        private const string SheetName = "Produtos";

        public ProdutoService()
        {
            _excelService = new ExcelService();
            InicializarArquivo();
        }

        /// <summary>
        /// Inicializa o arquivo Excel com cabeçalhos se não existir
        /// </summary>
        private void InicializarArquivo()
        {
            if (!_excelService.FileExists(Constants.ProdutosFile))
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(SheetName);
                
                // Cabeçalhos
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Codigo";
                worksheet.Cells[1, 3].Value = "Nome";
                worksheet.Cells[1, 4].Value = "Descricao";
                worksheet.Cells[1, 5].Value = "Categoria";
                worksheet.Cells[1, 6].Value = "PrecoVenda";
                worksheet.Cells[1, 7].Value = "PrecoCusto";
                worksheet.Cells[1, 8].Value = "EstoqueAtual";
                worksheet.Cells[1, 9].Value = "EstoqueMinimo";
                worksheet.Cells[1, 10].Value = "Unidade";
                worksheet.Cells[1, 11].Value = "DataCadastro";
                worksheet.Cells[1, 12].Value = "DataUltimaAtualizacao";
                worksheet.Cells[1, 13].Value = "Ativo";
                worksheet.Cells[1, 14].Value = "Observacoes";

                // Formatação do cabeçalho
                using (var range = worksheet.Cells[1, 1, 1, 14])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                _excelService.SavePackage(package, Constants.ProdutosFile);
            }
        }

        /// <summary>
        /// Obtém todos os produtos
        /// </summary>
        public List<Produto> ObterTodos()
        {
            var produtos = new List<Produto>();

            if (!_excelService.FileExists(Constants.ProdutosFile))
                return produtos;

            using var package = _excelService.GetPackage(Constants.ProdutosFile);
            if (package == null) return produtos;

            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet.Dimension == null) return produtos;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var produto = LerProdutoDaLinha(worksheet, row);
                if (produto != null)
                    produtos.Add(produto);
            }

            return produtos;
        }

        /// <summary>
        /// Obtém um produto por ID
        /// </summary>
        public Produto? ObterPorId(int id)
        {
            if (!_excelService.FileExists(Constants.ProdutosFile))
                return null;

            using var package = _excelService.GetPackage(Constants.ProdutosFile);
            if (package == null) return null;

            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet.Dimension == null) return null;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var produtoId = worksheet.Cells[row, 1].GetValue<int?>();
                if (produtoId == id)
                {
                    return LerProdutoDaLinha(worksheet, row);
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém um produto por código
        /// </summary>
        public Produto? ObterPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            if (!_excelService.FileExists(Constants.ProdutosFile))
                return null;

            using var package = _excelService.GetPackage(Constants.ProdutosFile);
            if (package == null) return null;

            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet.Dimension == null) return null;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var produtoCodigo = worksheet.Cells[row, 2].GetValue<string>();
                if (produtoCodigo?.Trim().Equals(codigo.Trim(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    return LerProdutoDaLinha(worksheet, row);
                }
            }

            return null;
        }

        /// <summary>
        /// Cria um novo produto
        /// </summary>
        public int Criar(Produto produto)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(produto.Nome))
                throw new ArgumentException("Nome do produto é obrigatório.");

            if (string.IsNullOrWhiteSpace(produto.Codigo))
                throw new ArgumentException("Código do produto é obrigatório.");

            // Verifica se código já existe
            if (ObterPorCodigo(produto.Codigo) != null)
                throw new ArgumentException("Código do produto já existe.");

            using var package = _excelService.GetPackage(Constants.ProdutosFile) ?? new ExcelPackage();
            var worksheet = package.Workbook.Worksheets[SheetName] ?? package.Workbook.Worksheets.Add(SheetName);

            // Obtém próximo ID
            int novoId = ObterProximoId(worksheet);
            produto.Id = novoId;
            produto.DataCadastro = DateTime.Now;
            produto.DataUltimaAtualizacao = DateTime.Now;

            // Encontra próxima linha vazia
            int novaLinha = _excelService.GetNextRow(worksheet);

            // Escreve dados
            EscreverProdutoNaLinha(worksheet, novaLinha, produto);

            _excelService.SavePackage(package, Constants.ProdutosFile);

            return novoId;
        }

        /// <summary>
        /// Atualiza um produto existente
        /// </summary>
        public void Atualizar(Produto produto)
        {
            if (produto.Id <= 0)
                throw new ArgumentException("ID do produto inválido.");

            if (string.IsNullOrWhiteSpace(produto.Nome))
                throw new ArgumentException("Nome do produto é obrigatório.");

            if (string.IsNullOrWhiteSpace(produto.Codigo))
                throw new ArgumentException("Código do produto é obrigatório.");

            using var package = _excelService.GetPackage(Constants.ProdutosFile);
            if (package == null)
                throw new FileNotFoundException("Arquivo de produtos não encontrado.");

            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet.Dimension == null)
                throw new InvalidOperationException("Arquivo de produtos está vazio.");

            int rowCount = worksheet.Dimension.End.Row;

            // Encontra a linha do produto
            for (int row = 2; row <= rowCount; row++)
            {
                var produtoId = worksheet.Cells[row, 1].GetValue<int?>();
                if (produtoId == produto.Id)
                {
                    // Verifica se código não está sendo usado por outro produto
                    var codigoAtual = worksheet.Cells[row, 2].GetValue<string>();
                    if (codigoAtual != produto.Codigo)
                    {
                        if (ObterPorCodigo(produto.Codigo) != null)
                            throw new ArgumentException("Código do produto já está em uso por outro produto.");
                    }

                    produto.DataUltimaAtualizacao = DateTime.Now;
                    EscreverProdutoNaLinha(worksheet, row, produto);
                    _excelService.SavePackage(package, Constants.ProdutosFile);
                    return;
                }
            }

            throw new KeyNotFoundException($"Produto com ID {produto.Id} não encontrado.");
        }

        /// <summary>
        /// Exclui um produto (soft delete - marca como inativo)
        /// </summary>
        public void Excluir(int id)
        {
            var produto = ObterPorId(id);
            if (produto == null)
                throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

            produto.Ativo = false;
            produto.DataUltimaAtualizacao = DateTime.Now;
            Atualizar(produto);
        }

        /// <summary>
        /// Atualiza o estoque de um produto
        /// </summary>
        public void AtualizarEstoque(int produtoId, int quantidade)
        {
            var produto = ObterPorId(produtoId);
            if (produto == null)
                throw new KeyNotFoundException($"Produto com ID {produtoId} não encontrado.");

            produto.Estoque = quantidade;
            produto.DataUltimaAtualizacao = DateTime.Now;
            Atualizar(produto);
        }

        /// <summary>
        /// Obtém produtos com estoque abaixo do mínimo
        /// </summary>
        public List<Produto> ObterProdutosEstoqueCritico()
        {
            return ObterTodos()
                .Where(p => p.Ativo && p.Estoque <= p.EstoqueMinimo)
                .ToList();
        }

        /// <summary>
        /// Lê um produto de uma linha do Excel
        /// </summary>
        private Produto? LerProdutoDaLinha(ExcelWorksheet worksheet, int row)
        {
            try
            {
                return new Produto
                {
                    Id = worksheet.Cells[row, 1].GetValue<int>(),
                    Codigo = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty,
                    Nome = worksheet.Cells[row, 3].GetValue<string>() ?? string.Empty,
                    Descricao = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
                    Preco = worksheet.Cells[row, 6].GetValue<decimal>(),
                    Custo = worksheet.Cells[row, 7].GetValue<decimal>(),
                    Estoque = worksheet.Cells[row, 8].GetValue<int>(),
                    EstoqueMinimo = worksheet.Cells[row, 9].GetValue<int>(),
                    Unidade = worksheet.Cells[row, 10].GetValue<string>() ?? "UN",
                    DataCadastro = worksheet.Cells[row, 11].GetValue<DateTime>(),
                    DataUltimaAtualizacao = worksheet.Cells[row, 12].GetValue<DateTime>(),
                    Ativo = worksheet.Cells[row, 13].GetValue<bool>()
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Escreve um produto em uma linha do Excel
        /// </summary>
        private void EscreverProdutoNaLinha(ExcelWorksheet worksheet, int row, Produto produto)
        {
            worksheet.Cells[row, 1].Value = produto.Id;
            worksheet.Cells[row, 2].Value = produto.Codigo;
            worksheet.Cells[row, 3].Value = produto.Nome;
            worksheet.Cells[row, 4].Value = produto.Descricao;
            worksheet.Cells[row, 5].Value = ""; // Categoria (futuro)
            worksheet.Cells[row, 6].Value = produto.Preco;
            worksheet.Cells[row, 7].Value = produto.Custo;
            worksheet.Cells[row, 8].Value = produto.Estoque;
            worksheet.Cells[row, 9].Value = produto.EstoqueMinimo;
            worksheet.Cells[row, 10].Value = produto.Unidade;
            worksheet.Cells[row, 11].Value = produto.DataCadastro;
            worksheet.Cells[row, 12].Value = produto.DataUltimaAtualizacao;
            worksheet.Cells[row, 13].Value = produto.Ativo;
            worksheet.Cells[row, 14].Value = ""; // Observações (futuro)
        }

        /// <summary>
        /// Obtém o próximo ID disponível
        /// </summary>
        private int ObterProximoId(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension == null)
                return 1;

            int maxId = 0;
            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id.HasValue && id.Value > maxId)
                    maxId = id.Value;
            }

            return maxId + 1;
        }
    }
}

