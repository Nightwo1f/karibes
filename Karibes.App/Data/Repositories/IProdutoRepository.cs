using System.Collections.Generic;
using Karibes.App.Models;

namespace Karibes.App.Data.Repositories
{
    public interface IProdutoRepository
    {
        List<Produto> ObterTodos();
        Produto? ObterPorId(int id);
        Produto? ObterPorCodigo(string codigo);
        int Criar(Produto produto);
        void Atualizar(Produto produto);
        void Excluir(int id);
        void AtualizarEstoque(int produtoId, int quantidade);
        List<Produto> ObterProdutosEstoqueCritico();
    }
}
