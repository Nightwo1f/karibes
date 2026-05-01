using Karibes.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço de auditoria de consistência financeira. Apenas compara valores já consolidados; não altera dados.
    /// </summary>
    public class AuditoriaFinanceiraService
    {
        private const decimal Tolerancia = 0.01m;
        private const string TipoEntrada = "Entrada";
        private const string TipoSaida = "Saída";

        /// <summary>
        /// Valida consistência entre Fluxo de Caixa (Entradas − Saídas) e Saldo do Relatório Consolidado.
        /// Não altera dados; não lança exceção em caso de divergência (apenas audita).
        /// </summary>
        /// <param name="relatorio">Relatório consolidado (fonte: RelatorioFinanceiroService)</param>
        /// <param name="fluxo">Itens do fluxo de caixa (fonte: FluxoCaixaService)</param>
        /// <returns>(consistente, mensagem)</returns>
        public (bool consistente, string mensagem) ValidarConsistencia(
            RelatorioFinanceiroConsolidado? relatorio,
            IEnumerable<FluxoCaixaItem>? fluxo)
        {
            if (relatorio == null)
            {
                System.Diagnostics.Debug.WriteLine("AuditoriaFinanceira: Relatório nulo.");
                return (false, "Relatório não disponível.");
            }

            var lista = fluxo?.ToList() ?? new List<FluxoCaixaItem>();
            decimal totalEntradas = lista.Where(x => string.Equals(x.Tipo, TipoEntrada, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Valor);
            decimal totalSaidas = lista.Where(x => string.Equals(x.Tipo, TipoSaida, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Valor);
            decimal saldoFluxo = totalEntradas - totalSaidas;
            decimal saldoRelatorio = relatorio.SaldoFinal;
            decimal diferenca = Math.Abs(saldoFluxo - saldoRelatorio);

            if (diferenca > Tolerancia)
            {
                string msg = $"Divergência: Saldo Fluxo = {saldoFluxo:N2}, Saldo Relatório = {saldoRelatorio:N2}, Diferença = {diferenca:N2}.";
                System.Diagnostics.Debug.WriteLine($"AuditoriaFinanceira: {msg}");
                return (false, msg);
            }

            return (true, "Consistente.");
        }
    }
}
