using System;
using System.Collections.Generic;
using Karibes.App.Models;
using Karibes.App.Services;

namespace Karibes.App.Data.Repositories
{
    public class ExcelFinanceiroRepository : IFinanceiroRepository
    {
        private readonly FinanceiroService _financeiroService;

        public ExcelFinanceiroRepository()
            : this(new FinanceiroService(new ExcelService()))
        {
        }

        public ExcelFinanceiroRepository(FinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        public void RegistrarReceita(LancamentoFinanceiro lancamento) => _financeiroService.RegistrarReceita(lancamento);
        public void RegistrarDespesa(LancamentoFinanceiro lancamento) => _financeiroService.RegistrarDespesa(lancamento);
        public void RegistrarPagamentoFiado(Cliente cliente, decimal valor, string observacao = "", DateTime? dataPagamento = null, string formaPagamento = "Dinheiro")
            => _financeiroService.RegistrarPagamentoFiado(cliente, valor, observacao, dataPagamento, formaPagamento);
        public void AtualizarStatus(int lancamentoId, string novoStatus) => _financeiroService.AtualizarStatus(lancamentoId, novoStatus);
        public List<LancamentoFinanceiro> ObterLancamentos(DateTime inicio, DateTime fim) => _financeiroService.ObterLancamentos(inicio, fim);
        public List<LancamentoFinanceiro> ObterLancamentosComPagamentoNoPeriodo(DateTime inicio, DateTime fim) => _financeiroService.ObterLancamentosComPagamentoNoPeriodo(inicio, fim);
        public decimal ObterSaldoCaixa(DateTime? ate = null) => _financeiroService.ObterSaldoCaixa(ate);
    }
}
