using System.Collections.Generic;
using Karibes.App.Models;

namespace Karibes.App.Data.Repositories
{
    public interface IClienteRepository
    {
        List<Cliente> ObterTodos();
        Cliente? ObterPorId(int id);
        List<Cliente> Buscar(string termo);
        void Salvar(Cliente cliente);
    }
}
