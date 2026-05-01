using System;
using System.Collections.Generic;
using Karibes.App.Models;

namespace Karibes.App.Data.Repositories
{
    public interface IVendaRepository
    {
        List<Venda> ObterTodas();
        List<Venda> ObterVendasPorCliente(int clienteId);
        Venda? ObterPorId(int id);
        Venda? ObterPorNumeroVenda(string numeroVenda);
        void RegistrarVendaAVista(Venda venda);
        void RegistrarVendaFiada(Venda venda);
        void CancelarVenda(int vendaId, string motivo = "");
        void DevolverVenda(int vendaId, Dictionary<int, int> itensDevolver, string observacao = "");
        void TrocarVenda(int vendaId, Dictionary<int, int> itensDevolver, List<ItemVenda> itensNovos, string observacao = "");
        decimal ObterCustoTotalVendasPeriodo(DateTime inicio, DateTime fim);
    }
}
