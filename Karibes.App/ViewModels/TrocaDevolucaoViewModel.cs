using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;

namespace Karibes.App.ViewModels
{
    /// <summary>
    /// ViewModel para gerenciamento de trocas e devoluções
    /// </summary>
    public class TrocaDevolucaoViewModel : BaseViewModel
    {
        private readonly VendaService _vendaService;
        private readonly ProdutoService _produtoService;
        private readonly ClienteService _clienteService;

        // Busca de venda
        private string _numeroVendaBusca = string.Empty;
        private Venda? _vendaSelecionada;

        // Itens para devolução
        private ObservableCollection<ItemDevolucaoViewModel> _itensDevolucao;
        private decimal _valorTotalDevolucao;

        // Itens para troca (novos)
        private ObservableCollection<ItemVenda> _itensNovos;
        private Produto? _produtoSelecionado;
        private int _quantidadeAdicionar = 1;
        private decimal _valorTotalNovos;

        // Resumo
        private decimal _diferencaValor;
        private string _observacao = string.Empty;
        private bool _isModoDevolucao = true; // true = Devolução, false = Troca

        // Produtos disponíveis
        private ObservableCollection<Produto> _produtosDisponiveis;
        private string _buscaProduto = string.Empty;

        public string NumeroVendaBusca
        {
            get => _numeroVendaBusca;
            set => SetProperty(ref _numeroVendaBusca, value);
        }

        public Venda? VendaSelecionada
        {
            get => _vendaSelecionada;
            set
            {
                SetProperty(ref _vendaSelecionada, value);
                if (value != null)
                {
                    CarregarItensDevolucao();
                }
            }
        }

        public ObservableCollection<ItemDevolucaoViewModel> ItensDevolucao
        {
            get => _itensDevolucao;
            set => SetProperty(ref _itensDevolucao, value);
        }

        public decimal ValorTotalDevolucao
        {
            get => _valorTotalDevolucao;
            set => SetProperty(ref _valorTotalDevolucao, value);
        }

        public ObservableCollection<ItemVenda> ItensNovos
        {
            get => _itensNovos;
            set => SetProperty(ref _itensNovos, value);
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

        public decimal ValorTotalNovos
        {
            get => _valorTotalNovos;
            set => SetProperty(ref _valorTotalNovos, value);
        }

        public decimal DiferencaValor
        {
            get => _diferencaValor;
            set => SetProperty(ref _diferencaValor, value);
        }

        public string Observacao
        {
            get => _observacao;
            set => SetProperty(ref _observacao, value);
        }

        public bool IsModoDevolucao
        {
            get => _isModoDevolucao;
            set
            {
                SetProperty(ref _isModoDevolucao, value);
                if (value)
                {
                    ItensNovos.Clear();
                    CalcularDiferenca();
                }
            }
        }

        public ObservableCollection<Produto> ProdutosDisponiveis
        {
            get => _produtosDisponiveis;
            set => SetProperty(ref _produtosDisponiveis, value);
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

        // Commands
        public RelayCommand BuscarVendaCommand { get; }
        public RelayCommand AdicionarItemNovoCommand { get; }
        public RelayCommand RemoverItemNovoCommand { get; }
        public RelayCommand ConfirmarDevolucaoCommand { get; }
        public RelayCommand ConfirmarTrocaCommand { get; }
        public RelayCommand LimparCommand { get; }

        public TrocaDevolucaoViewModel()
        {
            _vendaService = new VendaService();
            _produtoService = new ProdutoService();
            _clienteService = new ClienteService();

            ItensDevolucao = new ObservableCollection<ItemDevolucaoViewModel>();
            ItensNovos = new ObservableCollection<ItemVenda>();
            ProdutosDisponiveis = new ObservableCollection<Produto>();

            BuscarVendaCommand = new RelayCommand(_ => BuscarVenda());
            AdicionarItemNovoCommand = new RelayCommand(_ => AdicionarItemNovo());
            RemoverItemNovoCommand = new RelayCommand(RemoverItemNovo);
            ConfirmarDevolucaoCommand = new RelayCommand(_ => ConfirmarDevolucao(), _ => VendaSelecionada != null && ItensDevolucao.Any(i => i.QuantidadeDevolver > 0));
            ConfirmarTrocaCommand = new RelayCommand(_ => ConfirmarTroca(), _ => VendaSelecionada != null && ItensDevolucao.Any(i => i.QuantidadeDevolver > 0) && ItensNovos.Count > 0);
            LimparCommand = new RelayCommand(_ => Limpar());

            CarregarProdutos();
        }

        /// <summary>
        /// Busca uma venda pelo número
        /// </summary>
        private void BuscarVenda()
        {
            if (string.IsNullOrWhiteSpace(NumeroVendaBusca))
            {
                MessageBox.Show("Informe o número da venda.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var venda = _vendaService.ObterPorNumeroVenda(NumeroVendaBusca.Trim());
                if (venda == null)
                {
                    MessageBox.Show("Venda não encontrada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Carregar cliente se necessário
                if (venda.ClienteId > 0 && venda.Cliente == null)
                {
                    venda.Cliente = _clienteService.ObterPorId(venda.ClienteId);
                }

                VendaSelecionada = venda;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar venda: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carrega os itens da venda para devolução
        /// </summary>
        private void CarregarItensDevolucao()
        {
            if (VendaSelecionada == null) return;

            ItensDevolucao.Clear();
            foreach (var item in VendaSelecionada.Itens)
            {
                if (item.Produto == null)
                {
                    item.Produto = _produtoService.ObterPorId(item.ProdutoId);
                }

                var itemDevolucao = new ItemDevolucaoViewModel
                {
                    ItemVenda = item,
                    QuantidadeOriginal = item.Quantidade,
                    QuantidadeDevolver = 0,
                    ViewModelPrincipal = this
                };
                ItensDevolucao.Add(itemDevolucao);
            }
        }

        /// <summary>
        /// Adiciona um novo item para troca
        /// </summary>
        private void AdicionarItemNovo()
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

            // Verifica se produto já está na lista
            var itemExistente = ItensNovos.FirstOrDefault(i => i.ProdutoId == ProdutoSelecionado.Id);
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
                ItensNovos.Add(novoItem);
            }

            CalcularDiferenca();
            QuantidadeAdicionar = 1;
            ProdutoSelecionado = null;
        }

        /// <summary>
        /// Remove um item novo da lista
        /// </summary>
        private void RemoverItemNovo(object? parameter)
        {
            if (parameter is ItemVenda item)
            {
                ItensNovos.Remove(item);
                CalcularDiferenca();
            }
        }

        /// <summary>
        /// Calcula os valores e diferença
        /// </summary>
        public void CalcularDiferenca()
        {
            ValorTotalDevolucao = ItensDevolucao
                .Where(i => i.QuantidadeDevolver > 0)
                .Sum(i => (i.ItemVenda.ValorTotal / i.ItemVenda.Quantidade) * i.QuantidadeDevolver);

            ValorTotalNovos = ItensNovos.Sum(i => i.ValorTotal);

            DiferencaValor = ValorTotalNovos - ValorTotalDevolucao;

            OnPropertyChanged(nameof(ValorTotalDevolucao));
            OnPropertyChanged(nameof(ValorTotalNovos));
        }

        /// <summary>
        /// Confirma a devolução
        /// </summary>
        private void ConfirmarDevolucao()
        {
            if (VendaSelecionada == null) return;

            var itensDevolver = ItensDevolucao
                .Where(i => i.QuantidadeDevolver > 0)
                .ToDictionary(i => i.ItemVenda.Id, i => i.QuantidadeDevolver);

            if (itensDevolver.Count == 0)
            {
                MessageBox.Show("Selecione pelo menos um item para devolver.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"Deseja confirmar a devolução de {itensDevolver.Count} item(ns) no valor de {ValorTotalDevolucao:C}?",
                "Confirmar Devolução",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _vendaService.DevolverVenda(VendaSelecionada.Id, itensDevolver, Observacao);
                    MessageBox.Show("Devolução realizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Limpar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao realizar devolução: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Confirma a troca
        /// </summary>
        private void ConfirmarTroca()
        {
            if (VendaSelecionada == null) return;

            var itensDevolver = ItensDevolucao
                .Where(i => i.QuantidadeDevolver > 0)
                .ToDictionary(i => i.ItemVenda.Id, i => i.QuantidadeDevolver);

            if (itensDevolver.Count == 0)
            {
                MessageBox.Show("Selecione pelo menos um item para devolver.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ItensNovos.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos um item novo para troca.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string mensagem = $"Deseja confirmar a troca?\n\n" +
                             $"Itens devolvidos: {itensDevolver.Count} item(ns) - {ValorTotalDevolucao:C}\n" +
                             $"Itens novos: {ItensNovos.Count} item(ns) - {ValorTotalNovos:C}\n" +
                             $"Diferença: {DiferencaValor:C}";

            var resultado = MessageBox.Show(mensagem, "Confirmar Troca", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _vendaService.TrocarVenda(VendaSelecionada.Id, itensDevolver, ItensNovos.ToList(), Observacao);
                    MessageBox.Show("Troca realizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Limpar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao realizar troca: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Limpa todos os dados
        /// </summary>
        private void Limpar()
        {
            NumeroVendaBusca = string.Empty;
            VendaSelecionada = null;
            ItensDevolucao.Clear();
            ItensNovos.Clear();
            ProdutoSelecionado = null;
            QuantidadeAdicionar = 1;
            Observacao = string.Empty;
            ValorTotalDevolucao = 0;
            ValorTotalNovos = 0;
            DiferencaValor = 0;
        }

        /// <summary>
        /// Carrega produtos disponíveis
        /// </summary>
        private void CarregarProdutos()
        {
            try
            {
                var produtos = _produtoService.ObterTodos().Where(p => p.Ativo).ToList();
                ProdutosDisponiveis.Clear();
                foreach (var produto in produtos)
                {
                    ProdutosDisponiveis.Add(produto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Filtra produtos na busca
        /// </summary>
        private void FiltrarProdutos()
        {
            if (string.IsNullOrWhiteSpace(BuscaProduto))
            {
                CarregarProdutos();
                return;
            }

            var produtos = _produtoService.ObterTodos()
                .Where(p => p.Ativo && 
                    (p.Nome.ToLower().Contains(BuscaProduto.ToLower()) ||
                     p.Codigo.ToLower().Contains(BuscaProduto.ToLower())))
                .ToList();

            ProdutosDisponiveis.Clear();
            foreach (var produto in produtos)
            {
                ProdutosDisponiveis.Add(produto);
            }
        }
    }

    /// <summary>
    /// ViewModel auxiliar para itens de devolução
    /// </summary>
    public class ItemDevolucaoViewModel : BaseViewModel
    {
        private ItemVenda _itemVenda;
        private int _quantidadeOriginal;
        private int _quantidadeDevolver;
        private TrocaDevolucaoViewModel? _viewModelPrincipal;

        public ItemVenda ItemVenda
        {
            get => _itemVenda;
            set => SetProperty(ref _itemVenda, value);
        }

        public int QuantidadeOriginal
        {
            get => _quantidadeOriginal;
            set => SetProperty(ref _quantidadeOriginal, value);
        }

        public int QuantidadeDevolver
        {
            get => _quantidadeDevolver;
            set
            {
                if (value < 0) value = 0;
                if (value > QuantidadeOriginal) value = QuantidadeOriginal;
                if (SetProperty(ref _quantidadeDevolver, value))
                {
                    OnPropertyChanged(nameof(ValorDevolver));
                    _viewModelPrincipal?.CalcularDiferenca();
                }
            }
        }

        public TrocaDevolucaoViewModel? ViewModelPrincipal
        {
            get => _viewModelPrincipal;
            set => SetProperty(ref _viewModelPrincipal, value);
        }

        public decimal ValorDevolver => QuantidadeDevolver > 0 
            ? (ItemVenda.ValorTotal / QuantidadeOriginal) * QuantidadeDevolver 
            : 0;
    }
}

