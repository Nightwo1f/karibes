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
        private readonly FinanceiroViewModel _financeiroViewModel;
        private readonly RelatoriosViewModel _relatoriosViewModel;

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
        public RelayCommand NavigateToFinanceiroCommand { get; }
        public RelayCommand NavigateToRelatoriosCommand { get; }
        public RelayCommand AlterarTemaCommand { get; }

        public MainViewModel()
        {
            _temaService = new TemaService();


            _dashboardViewModel = new DashboardViewModel();
            _produtosViewModel = new ProdutosViewModel(_dashboardViewModel);
            _clientesViewModel = new ClientesViewModel(_dashboardViewModel);
            _vendasViewModel = new VendasViewModel(_dashboardViewModel);
            _financeiroViewModel = new FinanceiroViewModel(_dashboardViewModel);
            _relatoriosViewModel = new RelatoriosViewModel();

            // Inicializa com Dashboard
            CurrentViewModel = _dashboardViewModel;
            AbaAtiva = "Dashboard";
            
            // Configura commands de navegação
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateToDashboard());
            NavigateToProdutosCommand = new RelayCommand(_ => NavigateToProdutos());
            NavigateToClientesCommand = new RelayCommand(_ => NavigateToClientes());
            NavigateToVendasCommand = new RelayCommand(_ => NavigateToVendas());
            NavigateToFinanceiroCommand = new RelayCommand(_ => NavigateToFinanceiro());
            NavigateToRelatoriosCommand = new RelayCommand(_ => NavigateToRelatorios());
            
            AlterarTemaCommand = new RelayCommand(AlterarTema);
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

        private void NavigateToFinanceiro()
        {
            CurrentViewModel = _financeiroViewModel;
            AbaAtiva = "Financeiro";
        }

        private void NavigateToRelatorios()
        {
            CurrentViewModel = _relatoriosViewModel;
            AbaAtiva = "Relatorios";
        }

        private void AlterarTema(object? parameter)
        {
            if (parameter is string tema)
            {
                _temaService.AplicarTema(tema);
            }
        }
    }
}

