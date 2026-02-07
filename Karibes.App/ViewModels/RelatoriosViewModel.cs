using System;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;

namespace Karibes.App.ViewModels
{
    public class RelatoriosViewModel : BaseViewModel
    {
        private readonly RelatorioFinanceiroService _relatorioService;
        private DateTime _dataInicio = DateTime.Now.AddMonths(-1);
        private DateTime _dataFim = DateTime.Now;
        private RelatorioFinanceiroConsolidado? _relatorioAtual;

        public DateTime DataInicio
        {
            get => _dataInicio;
            set
            {
                if (SetProperty(ref _dataInicio, value))
                {
                    GerarRelatorio();
                }
            }
        }

        public DateTime DataFim
        {
            get => _dataFim;
            set
            {
                if (SetProperty(ref _dataFim, value))
                {
                    GerarRelatorio();
                }
            }
        }

        public RelatorioFinanceiroConsolidado? RelatorioAtual
        {
            get => _relatorioAtual;
            set => SetProperty(ref _relatorioAtual, value);
        }

        public RelayCommand GerarRelatorioCommand { get; }

        public RelatoriosViewModel()
        {
            _relatorioService = new RelatorioFinanceiroService();
            GerarRelatorioCommand = new RelayCommand(_ => GerarRelatorio());
            
            // Gera relatório inicial
            GerarRelatorio();
        }

        /// <summary>
        /// Gera o relatório financeiro consolidado
        /// </summary>
        private void GerarRelatorio()
        {
            try
            {
                RelatorioAtual = _relatorioService.GerarRelatorio(DataInicio, DataFim);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
                // Em caso de erro, cria relatório vazio
                RelatorioAtual = new RelatorioFinanceiroConsolidado
                {
                    PeriodoInicio = DataInicio,
                    PeriodoFim = DataFim
                };
            }
        }
    }
}





