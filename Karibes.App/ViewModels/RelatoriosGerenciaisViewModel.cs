using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;

namespace Karibes.App.ViewModels
{
    /// <summary>
    /// ViewModel de Relatórios Gerenciais. Consome apenas RelatorioFinanceiroService e FluxoCaixaService.
    /// Nenhum cálculo financeiro; apenas atribuição de valores consolidados.
    /// </summary>
    public class RelatoriosGerenciaisViewModel : BaseViewModel
    {
        private readonly RelatorioFinanceiroService _relatorioFinanceiroService;
        private readonly FluxoCaixaService _fluxoCaixaService;
        private readonly ExportacaoRelatorioService _exportacaoRelatorioService;
        private readonly PdfExportService _pdfExportService;
        private readonly AuditoriaFinanceiraService _auditoriaFinanceiraService;

        private DateTime _dataInicio = DateTime.Now.AddMonths(-1).Date;
        private DateTime _dataFim = DateTime.Now.Date;
        private RelatorioFinanceiroConsolidado? _relatorioAtual;
        private ObservableCollection<FluxoCaixaItem> _fluxoCaixa = new();
        private bool _isAuditoriaOk = true;
        private string _mensagemAuditoria = string.Empty;

        public DateTime DataInicio
        {
            get => _dataInicio;
            set
            {
                if (SetProperty(ref _dataInicio, value))
                    AtualizarRelatorio();
            }
        }

        public DateTime DataFim
        {
            get => _dataFim;
            set
            {
                if (SetProperty(ref _dataFim, value))
                    AtualizarRelatorio();
            }
        }

        public RelatorioFinanceiroConsolidado? RelatorioAtual
        {
            get => _relatorioAtual;
            set
            {
                if (SetProperty(ref _relatorioAtual, value))
                {
                    OnPropertyChanged(nameof(TotalVendasTexto));
                    OnPropertyChanged(nameof(TotalRecebidoTexto));
                    OnPropertyChanged(nameof(TotalDespesasTexto));
                    OnPropertyChanged(nameof(SaldoFinalTexto));
                }
            }
        }

        public string TotalVendasTexto => FormatarMoeda(RelatorioAtual?.TotalVendas ?? 0);
        public string TotalRecebidoTexto => FormatarMoeda(RelatorioAtual?.TotalRecebido ?? 0);
        public string TotalDespesasTexto => FormatarMoeda(RelatorioAtual?.TotalDespesas ?? 0);
        public string SaldoFinalTexto => FormatarMoeda(RelatorioAtual?.SaldoFinal ?? 0);

        /// <summary>
        /// Itens do fluxo de caixa (para listagem). Fonte: FluxoCaixaService.
        /// </summary>
        public ObservableCollection<FluxoCaixaItem> FluxoCaixa
        {
            get => _fluxoCaixa;
            set => SetProperty(ref _fluxoCaixa, value);
        }

        /// <summary>
        /// Indica se a auditoria de consistência financeira passou.
        /// </summary>
        public bool IsAuditoriaOk
        {
            get => _isAuditoriaOk;
            set => SetProperty(ref _isAuditoriaOk, value);
        }

        /// <summary>
        /// Mensagem da última auditoria (consistente ou divergência).
        /// </summary>
        public string MensagemAuditoria
        {
            get => _mensagemAuditoria;
            set => SetProperty(ref _mensagemAuditoria, value);
        }

        public RelayCommand AtualizarRelatorioCommand { get; }
        public RelayCommand ExportarRelatorioCommand { get; }
        public RelayCommand ExportarFluxoCaixaCommand { get; }
        public RelayCommand ExportarPdfCommand { get; }

        public RelatoriosGerenciaisViewModel()
        {
            _relatorioFinanceiroService = new RelatorioFinanceiroService();
            _fluxoCaixaService = new FluxoCaixaService();
            _exportacaoRelatorioService = new ExportacaoRelatorioService();
            _pdfExportService = new PdfExportService();
            _auditoriaFinanceiraService = new AuditoriaFinanceiraService();
            AtualizarRelatorioCommand = new RelayCommand(_ => AtualizarRelatorio());
            ExportarRelatorioCommand = new RelayCommand(_ => ExportarRelatorio());
            ExportarFluxoCaixaCommand = new RelayCommand(_ => ExportarFluxoCaixa());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());
            AtualizarRelatorio();
        }

        private void ExportarPdf()
        {
            if (RelatorioAtual == null) return;
            try
            {
                var pasta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var caminho = Path.Combine(pasta, $"RelatorioFinanceiro_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                _pdfExportService.ExportarRelatorio(RelatorioAtual, FluxoCaixa, caminho);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao exportar PDF: {ex.Message}");
            }
        }

        private static string FormatarMoeda(decimal valor) => $"R$ {valor:N2}";

        /// <summary>
        /// Atualiza relatório e fluxo de caixa delegando aos serviços. Sem cálculo.
        /// Após atualizar, executa auditoria de consistência.
        /// </summary>
        private void AtualizarRelatorio()
        {
            try
            {
                var fimAjustado = DataFim.Date.AddDays(1).AddSeconds(-1);
                RelatorioAtual = _relatorioFinanceiroService.GerarRelatorio(DataInicio, fimAjustado);

                var itens = _fluxoCaixaService.GerarFluxoCaixa(DataInicio, fimAjustado);
                FluxoCaixa.Clear();
                foreach (var item in itens)
                {
                    FluxoCaixa.Add(item);
                }

                var (consistente, mensagem) = _auditoriaFinanceiraService.ValidarConsistencia(RelatorioAtual, FluxoCaixa);
                IsAuditoriaOk = consistente;
                MensagemAuditoria = mensagem;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar relatório gerencial: {ex.Message}");
                RelatorioAtual = new RelatorioFinanceiroConsolidado
                {
                    PeriodoInicio = DataInicio,
                    PeriodoFim = DataFim
                };
                FluxoCaixa.Clear();
                IsAuditoriaOk = false;
                MensagemAuditoria = "Erro ao carregar dados.";
            }
        }

        /// <summary>
        /// Exporta relatório consolidado para Excel. Caminho: pasta Documentos.
        /// </summary>
        private void ExportarRelatorio()
        {
            if (RelatorioAtual == null) return;
            try
            {
                var pasta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var nome = $"RelatorioConsolidado_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var caminho = Path.Combine(pasta, nome);
                _exportacaoRelatorioService.ExportarRelatorioFinanceiro(RelatorioAtual, caminho);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao exportar relatório: {ex.Message}");
            }
        }

        /// <summary>
        /// Exporta fluxo de caixa para Excel. Caminho: pasta Documentos.
        /// </summary>
        private void ExportarFluxoCaixa()
        {
            try
            {
                var pasta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var nome = $"FluxoCaixa_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var caminho = Path.Combine(pasta, nome);
                _exportacaoRelatorioService.ExportarFluxoCaixa(FluxoCaixa, caminho);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao exportar fluxo de caixa: {ex.Message}");
            }
        }
    }
}
