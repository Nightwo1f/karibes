using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Karibes.App.Models;
using Karibes.App.Services;
using Karibes.App.Utils;

namespace Karibes.App.ViewModels
{
    public class ClientesViewModel : BaseViewModel
    {
        private readonly DashboardViewModel _dashboard;
        private readonly ClienteService _clienteService;
        private readonly VendaService _vendaService;
        private readonly CreditoService _creditoService;
        private ObservableCollection<Cliente> _clientes = new();
        private ObservableCollection<Cliente> _clientesFiltrados = new();
        private Cliente? _clienteSelecionado;
        private Cliente? _clienteEditando;
        private string _textoBusca = string.Empty;
        private string _statusSelecionado = "Todos";
        private bool _isModalAberto = false;
        
        // Histórico de compras e pagamentos
        private ObservableCollection<Venda> _comprasCliente = new();
        private ObservableCollection<HistoricoCredito> _historicoPagamentos = new();
        private decimal _valorPagamento = 0;
        private DateTime _dataPagamento = DateTime.Now;
        private string _observacaoPagamento = string.Empty;

        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }

        public ObservableCollection<Cliente> ClientesFiltrados
        {
            get => _clientesFiltrados;
            set => SetProperty(ref _clientesFiltrados, value);
        }

        public Cliente? ClienteSelecionado
        {
            get => _clienteSelecionado;
            set => SetProperty(ref _clienteSelecionado, value);
        }

        public Cliente? ClienteEditando
        {
            get => _clienteEditando;
            set => SetProperty(ref _clienteEditando, value);
        }

        public string TextoBusca
        {
            get => _textoBusca;
            set
            {
                SetProperty(ref _textoBusca, value);
                AplicarFiltros();
            }
        }

        public string StatusSelecionado
        {
            get => _statusSelecionado;
            set
            {
                SetProperty(ref _statusSelecionado, value);
                AplicarFiltros();
            }
        }

        public bool IsModalAberto
        {
            get => _isModalAberto;
            set => SetProperty(ref _isModalAberto, value);
        }

        public ObservableCollection<Venda> ComprasCliente
        {
            get => _comprasCliente;
            set => SetProperty(ref _comprasCliente, value);
        }

        public ObservableCollection<HistoricoCredito> HistoricoPagamentos
        {
            get => _historicoPagamentos;
            set => SetProperty(ref _historicoPagamentos, value);
        }

        public decimal ValorPagamento
        {
            get => _valorPagamento;
            set => SetProperty(ref _valorPagamento, value);
        }

        public DateTime DataPagamento
        {
            get => _dataPagamento;
            set => SetProperty(ref _dataPagamento, value);
        }

        public string ObservacaoPagamento
        {
            get => _observacaoPagamento;
            set => SetProperty(ref _observacaoPagamento, value);
        }

        // Commands
        public RelayCommand NovoClienteCommand { get; }
        public RelayCommand EditarClienteCommand { get; }
        public RelayCommand AtualizarCommand { get; }
        public RelayCommand SalvarClienteCommand { get; }
        public RelayCommand CancelarEdicaoCommand { get; }
        public RelayCommand RegistrarPagamentoCommand { get; }

        public ClientesViewModel(DashboardViewModel dashboard)
        {
            _dashboard = dashboard;
            _clienteService = new ClienteService();
            _vendaService = new VendaService();
            _creditoService = new CreditoService();
            Clientes = new ObservableCollection<Cliente>();
            ClientesFiltrados = new ObservableCollection<Cliente>();
            ComprasCliente = new ObservableCollection<Venda>();
            HistoricoPagamentos = new ObservableCollection<HistoricoCredito>();
            
            NovoClienteCommand = new RelayCommand(_ => NovoCliente());
            EditarClienteCommand = new RelayCommand(EditarCliente, _ => ClienteSelecionado != null);
            AtualizarCommand = new RelayCommand(_ => CarregarClientes());
            SalvarClienteCommand = new RelayCommand(_ => SalvarCliente(), _ => ClienteEditando != null);
            CancelarEdicaoCommand = new RelayCommand(_ => CancelarEdicao());
            RegistrarPagamentoCommand = new RelayCommand(_ => RegistrarPagamento(), _ => ClienteEditando != null && ClienteEditando.SaldoDevedor > 0);
            
            // Carrega clientes ao inicializar
            CarregarClientes();
        }

        /// <summary>
        /// Carrega todos os clientes
        /// </summary>
        private void CarregarClientes()
        {
            try
            {
                var clientes = _clienteService.ObterTodos();
                Clientes.Clear();
                foreach (var cliente in clientes.OrderBy(c => c.Nome))
                {
                    Clientes.Add(cliente);
                }
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar clientes: {ex.Message}", 
                    "Erro", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Aplica filtros na listagem
        /// </summary>
        private void AplicarFiltros()
        {
            try
            {
                if (Clientes == null)
                    return;

                var filtrados = Clientes.AsQueryable();

                // Filtro por texto (Nome, CPF/CNPJ, Telefone, ID)
                if (!string.IsNullOrWhiteSpace(TextoBusca))
                {
                    var termoLower = TextoBusca.ToLower();
                    filtrados = filtrados.Where(c =>
                        c.Nome.ToLower().Contains(termoLower) ||
                        c.Documento.Contains(TextoBusca) ||
                        (!string.IsNullOrWhiteSpace(c.Telefone) && c.Telefone.Contains(TextoBusca)) ||
                        (!string.IsNullOrWhiteSpace(c.Celular) && c.Celular.Contains(TextoBusca)) ||
                        c.Codigo.ToLower().Contains(termoLower) ||
                        c.Id.ToString().Contains(TextoBusca)
                    );
                }

                // Filtro por status
                if (StatusSelecionado == "Ativo")
                {
                    filtrados = filtrados.Where(c => c.Ativo);
                }
                else if (StatusSelecionado == "Inativo")
                {
                    filtrados = filtrados.Where(c => !c.Ativo);
                }

                ClientesFiltrados.Clear();
                foreach (var cliente in filtrados.OrderBy(c => c.Nome))
                {
                    ClientesFiltrados.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar filtros: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um novo cliente
        /// </summary>
        private void NovoCliente()
        {
            ClienteEditando = new Cliente
            {
                Codigo = string.Empty,
                Nome = string.Empty,
                TipoDocumento = "CPF",
                Documento = string.Empty,
                Email = string.Empty,
                Telefone = string.Empty,
                Celular = string.Empty,
                CEP = string.Empty,
                Endereco = string.Empty,
                Numero = string.Empty,
                Complemento = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                LimiteCredito = 0,
                SaldoDevedor = 0,
                TotalPago = 0,
                DataCadastro = DateTime.Now,
                DataUltimaAtualizacao = DateTime.Now,
                Ativo = true,
                Observacoes = string.Empty,
                Pagamentos = new List<PagamentoCliente>()
            };
            IsModalAberto = true;
        }

        /// <summary>
        /// Edita um cliente existente
        /// </summary>
        private void EditarCliente(object? parameter)
        {
            var cliente = parameter as Cliente ?? ClienteSelecionado;
            if (cliente == null) return;

            ClienteEditando = new Cliente
            {
                Id = cliente.Id,
                Codigo = cliente.Codigo,
                Nome = cliente.Nome,
                TipoDocumento = cliente.TipoDocumento,
                Documento = cliente.Documento,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                Celular = cliente.Celular,
                CEP = cliente.CEP,
                Endereco = cliente.Endereco,
                Numero = cliente.Numero,
                Complemento = cliente.Complemento,
                Bairro = cliente.Bairro,
                Cidade = cliente.Cidade,
                Estado = cliente.Estado,
                LimiteCredito = cliente.LimiteCredito,
                SaldoDevedor = cliente.SaldoDevedor,
                TotalPago = cliente.TotalPago,
                DataCadastro = cliente.DataCadastro,
                DataUltimaAtualizacao = cliente.DataUltimaAtualizacao,
                Ativo = cliente.Ativo,
                Observacoes = cliente.Observacoes,
                Pagamentos = cliente.Pagamentos ?? new List<PagamentoCliente>()
            };
            
            // Carrega histórico de compras e pagamentos
            CarregarHistoricoCliente(cliente.Id);
            
            IsModalAberto = true;
        }

        /// <summary>
        /// Carrega histórico de compras e pagamentos do cliente
        /// </summary>
        private void CarregarHistoricoCliente(int clienteId)
        {
            try
            {
                // Carrega compras
                var compras = _vendaService.ObterVendasPorCliente(clienteId);
                ComprasCliente.Clear();
                foreach (var compra in compras)
                {
                    ComprasCliente.Add(compra);
                }

                // Carrega histórico de crédito (pagamentos)
                var historico = _creditoService.ObterHistoricoPorCliente(clienteId);
                HistoricoPagamentos.Clear();
                foreach (var item in historico.Where(h => h.TipoMovimento == "Pagamento").OrderByDescending(h => h.DataMovimento))
                {
                    HistoricoPagamentos.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar histórico do cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra um pagamento do cliente
        /// </summary>
        private void RegistrarPagamento()
        {
            if (ClienteEditando == null) return;

            try
            {
                // Validações
                if (ValorPagamento <= 0)
                {
                    MessageBox.Show("O valor do pagamento deve ser maior que zero.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ValorPagamento > ClienteEditando.SaldoDevedor)
                {
                    MessageBox.Show($"O valor do pagamento (R$ {ValorPagamento:N2}) não pode ser maior que o saldo devedor (R$ {ClienteEditando.SaldoDevedor:N2}).", 
                        "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Registra pagamento via CreditoService
                _creditoService.RegistrarPagamento(ClienteEditando, ValorPagamento, ObservacaoPagamento);

                // Atualiza TotalPago
                ClienteEditando.TotalPago += ValorPagamento;

                // Salva cliente atualizado
                _clienteService.Salvar(ClienteEditando);

                // Limpa campos
                ValorPagamento = 0;
                ObservacaoPagamento = string.Empty;
                DataPagamento = DateTime.Now;

                // Recarrega histórico
                CarregarHistoricoCliente(ClienteEditando.Id);

                // Recarrega lista de clientes
                CarregarClientes();

                // Atualiza ClienteEditando com dados atualizados
                var clienteAtualizado = _clienteService.ObterPorId(ClienteEditando.Id);
                if (clienteAtualizado != null)
                {
                    ClienteEditando.SaldoDevedor = clienteAtualizado.SaldoDevedor;
                    ClienteEditando.TotalPago = clienteAtualizado.TotalPago;
                    OnPropertyChanged(nameof(ClienteEditando));
                }

                _dashboard.AtualizarDashboard();

                MessageBox.Show("Pagamento registrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar pagamento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Salva um cliente (novo ou editado)
        /// </summary>
        private void SalvarCliente()
        {
            if (ClienteEditando == null) return;

            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(ClienteEditando.Nome))
                {
                    MessageBox.Show("O nome é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Salva no ClienteService
                _clienteService.Salvar(ClienteEditando);
                
                // Recarrega a lista para garantir sincronização
                CarregarClientes();

                _dashboard.AtualizarDashboard();
                
                CancelarEdicao();
                MessageBox.Show("Cliente salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar cliente: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cancela a edição
        /// </summary>
        private void CancelarEdicao()
        {
            ClienteEditando = null;
            ClienteSelecionado = null;
            IsModalAberto = false;
        }
    }
}

