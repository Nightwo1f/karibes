using System.Collections.Generic;
using Karibes.App.Models;
using Karibes.App.Services;

namespace Karibes.App.Data.Repositories
{
    public class ExcelClienteRepository : IClienteRepository
    {
        private readonly ClienteService _clienteService;

        public ExcelClienteRepository()
            : this(new ClienteService())
        {
        }

        public ExcelClienteRepository(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public List<Cliente> ObterTodos() => _clienteService.ObterTodos();
        public Cliente? ObterPorId(int id) => _clienteService.ObterPorId(id);
        public List<Cliente> Buscar(string termo) => _clienteService.Buscar(termo);
        public void Salvar(Cliente cliente) => _clienteService.Salvar(cliente);
    }
}
