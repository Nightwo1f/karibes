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
    /// ViewModel para gerenciamento de produtos
    /// </summary>
    public class ProdutosViewModel : BaseViewModel
    {
        private readonly ProdutoService _produtoService;
        private ObservableCollection<Produto> _produtos = new();
        private Produto? _produtoSelecionado;
        private Produto? _produtoEditando;
        private string _filtroTexto = string.Empty;
        private bool _mostrarApenasAtivos = true;
        private readonly DashboardViewModel _dashboard;

        public ObservableCollection<Produto> Produtos
        {
            get => _produtos;
            set => SetProperty(ref _produtos, value);
        }

        public Produto? ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set
            {
                SetProperty(ref _produtoSelecionado, value);
                if (value != null)
                {
                    ProdutoEditando = new Produto
                    {
                        Id = value.Id,
                        Codigo = value.Codigo,
                        Nome = value.Nome,
                        Descricao = value.Descricao,
                        Preco = value.Preco,
                        Custo = value.Custo,
                        Estoque = value.Estoque,
                        EstoqueMinimo = value.EstoqueMinimo,
                        Unidade = value.Unidade,
                        DataCadastro = value.DataCadastro,
                        DataUltimaAtualizacao = value.DataUltimaAtualizacao,
                        Ativo = value.Ativo
                    };
                }
                else
                {
                    ProdutoEditando = null;
                }
            }
        }

        public Produto? ProdutoEditando
        {
            get => _produtoEditando;
            set => SetProperty(ref _produtoEditando, value);
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set
            {
                SetProperty(ref _filtroTexto, value);
                AplicarFiltros();
            }
        }

        public bool MostrarApenasAtivos
        {
            get => _mostrarApenasAtivos;
            set
            {
                SetProperty(ref _mostrarApenasAtivos, value);
                AplicarFiltros();
            }
        }

        // Commands
        public RelayCommand CarregarProdutosCommand { get; }
        public RelayCommand NovoProdutoCommand { get; }
        public RelayCommand SalvarProdutoCommand { get; }
        public RelayCommand ExcluirProdutoCommand { get; }
        public RelayCommand CancelarEdicaoCommand { get; }

        public ProdutosViewModel(DashboardViewModel dashboard)
        {
            _dashboard = dashboard;
            _produtoService = new ProdutoService();
            Produtos = new ObservableCollection<Produto>();

            CarregarProdutosCommand = new RelayCommand(_ => CarregarProdutos());
            NovoProdutoCommand = new RelayCommand(_ => NovoProduto());
            SalvarProdutoCommand = new RelayCommand(_ => SalvarProduto(), _ => ProdutoEditando != null);
            ExcluirProdutoCommand = new RelayCommand(_ => ExcluirProduto(), _ => ProdutoSelecionado != null && ProdutoSelecionado.Ativo);
            CancelarEdicaoCommand = new RelayCommand(_ => CancelarEdicao());

            CarregarProdutos();
        }

        /// <summary>
        /// Carrega todos os produtos
        /// </summary>
        private void CarregarProdutos()
        {
            try
            {
                var produtos = _produtoService.ObterTodos();
                Produtos.Clear();
                foreach (var produto in produtos)
                {
                    Produtos.Add(produto);
                }
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cria um novo produto para edição
        /// </summary>
        private void NovoProduto()
        {
            ProdutoSelecionado = null;
            ProdutoEditando = new Produto
            {
                Codigo = string.Empty,
                Nome = string.Empty,
                Descricao = string.Empty,
                Preco = 0,
                Custo = 0,
                Estoque = 0,
                EstoqueMinimo = 0,
                Unidade = "UN",
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataUltimaAtualizacao = DateTime.Now
            };
        }

        /// <summary>
        /// Salva o produto (cria ou atualiza)
        /// </summary>
        private void SalvarProduto()
        {
            if (ProdutoEditando == null)
                return;

            try
            {
                // Validações
                if (string.IsNullOrWhiteSpace(ProdutoEditando.Nome))
                {
                    MessageBox.Show("Nome do produto é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProdutoEditando.Codigo))
                {
                    MessageBox.Show("Código do produto é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ProdutoEditando.Preco < 0)
                {
                    MessageBox.Show("Preço não pode ser negativo.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ProdutoEditando.Custo < 0)
                {
                    MessageBox.Show("Custo não pode ser negativo.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ProdutoEditando.Estoque < 0)
                {
                    MessageBox.Show("Estoque não pode ser negativo.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ProdutoEditando.EstoqueMinimo < 0)
                {
                    MessageBox.Show("Estoque mínimo não pode ser negativo.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ProdutoEditando.Id == 0)
                {
                    // Novo produto
                    _produtoService.Criar(ProdutoEditando);
                    MessageBox.Show("Produto criado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Atualizar produto existente
                    _produtoService.Atualizar(ProdutoEditando);
                    MessageBox.Show("Produto atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CarregarProdutos();
                ProdutoSelecionado = Produtos.FirstOrDefault(p => p.Id == ProdutoEditando.Id);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exclui um produto (soft delete)
        /// </summary>
        private void ExcluirProduto()
        {
            if (ProdutoSelecionado == null)
                return;

            var resultado = MessageBox.Show(
                $"Deseja realmente excluir o produto '{ProdutoSelecionado.Nome}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _produtoService.Excluir(ProdutoSelecionado.Id);
                    MessageBox.Show("Produto excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    CarregarProdutos();
                    ProdutoSelecionado = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Cancela a edição atual
        /// </summary>
        private void CancelarEdicao()
        {
            ProdutoSelecionado = null;
            ProdutoEditando = null;
        }

        /// <summary>
        /// Aplica filtros na lista de produtos
        /// </summary>
        private void AplicarFiltros()
        {
            // Esta implementação é simplificada - em produção, você pode usar CollectionViewSource
            // Por enquanto, vamos recarregar e filtrar
            var todosProdutos = _produtoService.ObterTodos();

            var filtrados = todosProdutos.AsQueryable();

            if (MostrarApenasAtivos)
            {
                filtrados = filtrados.Where(p => p.Ativo);
            }

            if (!string.IsNullOrWhiteSpace(FiltroTexto))
            {
                var filtro = FiltroTexto.ToLower();
                filtrados = filtrados.Where(p =>
                    p.Nome.ToLower().Contains(filtro) ||
                    p.Codigo.ToLower().Contains(filtro) ||
                    (p.Descricao != null && p.Descricao.ToLower().Contains(filtro)));
            }

            Produtos.Clear();
            foreach (var produto in filtrados)
            {
                Produtos.Add(produto);
            }
        }
    }
}


