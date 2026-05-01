using System.Collections.Generic;
using Karibes.App.Models;
using Karibes.App.Services;

namespace Karibes.App.Data.Repositories
{
    public class ExcelProdutoRepository : IProdutoRepository
    {
        private readonly ProdutoService _produtoService;

        public ExcelProdutoRepository()
            : this(new ProdutoService())
        {
        }

        public ExcelProdutoRepository(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        public List<Produto> ObterTodos() => _produtoService.ObterTodos();
        public Produto? ObterPorId(int id) => _produtoService.ObterPorId(id);
        public Produto? ObterPorCodigo(string codigo) => _produtoService.ObterPorCodigo(codigo);
        public int Criar(Produto produto) => _produtoService.Criar(produto);
        public void Atualizar(Produto produto) => _produtoService.Atualizar(produto);
        public void Excluir(int id) => _produtoService.Excluir(id);
        public void AtualizarEstoque(int produtoId, int quantidade) => _produtoService.AtualizarEstoque(produtoId, quantidade);
        public List<Produto> ObterProdutosEstoqueCritico() => _produtoService.ObterProdutosEstoqueCritico();
    }
}
