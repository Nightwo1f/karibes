using System;
using System.Collections.Generic;
using Karibes.App.Models;
using Karibes.App.Services;

namespace Karibes.App.Data.Repositories
{
    public class ExcelVendaRepository : IVendaRepository
    {
        private readonly VendaService _vendaService;

        public ExcelVendaRepository()
            : this(new VendaService())
        {
        }

        public ExcelVendaRepository(VendaService vendaService)
        {
            _vendaService = vendaService;
        }

        public List<Venda> ObterTodas() => _vendaService.ObterTodas();
        public List<Venda> ObterVendasPorCliente(int clienteId) => _vendaService.ObterVendasPorCliente(clienteId);
        public Venda? ObterPorId(int id) => _vendaService.ObterPorId(id);
        public Venda? ObterPorNumeroVenda(string numeroVenda) => _vendaService.ObterPorNumeroVenda(numeroVenda);
        public void RegistrarVendaAVista(Venda venda) => _vendaService.RegistrarVendaAVista(venda);
        public void RegistrarVendaFiada(Venda venda) => _vendaService.RegistrarVendaFiada(venda);
        public void CancelarVenda(int vendaId, string motivo = "") => _vendaService.CancelarVenda(vendaId, motivo);
        public void DevolverVenda(int vendaId, Dictionary<int, int> itensDevolver, string observacao = "") => _vendaService.DevolverVenda(vendaId, itensDevolver, observacao);
        public void TrocarVenda(int vendaId, Dictionary<int, int> itensDevolver, List<ItemVenda> itensNovos, string observacao = "") => _vendaService.TrocarVenda(vendaId, itensDevolver, itensNovos, observacao);
        public decimal ObterCustoTotalVendasPeriodo(DateTime inicio, DateTime fim) => _vendaService.ObterCustoTotalVendasPeriodo(inicio, fim);
    }
}
