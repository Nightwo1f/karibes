using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Karibes.App.Data.Repositories;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;

namespace Karibes.App.ViewModels
{
    /// <summary>
    /// ViewModel para gerenciamento financeiro
    /// </summary>
    public class FinanceiroViewModel : BaseViewModel
    {
        private readonly IFinanceiroRepository _financeiroRepository;
        private readonly CalculoFinanceiroService _calculoFinanceiro;
        private readonly DashboardViewModel _dashboard;

        // Listagem
        private ObservableCollection<LancamentoFinanceiro> _lancamentos = new();
        private ObservableCollection<LancamentoFinanceiro> _lancamentosFiltrados = new();
        private LancamentoFinanceiro? _lancamentoSelecionado;

        // Filtros
        private DateTime _filtroDataInicio = DateTime.Now.AddMonths(-1);
        private DateTime _filtroDataFim = DateTime.Now;
        private string _filtroTipo = "Todos";
        private string _filtroStatus = "Todos";
        private string _filtroCategoria = string.Empty;

        // Formulário de cadastro
        private string _categoriaNova = string.Empty;
        private string _descricaoNova = string.Empty;
        private decimal _valorNovo = 0;
        private DateTime? _dataVencimentoNova;
        private string _statusNovo = Constants.StatusPendente;
        private string _formaPagamentoNova = Constants.PagamentoDinheiro;
        private string _observacoesNova = string.Empty;
        private bool _isCadastroReceita = true; // true = Receita, false = Despesa

        // Totalizadores
        private decimal _totalReceitas;
        private decimal _totalDespesas;
        private decimal _saldoCaixa;

        public ObservableCollection<LancamentoFinanceiro> Lancamentos
        {
            get => _lancamentos;
            set => SetProperty(ref _lancamentos, value);
        }

        public ObservableCollection<LancamentoFinanceiro> LancamentosFiltrados
        {
            get => _lancamentosFiltrados;
            set => SetProperty(ref _lancamentosFiltrados, value);
        }

        public LancamentoFinanceiro? LancamentoSelecionado
        {
            get => _lancamentoSelecionado;
            set
            {
                if (SetProperty(ref _lancamentoSelecionado, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public DateTime FiltroDataInicio
        {
            get => _filtroDataInicio;
            set
            {
                SetProperty(ref _filtroDataInicio, value);
                if (_isInitialized)
                    AplicarFiltros();
            }
        }

        public DateTime FiltroDataFim
        {
            get => _filtroDataFim;
            set
            {
                SetProperty(ref _filtroDataFim, value);
                if (_isInitialized)
                    AplicarFiltros();
            }
        }

        public string FiltroTipo
        {
            get => _filtroTipo;
            set
            {
                SetProperty(ref _filtroTipo, value);
                if (_isInitialized)
                    AplicarFiltros();
            }
        }

        public string FiltroStatus
        {
            get => _filtroStatus;
            set
            {
                SetProperty(ref _filtroStatus, value);
                if (_isInitialized)
                    AplicarFiltros();
            }
        }

        public string FiltroCategoria
        {
            get => _filtroCategoria;
            set
            {
                SetProperty(ref _filtroCategoria, value);
                if (_isInitialized)
                    AplicarFiltros();
            }
        }

        public string CategoriaNova
        {
            get => _categoriaNova;
            set => SetProperty(ref _categoriaNova, value);
        }

        public string DescricaoNova
        {
            get => _descricaoNova;
            set => SetProperty(ref _descricaoNova, value);
        }

        public decimal ValorNovo
        {
            get => _valorNovo;
            set => SetProperty(ref _valorNovo, value);
        }

        public DateTime? DataVencimentoNova
        {
            get => _dataVencimentoNova;
            set => SetProperty(ref _dataVencimentoNova, value);
        }

        public string StatusNovo
        {
            get => _statusNovo;
            set => SetProperty(ref _statusNovo, value);
        }

        public string FormaPagamentoNova
        {
            get => _formaPagamentoNova;
            set => SetProperty(ref _formaPagamentoNova, value);
        }

        public string ObservacoesNova
        {
            get => _observacoesNova;
            set => SetProperty(ref _observacoesNova, value);
        }

        public bool IsCadastroReceita
        {
            get => _isCadastroReceita;
            set
            {
                SetProperty(ref _isCadastroReceita, value);
                LimparFormulario();
            }
        }

        public decimal TotalReceitas
        {
            get => _totalReceitas;
            set
            {
                if (SetProperty(ref _totalReceitas, value))
                    OnPropertyChanged(nameof(TotalReceitasTexto));
            }
        }

        public decimal TotalDespesas
        {
            get => _totalDespesas;
            set
            {
                if (SetProperty(ref _totalDespesas, value))
                    OnPropertyChanged(nameof(TotalDespesasTexto));
            }
        }

        public decimal SaldoCaixa
        {
            get => _saldoCaixa;
            set
            {
                if (SetProperty(ref _saldoCaixa, value))
                    OnPropertyChanged(nameof(SaldoCaixaTexto));
            }
        }

        public string TotalReceitasTexto => FormatarMoeda(TotalReceitas);
        public string TotalDespesasTexto => FormatarMoeda(TotalDespesas);
        public string SaldoCaixaTexto => FormatarMoeda(SaldoCaixa);

        // Commands
        public RelayCommand CarregarLancamentosCommand { get; }
        public RelayCommand RegistrarReceitaCommand { get; }
        public RelayCommand RegistrarDespesaCommand { get; }
        public RelayCommand AtualizarStatusCommand { get; }
        public RelayCommand LimparFormularioCommand { get; }

        private bool _isInitialized = false;

        private static string FormatarMoeda(decimal valor) => $"R$ {valor:N2}";

        public FinanceiroViewModel(DashboardViewModel dashboard)
        {
            _dashboard = dashboard;
            _financeiroRepository = RepositoryFactory.CriarFinanceiroRepository();
            _calculoFinanceiro = new CalculoFinanceiroService();

            Lancamentos = new ObservableCollection<LancamentoFinanceiro>();
            LancamentosFiltrados = new ObservableCollection<LancamentoFinanceiro>();

            CarregarLancamentosCommand = new RelayCommand(_ => CarregarLancamentos());
            RegistrarReceitaCommand = new RelayCommand(_ => RegistrarReceita());
            RegistrarDespesaCommand = new RelayCommand(_ => RegistrarDespesa());
            AtualizarStatusCommand = new RelayCommand(AtualizarStatus, _ => LancamentoSelecionado != null);
            LimparFormularioCommand = new RelayCommand(_ => LimparFormulario());

            CarregarLancamentos();
            
            _isInitialized = true;
        }

        /// <summary>
        /// Carrega todos os lançamentos financeiros
        /// </summary>
        private void CarregarLancamentos()
        {
            try
            {
                // Carrega lançamentos do último ano para ter dados suficientes
                var inicio = DateTime.Now.AddYears(-1);
                var fim = DateTime.Now.AddDays(1);
                var lancamentos = _financeiroRepository.ObterLancamentos(inicio, fim);

                Lancamentos.Clear();
                foreach (var lancamento in lancamentos.OrderByDescending(l => l.DataLancamento))
                {
                    Lancamentos.Add(lancamento);
                }

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                // Em caso de erro, mantém lista vazia
                // Não exibe mensagem para não interromper a experiência
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar lançamentos: {ex.Message}");
                // Garante que as coleções estão inicializadas
                if (Lancamentos == null)
                    Lancamentos = new ObservableCollection<LancamentoFinanceiro>();
                if (LancamentosFiltrados == null)
                    LancamentosFiltrados = new ObservableCollection<LancamentoFinanceiro>();
                
                // Garante que os totalizadores são calculados mesmo em caso de erro
                CalcularTotalizadores();
            }
        }

        /// <summary>
        /// Aplica filtros na listagem
        /// </summary>
        private void AplicarFiltros()
        {
            try
            {
                if (Lancamentos == null || LancamentosFiltrados == null)
                    return;

                var filtrados = Lancamentos.AsQueryable();

                // Filtro por período
                filtrados = filtrados.Where(l => 
                    l.DataLancamento >= FiltroDataInicio && 
                    l.DataLancamento <= FiltroDataFim.AddDays(1));

                // Filtro por tipo
                if (FiltroTipo != "Todos")
                {
                    filtrados = filtrados.Where(l => l.Tipo == FiltroTipo);
                }

                // Filtro por status
                if (FiltroStatus != "Todos")
                {
                    filtrados = filtrados.Where(l => l.Status == FiltroStatus);
                }

                // Filtro por categoria
                if (!string.IsNullOrWhiteSpace(FiltroCategoria))
                {
                    var categoriaLower = FiltroCategoria.ToLower();
                    filtrados = filtrados.Where(l => 
                        !string.IsNullOrWhiteSpace(l.Categoria) &&
                        l.Categoria.ToLower().Contains(categoriaLower));
                }

                LancamentosFiltrados.Clear();
                foreach (var lancamento in filtrados.OrderByDescending(l => l.DataLancamento))
                {
                    LancamentosFiltrados.Add(lancamento);
                }

                CalcularTotalizadores();
            }
            catch (Exception ex)
            {
                // Em caso de erro, mantém lista vazia
                // Não exibe mensagem para não interromper a experiência
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar filtros financeiros: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcula os totalizadores
        /// </summary>
        private void CalcularTotalizadores()
        {
            try
            {
                if (LancamentosFiltrados == null)
                {
                    TotalReceitas = 0;
                    TotalDespesas = 0;
                    SaldoCaixa = 0;
                    return;
                }

                var (totalReceitas, totalDespesas, saldoCaixa) = _calculoFinanceiro.CalcularTotalizadoresLancamentos(LancamentosFiltrados);
                TotalReceitas = totalReceitas;
                TotalDespesas = totalDespesas;
                SaldoCaixa = saldoCaixa;
            }
            catch (Exception ex)
            {
                // Em caso de erro, zera os totalizadores
                System.Diagnostics.Debug.WriteLine($"Erro ao calcular totalizadores: {ex.Message}");
                TotalReceitas = 0;
                TotalDespesas = 0;
                SaldoCaixa = 0;
            }
        }

        /// <summary>
        /// Registra uma nova receita
        /// </summary>
        private void RegistrarReceita()
        {
            if (!ValidarFormulario())
                return;

            try
            {
                var lancamento = new LancamentoFinanceiro
                {
                    Categoria = CategoriaNova,
                    Descricao = DescricaoNova,
                    Valor = ValorNovo,
                    DataLancamento = DateTime.Now,
                    DataVencimento = DataVencimentoNova,
                    DataPagamento = StatusNovo == Constants.StatusPago ? DateTime.Now : (DateTime?)null,
                    Status = StatusNovo,
                    FormaPagamento = FormaPagamentoNova,
                    Origem = "Manual",
                    Observacoes = ObservacoesNova
                };

                _financeiroRepository.RegistrarReceita(lancamento);
                _dashboard.AtualizarDashboard();
                MessageBox.Show("Receita registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                
                LimparFormulario();
                CarregarLancamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar receita: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Registra uma nova despesa
        /// </summary>
        private void RegistrarDespesa()
        {
            if (!ValidarFormulario())
                return;

            try
            {
                var lancamento = new LancamentoFinanceiro
                {
                    Categoria = CategoriaNova,
                    Descricao = DescricaoNova,
                    Valor = ValorNovo,
                    DataLancamento = DateTime.Now,
                    DataVencimento = DataVencimentoNova,
                    DataPagamento = StatusNovo == Constants.StatusPago ? DateTime.Now : (DateTime?)null,
                    Status = StatusNovo,
                    FormaPagamento = FormaPagamentoNova,
                    Origem = "Manual",
                    Observacoes = ObservacoesNova
                };

                _financeiroRepository.RegistrarDespesa(lancamento);
                _dashboard.AtualizarDashboard();
                MessageBox.Show("Despesa registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                
                LimparFormulario();
                CarregarLancamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar despesa: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Atualiza o status de um lançamento
        /// </summary>
        private void AtualizarStatus(object? parameter)
        {
            if (LancamentoSelecionado == null) return;

            if (LancamentoSelecionado.Status == Constants.StatusPago)
            {
                MessageBox.Show("Este lançamento já está pago.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"Deseja marcar este lançamento como pago?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _financeiroRepository.AtualizarStatus(LancamentoSelecionado.Id, Constants.StatusPago);
                    _dashboard.AtualizarDashboard();
                    MessageBox.Show("Status atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    CarregarLancamentos();
                    LancamentoSelecionado = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao atualizar status: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Valida o formulário antes de salvar
        /// </summary>
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(CategoriaNova))
            {
                MessageBox.Show("Informe a categoria.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescricaoNova))
            {
                MessageBox.Show("Informe a descrição.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ValorNovo <= 0)
            {
                MessageBox.Show("O valor deve ser maior que zero.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Limpa o formulário de cadastro
        /// </summary>
        private void LimparFormulario()
        {
            CategoriaNova = string.Empty;
            DescricaoNova = string.Empty;
            ValorNovo = 0;
            DataVencimentoNova = null;
            StatusNovo = Constants.StatusPendente;
            FormaPagamentoNova = Constants.PagamentoDinheiro;
            ObservacoesNova = string.Empty;
        }
    }
}
