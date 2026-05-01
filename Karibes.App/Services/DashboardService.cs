using Karibes.App.Data.Repositories;
using Karibes.App.Models;
using System;
using System.Collections.Generic;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço do Dashboard: consome dados consolidados de RelatorioFinanceiroService e FluxoCaixaService.
    /// Não calcula valores; apenas delega para os serviços corretos.
    /// </summary>
    public class DashboardService
    {
        private readonly RelatorioFinanceiroService _relatorioFinanceiroService;
        private readonly FluxoCaixaService _fluxoCaixaService;
        private readonly IProdutoRepository _produtoRepository;

        public DashboardService()
        {
            _relatorioFinanceiroService = new RelatorioFinanceiroService();
            _fluxoCaixaService = new FluxoCaixaService();
            _produtoRepository = RepositoryFactory.CriarProdutoRepository();
        }

        /// <summary>
        /// Obtém resumo diário (vendas do dia). Delega ao RelatorioFinanceiroService.
        /// </summary>
        public (decimal totalVendas, int quantidadeVendas) ObterResumoDiario(DateTime data)
        {
            var inicio = data.Date;
            var fim = inicio.AddDays(1).AddSeconds(-1);
            var relatorio = _relatorioFinanceiroService.GerarRelatorio(inicio, fim);
            return (relatorio.TotalVendas, relatorio.QuantidadeVendas);
        }

        /// <summary>
        /// Obtém resumo mensal (vendas do mês e lucro estimado). Delega ao RelatorioFinanceiroService.
        /// </summary>
        public (decimal totalVendas, int quantidadeVendas, decimal lucroEstimado) ObterResumoMensal(DateTime referencia)
        {
            var inicio = new DateTime(referencia.Year, referencia.Month, 1);
            var fim = inicio.AddMonths(1).AddSeconds(-1);
            var relatorio = _relatorioFinanceiroService.GerarRelatorio(inicio, fim);
            var lucro = _relatorioFinanceiroService.ObterLucroEstimadoPeriodo(inicio, fim);
            return (relatorio.TotalVendas, relatorio.QuantidadeVendas, lucro);
        }

        /// <summary>
        /// Obtém produtos com estoque crítico. Delega ao ProdutoService.
        /// </summary>
        public List<Produto> ObterProdutosEstoqueCritico()
        {
            return _produtoRepository.ObterProdutosEstoqueCritico();
        }

        /// <summary>
        /// Obtém fluxo de caixa do período (para Relatórios/Dashboard). Delega ao FluxoCaixaService.
        /// </summary>
        public IEnumerable<FluxoCaixaItem> ObterFluxoCaixa(DateTime inicio, DateTime fim)
        {
            return _fluxoCaixaService.GerarFluxoCaixa(inicio, fim);
        }
    }
}
