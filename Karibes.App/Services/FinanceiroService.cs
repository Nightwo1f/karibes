using OfficeOpenXml;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço responsável pelo controle financeiro do sistema
    /// </summary>
    public class FinanceiroService
    {
        private readonly ExcelService _excelService;
        private readonly CreditoService _creditoService;
        private readonly CalculoFinanceiroService _calculoFinanceiro;
        private const string LancamentosSheetName = "Lancamentos";

        public FinanceiroService(ExcelService excelService)
        {
            _excelService = excelService;
            _creditoService = new CreditoService();
            _calculoFinanceiro = new CalculoFinanceiroService();
            InicializarArquivo();
        }

        /// <summary>
        /// Registra uma receita
        /// </summary>
        /// <param name="lancamento">Lançamento financeiro do tipo Receita</param>
        public void RegistrarReceita(LancamentoFinanceiro lancamento)
        {
            // Validações
            if (lancamento == null)
                throw new ArgumentNullException(nameof(lancamento));

            if (lancamento.Valor <= 0)
                throw new ArgumentException("Valor da receita deve ser maior que zero.", nameof(lancamento));

            // Configurar receita
            lancamento.Tipo = Constants.TipoReceita;
            lancamento.Status = Constants.StatusPago; // Status inicial = Pago
            lancamento.DataLancamento = lancamento.DataLancamento == default ? DateTime.Now : lancamento.DataLancamento;
            lancamento.DataPagamento = lancamento.DataPagamento ?? lancamento.DataLancamento;

            // Salvar lançamento
            SalvarLancamento(lancamento);
        }

        /// <summary>
        /// Registra uma despesa
        /// </summary>
        /// <param name="lancamento">Lançamento financeiro do tipo Despesa</param>
        public void RegistrarDespesa(LancamentoFinanceiro lancamento)
        {
            // Validações
            if (lancamento == null)
                throw new ArgumentNullException(nameof(lancamento));

            if (lancamento.Valor <= 0)
                throw new ArgumentException("Valor da despesa deve ser maior que zero.", nameof(lancamento));

            // Configurar despesa
            lancamento.Tipo = Constants.TipoDespesa;
            lancamento.DataLancamento = lancamento.DataLancamento == default ? DateTime.Now : lancamento.DataLancamento;
            
            // Status pode ser Pendente ou Pago (mantém o informado ou padrão)
            if (string.IsNullOrWhiteSpace(lancamento.Status))
                lancamento.Status = Constants.StatusPendente;

            // Salvar lançamento
            SalvarLancamento(lancamento);
        }

        /// <summary>
        /// Registra pagamento de fiado e gera lançamento financeiro
        /// </summary>
        /// <param name="cliente">Cliente que está pagando</param>
        /// <param name="valor">Valor do pagamento</param>
        /// <param name="observacao">Observação sobre o pagamento</param>
        public void RegistrarPagamentoFiado(
            Cliente cliente,
            decimal valor,
            string observacao = "",
            DateTime? dataPagamento = null,
            string formaPagamento = Constants.PagamentoDinheiro)
        {
            // Validações
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (valor <= 0)
                throw new ArgumentException("Valor do pagamento deve ser maior que zero.", nameof(valor));

            // Delegar crédito ao CreditoService
            _creditoService.RegistrarPagamento(cliente, valor, observacao);

            // Gerar lançamento financeiro do tipo Receita
            var dataEfetiva = dataPagamento ?? DateTime.Now;
            var lancamento = new LancamentoFinanceiro
            {
                Tipo = Constants.TipoReceita,
                Categoria = "Recebimento de Crédito",
                Descricao = $"Pagamento de fiado - Cliente: {cliente.Nome}",
                Valor = valor,
                DataLancamento = dataEfetiva,
                DataPagamento = dataEfetiva,
                Status = Constants.StatusPago,
                FormaPagamento = formaPagamento,
                Origem = "Pagamento de fiado",
                OrigemId = cliente.Id,
                Observacoes = string.IsNullOrWhiteSpace(observacao) 
                    ? $"Pagamento de crédito do cliente {cliente.Nome}" 
                    : observacao
            };

            SalvarLancamento(lancamento);
        }

        /// <summary>
        /// Atualiza o status de um lançamento financeiro
        /// </summary>
        /// <param name="lancamentoId">ID do lançamento</param>
        /// <param name="novoStatus">Novo status (Pendente, Pago, Cancelado)</param>
        public void AtualizarStatus(int lancamentoId, string novoStatus)
        {
            if (!_excelService.FileExists(Constants.FinanceiroFile))
                throw new FileNotFoundException("Arquivo financeiro não encontrado.");

            using var package = _excelService.GetPackage(Constants.FinanceiroFile);
            if (package == null)
                throw new InvalidOperationException("Não foi possível abrir o arquivo financeiro.");

            var worksheet = package.Workbook.Worksheets[LancamentosSheetName];
            if (worksheet == null || worksheet.Dimension == null)
                throw new InvalidOperationException("Planilha de lançamentos não encontrada.");

            int rowCount = worksheet.Dimension.End.Row;

            // Encontra a linha do lançamento
            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id == lancamentoId)
                {
                    worksheet.Cells[row, 9].Value = novoStatus; // Status na coluna 9
                    
                    // Se status for Pago e DataPagamento estiver vazia, preencher
                    if (novoStatus == Constants.StatusPago)
                    {
                        var dataPagamento = worksheet.Cells[row, 8].GetValue<DateTime?>();
                        if (dataPagamento == null)
                        {
                            worksheet.Cells[row, 8].Value = DateTime.Now;
                        }
                    }

                    _excelService.SavePackage(package, Constants.FinanceiroFile);
                    return;
                }
            }

            throw new KeyNotFoundException($"Lançamento com ID {lancamentoId} não encontrado.");
        }

        /// <summary>
        /// Obtém o saldo atual do caixa
        /// </summary>
        /// <param name="ate">Data limite para cálculo (null = todas as datas)</param>
        /// <returns>Saldo do caixa (receitas pagas - despesas pagas)</returns>
        public decimal ObterSaldoCaixa(DateTime? ate = null)
        {
            var lancamentos = ObterLancamentos(
                DateTime.MinValue,
                ate ?? DateTime.MaxValue);
            return _calculoFinanceiro.CalcularSaldoCaixa(lancamentos);
        }

        /// <summary>
        /// Obtém lançamentos financeiros em um período
        /// </summary>
        /// <param name="inicio">Data inicial</param>
        /// <param name="fim">Data final</param>
        /// <returns>Lista de lançamentos</returns>
        public List<LancamentoFinanceiro> ObterLancamentos(DateTime inicio, DateTime fim)
        {
            var lancamentos = new List<LancamentoFinanceiro>();

            if (!_excelService.FileExists(Constants.FinanceiroFile))
                return lancamentos;

            using var package = _excelService.GetPackage(Constants.FinanceiroFile);
            if (package == null) return lancamentos;

            var worksheet = package.Workbook.Worksheets[LancamentosSheetName];
            if (worksheet == null || worksheet.Dimension == null) return lancamentos;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var dataLancamento = worksheet.Cells[row, 6].GetValue<DateTime>();
                    if (dataLancamento < inicio || dataLancamento > fim)
                        continue;

                    var lancamento = LerLancamentoDaLinha(worksheet, row);
                    if (lancamento != null)
                        lancamentos.Add(lancamento);
                }
                catch
                {
                    continue;
                }
            }

            return lancamentos;
        }

        /// <summary>
        /// Obtém lançamentos com data de pagamento efetiva no período (Status Pago).
        /// Usado para fluxo de caixa (entradas e saídas reais).
        /// </summary>
        /// <param name="inicio">Data inicial (inclusive)</param>
        /// <param name="fim">Data final (inclusive)</param>
        /// <returns>Lançamentos pagos cuja data de pagamento está no período</returns>
        public List<LancamentoFinanceiro> ObterLancamentosComPagamentoNoPeriodo(DateTime inicio, DateTime fim)
        {
            var lancamentos = new List<LancamentoFinanceiro>();

            if (!_excelService.FileExists(Constants.FinanceiroFile))
                return lancamentos;

            using var package = _excelService.GetPackage(Constants.FinanceiroFile);
            if (package == null) return lancamentos;

            var worksheet = package.Workbook.Worksheets[LancamentosSheetName];
            if (worksheet == null || worksheet.Dimension == null) return lancamentos;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var lancamento = LerLancamentoDaLinha(worksheet, row);
                    if (lancamento == null || lancamento.Status != Constants.StatusPago)
                        continue;

                    var dataPagamento = lancamento.DataPagamento ?? lancamento.DataLancamento;
                    if (dataPagamento < inicio || dataPagamento > fim)
                        continue;

                    lancamentos.Add(lancamento);
                }
                catch
                {
                    continue;
                }
            }

            return lancamentos;
        }

        /// <summary>
        /// Registra um lançamento financeiro (método genérico mantido para compatibilidade)
        /// </summary>
        /// <param name="lancamento">Lançamento a ser registrado</param>
        public void RegistrarLancamento(LancamentoFinanceiro lancamento)
        {
            if (lancamento == null)
                throw new ArgumentNullException(nameof(lancamento));

            // Determina o tipo e chama o método específico
            if (lancamento.Tipo == Constants.TipoReceita)
            {
                RegistrarReceita(lancamento);
            }
            else if (lancamento.Tipo == Constants.TipoDespesa)
            {
                RegistrarDespesa(lancamento);
            }
            else
            {
                throw new ArgumentException("Tipo de lançamento inválido. Deve ser Receita ou Despesa.");
            }
        }

        /// <summary>
        /// Calcula o saldo em um período (método mantido para compatibilidade)
        /// </summary>
        /// <param name="dataInicio">Data inicial</param>
        /// <param name="dataFim">Data final</param>
        /// <returns>Saldo do período</returns>
        public decimal CalcularSaldo(DateTime dataInicio, DateTime dataFim)
        {
            return ObterSaldoCaixa(dataFim);
        }

        /// <summary>
        /// Gera balanço mensal (método mantido para compatibilidade)
        /// </summary>
        /// <param name="ano">Ano do balanço</param>
        /// <param name="mes">Mês do balanço</param>
        /// <returns>Balanço mensal</returns>
        public BalancoMensal GerarBalancoMensal(int ano, int mes)
        {
            var inicioMes = new DateTime(ano, mes, 1);
            var fimMes = inicioMes.AddMonths(1).AddSeconds(-1);

            var lancamentos = ObterLancamentos(inicioMes, fimMes);
            var (receitaTotal, despesaTotal) = _calculoFinanceiro.CalcularReceitasEDespesasPagas(lancamentos);

            return new BalancoMensal
            {
                Ano = ano,
                Mes = mes,
                ReceitaTotal = receitaTotal,
                DespesaTotal = despesaTotal,
                Lucro = receitaTotal - despesaTotal,
                DataGeracao = DateTime.Now
            };
        }

        /// <summary>
        /// Inicializa o arquivo de financeiro com cabeçalhos se não existir
        /// </summary>
        private void InicializarArquivo()
        {
            if (!_excelService.FileExists(Constants.FinanceiroFile))
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(LancamentosSheetName);

                // Cabeçalhos conforme documentação
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Tipo";
                worksheet.Cells[1, 3].Value = "Categoria";
                worksheet.Cells[1, 4].Value = "Descricao";
                worksheet.Cells[1, 5].Value = "Valor";
                worksheet.Cells[1, 6].Value = "DataLancamento";
                worksheet.Cells[1, 7].Value = "DataVencimento";
                worksheet.Cells[1, 8].Value = "DataPagamento";
                worksheet.Cells[1, 9].Value = "Status";
                worksheet.Cells[1, 10].Value = "FormaPagamento";
                worksheet.Cells[1, 11].Value = "Origem";
                worksheet.Cells[1, 12].Value = "OrigemId";
                worksheet.Cells[1, 13].Value = "Observacoes";

                // Formatação do cabeçalho
                using (var range = worksheet.Cells[1, 1, 1, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                _excelService.SavePackage(package, Constants.FinanceiroFile);
            }
        }

        /// <summary>
        /// Salva um lançamento financeiro no Excel
        /// </summary>
        private void SalvarLancamento(LancamentoFinanceiro lancamento)
        {
            using var package = _excelService.GetPackage(Constants.FinanceiroFile) ?? new ExcelPackage();
            var worksheet = package.Workbook.Worksheets[LancamentosSheetName] ?? package.Workbook.Worksheets.Add(LancamentosSheetName);

            // Obtém próximo ID
            int novoId = ObterProximoId(worksheet);
            lancamento.Id = novoId;

            // Encontra próxima linha vazia
            int novaLinha = _excelService.GetNextRow(worksheet);

            // Escreve dados
            worksheet.Cells[novaLinha, 1].Value = lancamento.Id;
            worksheet.Cells[novaLinha, 2].Value = lancamento.Tipo;
            worksheet.Cells[novaLinha, 3].Value = lancamento.Categoria;
            worksheet.Cells[novaLinha, 4].Value = lancamento.Descricao;
            worksheet.Cells[novaLinha, 5].Value = lancamento.Valor;
            worksheet.Cells[novaLinha, 6].Value = lancamento.DataLancamento;
            
            // Datas opcionais
            if (lancamento.DataVencimento.HasValue)
                worksheet.Cells[novaLinha, 7].Value = lancamento.DataVencimento.Value;
            
            if (lancamento.DataPagamento.HasValue)
                worksheet.Cells[novaLinha, 8].Value = lancamento.DataPagamento.Value;
            
            worksheet.Cells[novaLinha, 9].Value = lancamento.Status;
            worksheet.Cells[novaLinha, 10].Value = lancamento.FormaPagamento;
            worksheet.Cells[novaLinha, 11].Value = lancamento.Origem;
            
            // OrigemId opcional
            if (lancamento.OrigemId.HasValue)
                worksheet.Cells[novaLinha, 12].Value = lancamento.OrigemId.Value;
            
            worksheet.Cells[novaLinha, 13].Value = lancamento.Observacoes;

            _excelService.SavePackage(package, Constants.FinanceiroFile);
        }

        /// <summary>
        /// Lê um lançamento de uma linha do Excel
        /// </summary>
        private LancamentoFinanceiro? LerLancamentoDaLinha(ExcelWorksheet worksheet, int row)
        {
            try
            {
                return new LancamentoFinanceiro
                {
                    Id = worksheet.Cells[row, 1].GetValue<int>(),
                    Tipo = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty,
                    Categoria = worksheet.Cells[row, 3].GetValue<string>() ?? string.Empty,
                    Descricao = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
                    Valor = worksheet.Cells[row, 5].GetValue<decimal>(),
                    DataLancamento = worksheet.Cells[row, 6].GetValue<DateTime>(),
                    DataVencimento = worksheet.Cells[row, 7].GetValue<DateTime?>(),
                    DataPagamento = worksheet.Cells[row, 8].GetValue<DateTime?>(),
                    Status = worksheet.Cells[row, 9].GetValue<string>() ?? Constants.StatusPendente,
                    FormaPagamento = worksheet.Cells[row, 10].GetValue<string>() ?? string.Empty,
                    Origem = worksheet.Cells[row, 11].GetValue<string>() ?? string.Empty,
                    OrigemId = worksheet.Cells[row, 12].GetValue<int?>(),
                    Observacoes = worksheet.Cells[row, 13].GetValue<string>() ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtém o próximo ID disponível
        /// </summary>
        private int ObterProximoId(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension == null)
                return 1;

            int maxId = 0;
            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id.HasValue && id.Value > maxId)
                    maxId = id.Value;
            }

            return maxId + 1;
        }
    }
}
