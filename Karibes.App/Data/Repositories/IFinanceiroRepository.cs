using System;
using System.Collections.Generic;
using Karibes.App.Models;

namespace Karibes.App.Data.Repositories
{
    public interface IFinanceiroRepository
    {
        void RegistrarReceita(LancamentoFinanceiro lancamento);
        void RegistrarDespesa(LancamentoFinanceiro lancamento);
        void RegistrarPagamentoFiado(
            Cliente cliente,
            decimal valor,
            string observacao = "",
            DateTime? dataPagamento = null,
            string formaPagamento = "Dinheiro");
        void AtualizarStatus(int lancamentoId, string novoStatus);
        List<LancamentoFinanceiro> ObterLancamentos(DateTime inicio, DateTime fim);
        List<LancamentoFinanceiro> ObterLancamentosComPagamentoNoPeriodo(DateTime inicio, DateTime fim);
        decimal ObterSaldoCaixa(DateTime? ate = null);
    }
}
