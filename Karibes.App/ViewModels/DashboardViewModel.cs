using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;
using System.Windows;

namespace Karibes.App.ViewModels
{
    /// <summary>
    /// ViewModel para o Dashboard principal
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DashboardService _dashboardService;
        private decimal _vendasDia;
        private int _quantidadeVendasDia;
        private decimal _vendasMes;
        private int _quantidadeVendasMes;
        private decimal _lucroEstimado;
        private ObservableCollection<Produto> _produtosEstoqueCritico = new();

        public decimal VendasDia
        {
            get => _vendasDia;
            set => SetProperty(ref _vendasDia, value);
        }

        public int QuantidadeVendasDia
        {
            get => _quantidadeVendasDia;
            set => SetProperty(ref _quantidadeVendasDia, value);
        }

        public decimal VendasMes
        {
            get => _vendasMes;
            set => SetProperty(ref _vendasMes, value);
        }

        public int QuantidadeVendasMes
        {
            get => _quantidadeVendasMes;
            set => SetProperty(ref _quantidadeVendasMes, value);
        }

        public decimal LucroEstimado
        {
            get => _lucroEstimado;
            set => SetProperty(ref _lucroEstimado, value);
        }

        public ObservableCollection<Produto> ProdutosEstoqueCritico
        {
            get => _produtosEstoqueCritico;
            set => SetProperty(ref _produtosEstoqueCritico, value);
        }

        public RelayCommand CarregarDadosCommand { get; }

        public DashboardViewModel()
        {
            _dashboardService = new DashboardService();
            ProdutosEstoqueCritico = new ObservableCollection<Produto>();

            CarregarDadosCommand = new RelayCommand(_ => AtualizarDashboard());

            // Carrega dados ao inicializar
            AtualizarDashboard();
        }

        /// <summary>
        /// Carrega todos os dados do dashboard (valores consolidados do DashboardService).
        /// Nenhum cálculo direto; apenas atribuição de valores já consolidados.
        /// </summary>
        public void AtualizarDashboard()
        {
            try
            {
                var hoje = DateTime.Now;

                var (totalVendasDia, quantidadeDia) = _dashboardService.ObterResumoDiario(hoje);
                VendasDia = totalVendasDia;
                QuantidadeVendasDia = quantidadeDia;

                var (totalVendasMes, quantidadeMes, lucro) = _dashboardService.ObterResumoMensal(hoje);
                VendasMes = totalVendasMes;
                QuantidadeVendasMes = quantidadeMes;
                LucroEstimado = lucro;

                var produtosCriticos = _dashboardService.ObterProdutosEstoqueCritico();
                ProdutosEstoqueCritico.Clear();
                foreach (var produto in produtosCriticos)
                {
                    ProdutosEstoqueCritico.Add(produto);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados do dashboard: {ex.Message}");
            }
        }
    }
}

