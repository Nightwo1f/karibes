using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// ViewModel para gerenciamento de vendas
    /// </summary>
    public class VendasViewModel : BaseViewModel
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly CalculoFinanceiroService _calculoFinanceiro;
        private readonly DashboardViewModel _dashboard;

        // Listagem de vendas
        private ObservableCollection<Venda> _vendas = new();
        private Venda? _vendaSelecionada;
        private DateTime _filtroDataInicio = DateTime.Now.AddMonths(-1);
        private DateTime _filtroDataFim = DateTime.Now;
        private string _filtroCliente = string.Empty;
        private string _filtroFormaPagamento = "Todas";
        private string _filtroStatus = "Todas";

        // Nova venda
        private Cliente? _clienteSelecionado;
        private ObservableCollection<ItemVenda> _itensCarrinho = new();
        private Produto? _produtoSelecionado;
        private int _quantidadeAdicionar = 1;
        private string _formaPagamento = Constants.PagamentoDinheiro;
        private decimal _descontoVenda = 0;
        private string _vendedor = string.Empty;
        private string _observacoesVenda = string.Empty;

        // Produtos e clientes para busca
        private ObservableCollection<Produto> _produtosDisponiveis = new();
        private ObservableCollection<Cliente> _clientesDisponiveis = new();
        private string _buscaProduto = string.Empty;
        private string _buscaCliente = string.Empty;

        public ObservableCollection<Venda> Vendas
        {
            get => _vendas;
            set => SetProperty(ref _vendas, value);
        }

        public Venda? VendaSelecionada
        {
            get => _vendaSelecionada;
            set
            {
                if (SetProperty(ref _vendaSelecionada, value))
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

        public string FiltroCliente
        {
            get => _filtroCliente;
            set
            {
                SetProperty(ref _filtroCliente, value);
                if (_isInitialized)
                    AplicarFiltros();
            }
        }

        public string FiltroFormaPagamento
        {
            get => _filtroFormaPagamento;
            set
            {
                SetProperty(ref _filtroFormaPagamento, value);
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

        public Cliente? ClienteSelecionado
        {
            get => _clienteSelecionado;
            set
            {
                if (SetProperty(ref _clienteSelecionado, value))
                {
                    OnPropertyChanged(nameof(PodeVenderFiado));
                }
            }
        }

        public ObservableCollection<ItemVenda> ItensCarrinho
        {
            get => _itensCarrinho;
            set
            {
                // Remove handler antigo se houver
                if (_itensCarrinho != null)
                {
                    _itensCarrinho.CollectionChanged -= ItensCarrinho_CollectionChanged;
                }
                
                SetProperty(ref _itensCarrinho, value);
                
                // Adiciona handler para notificar mudanças nos totais
                if (_itensCarrinho != null)
                {
                    _itensCarrinho.CollectionChanged += ItensCarrinho_CollectionChanged;
                }
                
                CalcularTotalVenda();
            }
        }
        
        private void ItensCarrinho_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CalcularTotalVenda();
            
            // Se itens foram adicionados ou removidos, adiciona handlers para mudanças de propriedade
            if (e.NewItems != null)
            {
                foreach (ItemVenda item in e.NewItems)
                {
                    if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                    {
                        notifyItem.PropertyChanged += (s, args) =>
                        {
                            if (args.PropertyName == nameof(ItemVenda.Quantidade) || 
                                args.PropertyName == nameof(ItemVenda.PrecoUnitario) ||
                                args.PropertyName == nameof(ItemVenda.Desconto) ||
                                args.PropertyName == nameof(ItemVenda.ValorTotal))
                            {
                                CalcularTotalVenda();
                            }
                        };
                    }
                }
            }
        }

        public Produto? ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set => SetProperty(ref _produtoSelecionado, value);
        }

        public int QuantidadeAdicionar
        {
            get => _quantidadeAdicionar;
            set => SetProperty(ref _quantidadeAdicionar, value);
        }

        public string FormaPagamento
        {
            get => _formaPagamento;
            set
            {
                if (SetProperty(ref _formaPagamento, value))
                {
                    OnPropertyChanged(nameof(PodeVenderFiado));
                }
            }
        }

        public decimal DescontoVenda
        {
            get => _descontoVenda;
            set
            {
                if (SetProperty(ref _descontoVenda, value))
                {
                    CalcularTotalVenda();
                    OnPropertyChanged(nameof(SubtotalVenda));
                    OnPropertyChanged(nameof(TotalVenda));
                }
            }
        }

        public string Vendedor
        {
            get => _vendedor;
            set => SetProperty(ref _vendedor, value);
        }

        public string ObservacoesVenda
        {
            get => _observacoesVenda;
            set => SetProperty(ref _observacoesVenda, value);
        }

        public ObservableCollection<Produto> ProdutosDisponiveis
        {
            get => _produtosDisponiveis;
            set => SetProperty(ref _produtosDisponiveis, value);
        }

        public ObservableCollection<Cliente> ClientesDisponiveis
        {
            get => _clientesDisponiveis;
            set => SetProperty(ref _clientesDisponiveis, value);
        }

        public string BuscaProduto
        {
            get => _buscaProduto;
            set
            {
                SetProperty(ref _buscaProduto, value);
                FiltrarProdutos();
            }
        }

        public string BuscaCliente
        {
            get => _buscaCliente;
            set
            {
                SetProperty(ref _buscaCliente, value);
                FiltrarClientes();
            }
        }

        public decimal SubtotalVenda => _calculoFinanceiro.CalcularSubtotalItens(ItensCarrinho);
        public decimal TotalVenda => _calculoFinanceiro.CalcularTotalComDesconto(SubtotalVenda, DescontoVenda);
        public string SubtotalVendaTexto => SubtotalVenda.ToString("C2");
        public string TotalVendaTexto => TotalVenda.ToString("C2");
        public bool PodeVenderFiado
        {
            get
            {
                try
                {
                    if (FormaPagamento != Constants.PagamentoCredito || ClienteSelecionado == null)
                        return false;
                    return ClienteSelecionado.Ativo &&
                           ClienteSelecionado.SaldoDevedor + TotalVenda <= ClienteSelecionado.LimiteCredito;
                }
                catch
                {
                    return false;
                }
            }
        }

        // Commands
        public RelayCommand CarregarVendasCommand { get; }
        public RelayCommand NovaVendaCommand { get; }
        public RelayCommand AdicionarItemCarrinhoCommand { get; }
        public RelayCommand RemoverItemCarrinhoCommand { get; }
        public RelayCommand ConfirmarVendaCommand { get; }
        public RelayCommand CancelarVendaCommand { get; }
        public RelayCommand VerDetalhesVendaCommand { get; }
        public RelayCommand LimparCarrinhoCommand { get; }

        private bool _isInitialized = false;

        public VendasViewModel(DashboardViewModel dashboard)
        {
            _vendaRepository = RepositoryFactory.CriarVendaRepository();
            _produtoRepository = RepositoryFactory.CriarProdutoRepository();
            _clienteRepository = RepositoryFactory.CriarClienteRepository();
            _calculoFinanceiro = new CalculoFinanceiroService();
            _dashboard = dashboard;

            Vendas = new ObservableCollection<Venda>();
            _itensCarrinho = new ObservableCollection<ItemVenda>();
            _itensCarrinho.CollectionChanged += ItensCarrinho_CollectionChanged;
            ProdutosDisponiveis = new ObservableCollection<Produto>();
            ClientesDisponiveis = new ObservableCollection<Cliente>();
            
            // Notifica valores iniciais das propriedades calculadas
            CalcularTotalVenda();

            CarregarVendasCommand = new RelayCommand(_ => CarregarVendas());
            NovaVendaCommand = new RelayCommand(_ => NovaVenda());
            AdicionarItemCarrinhoCommand = new RelayCommand(_ => AdicionarItemCarrinho());
            RemoverItemCarrinhoCommand = new RelayCommand(RemoverItemCarrinho);
            ConfirmarVendaCommand = new RelayCommand(_ => ConfirmarVenda(), _ => ItensCarrinho.Count > 0);
            CancelarVendaCommand = new RelayCommand(CancelarVenda, _ => VendaSelecionada != null);
            VerDetalhesVendaCommand = new RelayCommand(_ => VerDetalhesVenda(), _ => VendaSelecionada != null);
            LimparCarrinhoCommand = new RelayCommand(_ => LimparCarrinho());

            CarregarVendas();
            CarregarProdutos();
            CarregarClientes();
            
            _isInitialized = true;
        }

        /// <summary>
        /// Carrega todas as vendas
        /// </summary>
        private void CarregarVendas()
        {
            try
            {
                var vendas = _vendaRepository.ObterTodas();
                Vendas.Clear();
                foreach (var venda in vendas)
                {
                    Vendas.Add(venda);
                }
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                // Em caso de erro, mantém lista vazia
                // Não exibe mensagem para não interromper a experiência
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar vendas: {ex.Message}");
            }
        }

        /// <summary>
        /// Inicia uma nova venda
        /// </summary>
        private void NovaVenda()
        {
            ClienteSelecionado = null;
            ItensCarrinho.Clear();
            ProdutoSelecionado = null;
            QuantidadeAdicionar = 1;
            FormaPagamento = Constants.PagamentoDinheiro;
            DescontoVenda = 0;
            Vendedor = string.Empty;
            ObservacoesVenda = string.Empty;
        }

        /// <summary>
        /// Adiciona item ao carrinho
        /// </summary>
        private void AdicionarItemCarrinho()
        {
            if (ProdutoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (QuantidadeAdicionar <= 0)
            {
                MessageBox.Show("Quantidade deve ser maior que zero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProdutoSelecionado.Estoque < QuantidadeAdicionar)
            {
                MessageBox.Show($"Estoque insuficiente. Disponível: {ProdutoSelecionado.Estoque}", 
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ProdutoSelecionado.Ativo)
            {
                MessageBox.Show("Produto está inativo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica se produto já está no carrinho
            var itemExistente = ItensCarrinho.FirstOrDefault(i => i.ProdutoId == ProdutoSelecionado.Id);
            if (itemExistente != null)
            {
                int novaQuantidade = itemExistente.Quantidade + QuantidadeAdicionar;
                if (ProdutoSelecionado.Estoque < novaQuantidade)
                {
                    MessageBox.Show($"Estoque insuficiente para a quantidade total. Disponível: {ProdutoSelecionado.Estoque}", 
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                itemExistente.Quantidade = novaQuantidade;            
            }
            else
            {
                var novoItem = new ItemVenda
            {
                ProdutoId = ProdutoSelecionado.Id,
                Produto = ProdutoSelecionado,
                Quantidade = QuantidadeAdicionar,
                PrecoUnitario = ProdutoSelecionado.Preco,
                Desconto = 0,                
            };

                ItensCarrinho.Add(novoItem);
            }

            CalcularTotalVenda();
            QuantidadeAdicionar = 1;
            ProdutoSelecionado = null;
        }

        /// <summary>
        /// Remove item do carrinho
        /// </summary>
        private void RemoverItemCarrinho(object? parameter)
        {
            if (parameter is ItemVenda item)
            {
                ItensCarrinho.Remove(item);
                CalcularTotalVenda();
            }
        }

        /// <summary>
        /// Confirma e registra a venda
        /// </summary>
        private void ConfirmarVenda()
        {
            if (ItensCarrinho.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos um item ao carrinho.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FormaPagamento == Constants.PagamentoCredito)
            {
                if (ClienteSelecionado == null)
                {
                    MessageBox.Show("Cliente é obrigatório para venda fiada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!PodeVenderFiado)
                {
                    MessageBox.Show("Cliente não possui limite de crédito suficiente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                var venda = new Venda
                {
                    ClienteId = ClienteSelecionado?.Id ?? 0,
                    Cliente = ClienteSelecionado,
                    Itens = ItensCarrinho.ToList(),
                    ValorSubtotal = SubtotalVenda,
                    Desconto = DescontoVenda,
                    ValorTotal = TotalVenda,
                    FormaPagamento = FormaPagamento,
                    Vendedor = Vendedor,
                    Observacoes = ObservacoesVenda
                };

                if (FormaPagamento == Constants.PagamentoCredito)
                {
                    _vendaRepository.RegistrarVendaFiada(venda);
                }
                else
                {
                    _vendaRepository.RegistrarVendaAVista(venda);
                }

                _dashboard.AtualizarDashboard();

                MessageBox.Show("Venda registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                
                NovaVenda();
                CarregarVendas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar venda: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cancela uma venda
        /// </summary>
        private void CancelarVenda(object? parameter)
        {
            if (VendaSelecionada == null) return;

            if (VendaSelecionada.Status == "Cancelada")
            {
                MessageBox.Show("Esta venda já está cancelada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"Deseja realmente cancelar a venda {VendaSelecionada.NumeroVenda}?",
                "Confirmar Cancelamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _vendaRepository.CancelarVenda(VendaSelecionada.Id, "Cancelamento solicitado pelo usuário");
                    _dashboard.AtualizarDashboard();
                    MessageBox.Show("Venda cancelada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    CarregarVendas();
                    VendaSelecionada = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao cancelar venda: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Exibe detalhes da venda selecionada
        /// </summary>
        private void VerDetalhesVenda()
        {
            if (VendaSelecionada == null) return;
            // A implementação da tela de detalhes será feita na View
        }

        /// <summary>
        /// Limpa o carrinho
        /// </summary>
        private void LimparCarrinho()
        {
            ItensCarrinho.Clear();
            CalcularTotalVenda();
        }

        /// <summary>
        /// Calcula o total da venda
        /// </summary>
        private void CalcularTotalVenda()
        {
            OnPropertyChanged(nameof(SubtotalVenda));
            OnPropertyChanged(nameof(TotalVenda));
            OnPropertyChanged(nameof(SubtotalVendaTexto));
            OnPropertyChanged(nameof(TotalVendaTexto));
            OnPropertyChanged(nameof(PodeVenderFiado));
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Carrega produtos disponíveis
        /// </summary>
        private void CarregarProdutos()
        {
            try
            {
                var produtos = _produtoRepository.ObterTodos().Where(p => p.Ativo).ToList();
                ProdutosDisponiveis.Clear();
                foreach (var produto in produtos)
                {
                    ProdutosDisponiveis.Add(produto);
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, mantém lista vazia
                // Não exibe mensagem para não interromper a experiência
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar produtos: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega clientes disponíveis
        /// </summary>
        private void CarregarClientes()
        {
            try
            {
                var clientes = _clienteRepository.ObterTodos().Where(c => c.Ativo).ToList();
                ClientesDisponiveis.Clear();
                foreach (var cliente in clientes)
                {
                    ClientesDisponiveis.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, mantém lista vazia
                // Não exibe mensagem para não interromper a experiência
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtra produtos na busca
        /// </summary>
        private void FiltrarProdutos()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BuscaProduto))
                {
                    CarregarProdutos();
                    return;
                }

                var produtos = _produtoRepository.ObterTodos()
                    .Where(p => p.Ativo && 
                        (p.Nome.ToLower().Contains(BuscaProduto.ToLower()) ||
                         p.Codigo.ToLower().Contains(BuscaProduto.ToLower()) ||
                         (!string.IsNullOrWhiteSpace(p.Categoria) && p.Categoria.ToLower().Contains(BuscaProduto.ToLower()))))
                    .ToList();

                ProdutosDisponiveis.Clear();
                foreach (var produto in produtos)
                {
                    ProdutosDisponiveis.Add(produto);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao filtrar produtos: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtra clientes na busca
        /// </summary>
        private void FiltrarClientes()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BuscaCliente))
                {
                    CarregarClientes();
                    return;
                }

                var termoLower = BuscaCliente.ToLower();
                var todosClientes = _clienteRepository.ObterTodos().Where(c => c.Ativo).ToList();
                
                var clientesFiltrados = todosClientes.Where(c =>
                    c.Nome.ToLower().Contains(termoLower) ||
                    c.Codigo.ToLower().Contains(termoLower) ||
                    c.Documento.Contains(BuscaCliente) ||
                    (!string.IsNullOrWhiteSpace(c.Telefone) && c.Telefone.Contains(BuscaCliente)) ||
                    (!string.IsNullOrWhiteSpace(c.Celular) && c.Celular.Contains(BuscaCliente))
                ).ToList();

                ClientesDisponiveis.Clear();
                foreach (var cliente in clientesFiltrados.OrderBy(c => c.Nome))
                {
                    ClientesDisponiveis.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao filtrar clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// Aplica filtros na listagem de vendas
        /// </summary>
        private void AplicarFiltros()
        {
            try
            {
                if (Vendas == null)
                    return;

                var todasVendas = _vendaRepository.ObterTodas();

                var filtradas = todasVendas.AsQueryable();

            // Filtro por período
            filtradas = filtradas.Where(v => v.DataVenda >= FiltroDataInicio && v.DataVenda <= FiltroDataFim.AddDays(1));

            // Filtro por cliente
            if (!string.IsNullOrWhiteSpace(FiltroCliente))
            {
                var filtroLower = FiltroCliente.ToLower();
                filtradas = filtradas.Where(v => 
                    v.Cliente != null && 
                    (v.Cliente.Nome.ToLower().Contains(filtroLower) ||
                     v.Cliente.Codigo.ToLower().Contains(filtroLower)));
            }

            // Filtro por forma de pagamento
            if (FiltroFormaPagamento != "Todas")
            {
                filtradas = filtradas.Where(v => v.FormaPagamento == FiltroFormaPagamento);
            }

            // Filtro por status
            if (FiltroStatus != "Todas")
            {
                filtradas = filtradas.Where(v => v.Status == FiltroStatus);
            }

                Vendas.Clear();
                foreach (var venda in filtradas.OrderByDescending(v => v.DataVenda))
                {
                    Vendas.Add(venda);
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, mantém lista vazia
                // Não exibe mensagem para não interromper a experiência
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar filtros de vendas: {ex.Message}");
            }
        }
    }
}
