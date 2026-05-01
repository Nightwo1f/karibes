using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Karibes.App.Utils;
using Karibes.App.Services;

namespace Karibes.App.ViewModels
{
    /// <summary>
    /// ViewModel principal que gerencia a navegação entre módulos
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel? _currentViewModel;
        private readonly TemaService _temaService;
        private string _abaAtiva = "Dashboard";
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly ProdutosViewModel _produtosViewModel;
        private readonly ClientesViewModel _clientesViewModel;
        private readonly VendasViewModel _vendasViewModel;
        private readonly TrocaDevolucaoViewModel _trocaDevolucaoViewModel;
        private readonly FinanceiroViewModel _financeiroViewModel;
        private readonly RelatoriosGerenciaisViewModel _relatoriosGerenciaisViewModel;
        private readonly BackupService _backupService;

        /// <summary>
        /// ViewModel atual exibido no ContentControl
        /// </summary>
        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        /// <summary>
        /// Aba atualmente ativa para destacar no menu
        /// </summary>
        public string AbaAtiva
        {
            get => _abaAtiva;
            set => SetProperty(ref _abaAtiva, value);
        }

        // Commands de navegação
        public RelayCommand NavigateToDashboardCommand { get; }
        public RelayCommand NavigateToProdutosCommand { get; }
        public RelayCommand NavigateToClientesCommand { get; }
        public RelayCommand NavigateToVendasCommand { get; }
        public RelayCommand NavigateToTrocasCommand { get; }
        public RelayCommand NavigateToFinanceiroCommand { get; }
        public RelayCommand NavigateToRelatoriosCommand { get; }
        public RelayCommand AlterarTemaCommand { get; }
        public RelayCommand AbrirPastaBancoCommand { get; }
        public RelayCommand CriarBackupBancoCommand { get; }

        public string BancoDadosPath => _backupService.DatabasePath;
        public string BackupPath => _backupService.BackupDirectory;

        public MainViewModel()
        {
            _temaService = new TemaService();
            _backupService = new BackupService();


            _dashboardViewModel = new DashboardViewModel();
            _produtosViewModel = new ProdutosViewModel(_dashboardViewModel);
            _clientesViewModel = new ClientesViewModel(_dashboardViewModel);
            _vendasViewModel = new VendasViewModel(_dashboardViewModel);
            _trocaDevolucaoViewModel = new TrocaDevolucaoViewModel();
            _financeiroViewModel = new FinanceiroViewModel(_dashboardViewModel);
            _relatoriosGerenciaisViewModel = new RelatoriosGerenciaisViewModel();

            // Inicializa com Dashboard
            CurrentViewModel = _dashboardViewModel;
            AbaAtiva = "Dashboard";
            
            // Configura commands de navegação
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateToDashboard());
            NavigateToProdutosCommand = new RelayCommand(_ => NavigateToProdutos());
            NavigateToClientesCommand = new RelayCommand(_ => NavigateToClientes());
            NavigateToVendasCommand = new RelayCommand(_ => NavigateToVendas());
            NavigateToTrocasCommand = new RelayCommand(_ => NavigateToTrocas());
            NavigateToFinanceiroCommand = new RelayCommand(_ => NavigateToFinanceiro());
            NavigateToRelatoriosCommand = new RelayCommand(_ => NavigateToRelatorios());
            
            AlterarTemaCommand = new RelayCommand(AlterarTema);
            AbrirPastaBancoCommand = new RelayCommand(_ => AbrirPastaBanco());
            CriarBackupBancoCommand = new RelayCommand(_ => CriarBackupBanco());
        }

        // Métodos de navegação
        private void NavigateToDashboard()
        {
            CurrentViewModel = _dashboardViewModel;
            AbaAtiva = "Dashboard";
        }

        private void NavigateToProdutos()
        {
            CurrentViewModel = _produtosViewModel;
            AbaAtiva = "Produtos";
        }

        private void NavigateToClientes()
        {
            CurrentViewModel = _clientesViewModel;
            AbaAtiva = "Clientes";
        }

        private void NavigateToVendas()
        {
            CurrentViewModel = _vendasViewModel;
            AbaAtiva = "Vendas";
        }

        private void NavigateToTrocas()
        {
            CurrentViewModel = _trocaDevolucaoViewModel;
            AbaAtiva = "Trocas";
        }

        private void NavigateToFinanceiro()
        {
            CurrentViewModel = _financeiroViewModel;
            AbaAtiva = "Financeiro";
        }

        private void NavigateToRelatorios()
        {
            CurrentViewModel = _relatoriosGerenciaisViewModel;
            AbaAtiva = "Relatorios";
        }

        private void AlterarTema(object? parameter)
        {
            if (parameter is string tema)
            {
                _temaService.AplicarTema(tema);
            }
        }

        private void AbrirPastaBanco()
        {
            Directory.CreateDirectory(_backupService.DatabaseDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = _backupService.DatabaseDirectory,
                UseShellExecute = true
            });
        }

        private void CriarBackupBanco()
        {
            try
            {
                var backupPath = _backupService.CriarBackupSqlite();
                MessageBox.Show(
                    $"Backup criado em:\n{backupPath}",
                    "Backup do banco",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao criar backup: {ex.Message}",
                    "Backup do banco",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
