using OfficeOpenXml;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço responsável por registrar vendas e orquestrar operações relacionadas
    /// </summary>
    public class VendaService
    {
        private readonly ExcelService _excelService;
        private readonly CreditoService _creditoService;
        private readonly ProdutoService _produtoService;
        private readonly ClienteService _clienteService;
        private readonly FinanceiroService _financeiroService;
        private readonly CalculoFinanceiroService _calculoFinanceiro;
        private const string VendasSheetName = "Vendas";
        private const string ItensVendaSheetName = "ItensVenda";

        public VendaService()
        {
            _excelService = new ExcelService();
            _creditoService = new CreditoService();
            _produtoService = new ProdutoService();
            _clienteService = new ClienteService();
            _financeiroService = new FinanceiroService(_excelService);
            _calculoFinanceiro = new CalculoFinanceiroService();
            InicializarArquivo();
        }

        /// <summary>
        /// Registra uma venda à vista (Pix, Dinheiro ou Cartão)
        /// </summary>
        /// <param name="venda">Venda a ser registrada</param>
        public void RegistrarVendaAVista(Venda venda)
        {
            // Validações
            ValidarVenda(venda);

            // Validar forma de pagamento
            if (venda.FormaPagamento != Constants.PagamentoDinheiro &&
                venda.FormaPagamento != Constants.PagamentoCartao &&
                venda.FormaPagamento != Constants.PagamentoPix)
            {
                throw new ArgumentException("Forma de pagamento inválida para venda à vista.");
            }

            // Validar estoque antes de processar
            ValidarEstoqueDisponivel(venda.Itens);

            // Definir status como Finalizada
            venda.Status = "Finalizada";
            venda.DataVenda = DateTime.Now;

            // Gerar número único da venda
            if (string.IsNullOrWhiteSpace(venda.NumeroVenda))
            {
                venda.NumeroVenda = GerarNumeroVenda();
            }

            // Persistir venda
            int vendaId = SalvarVenda(venda);

            // Atualizar estoque dos produtos vendidos
            AtualizarEstoqueVenda(venda.Itens, vendaId);

            // Gerar lançamento financeiro do tipo Receita
            CriarLancamentoFinanceiroReceita(venda);
        }

        /// <summary>
        /// Registra uma venda fiada (crédito)
        /// </summary>
        /// <param name="venda">Venda a ser registrada</param>
        public void RegistrarVendaFiada(Venda venda)
        {
            // Validações
            ValidarVenda(venda);

            // Validar forma de pagamento
            if (venda.FormaPagamento != Constants.PagamentoCredito && venda.FormaPagamento != "Credito")
            {
                throw new ArgumentException("Forma de pagamento deve ser Crédito para venda fiada.");
            }

            // Validar Cliente não nulo
            if (venda.Cliente == null)
            {
                throw new ArgumentException("Cliente é obrigatório para venda fiada.");
            }

            // Validar limite usando CreditoService
            if (!_creditoService.PodeComprarNoCredito(venda.Cliente, venda.ValorTotal))
            {
                throw new InvalidOperationException(
                    $"Cliente não pode comprar no crédito. Limite insuficiente.");
            }

            // Validar estoque antes de processar
            ValidarEstoqueDisponivel(venda.Itens);

            // Definir status como Pendente (será finalizada após pagamento)
            venda.Status = "Pendente";
            venda.DataVenda = DateTime.Now;

            // Gerar número único da venda
            if (string.IsNullOrWhiteSpace(venda.NumeroVenda))
            {
                venda.NumeroVenda = GerarNumeroVenda();
            }

            // Persistir venda
            int vendaId = SalvarVenda(venda);

            // Atualizar estoque dos produtos vendidos
            AtualizarEstoqueVenda(venda.Itens, vendaId);

            // Delegar débito ao CreditoService
            _creditoService.RegistrarVendaFiada(venda.Cliente, venda);

            // NÃO gerar lançamento financeiro imediato (será gerado quando cliente pagar)
        }

        /// <summary>
        /// Valida os dados básicos da venda
        /// </summary>
        private void ValidarVenda(Venda venda)
        {
            if (venda == null)
                throw new ArgumentNullException(nameof(venda));

            if (venda.Itens == null || venda.Itens.Count == 0)
                throw new ArgumentException("Venda deve ter pelo menos um item.");

            if (venda.ValorTotal <= 0)
                throw new ArgumentException("Valor total da venda deve ser maior que zero.");

            if (venda.ValorSubtotal < venda.Desconto)
                throw new ArgumentException("Desconto não pode ser maior que o subtotal.");
        }

        /// <summary>
        /// Valida se há estoque disponível para todos os itens
        /// </summary>
        private void ValidarEstoqueDisponivel(List<ItemVenda> itens)
        {
            foreach (var item in itens)
            {
                var produto = _produtoService.ObterPorId(item.ProdutoId);
                if (produto == null)
                    throw new ArgumentException($"Produto com ID {item.ProdutoId} não encontrado.");

                if (!produto.Ativo)
                    throw new ArgumentException($"Produto {produto.Nome} está inativo.");

                if (produto.Estoque < item.Quantidade)
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para o produto {produto.Nome}. " +
                        $"Disponível: {produto.Estoque}, Solicitado: {item.Quantidade}");
            }
        }

        /// <summary>
        /// Atualiza o estoque dos produtos vendidos
        /// </summary>
        private void AtualizarEstoqueVenda(List<ItemVenda> itens, int vendaId)
        {
            foreach (var item in itens)
            {
                var produto = _produtoService.ObterPorId(item.ProdutoId);
                if (produto != null)
                {
                    int novoEstoque = produto.Estoque - item.Quantidade;
                    if (novoEstoque < 0)
                        novoEstoque = 0;

                    _produtoService.AtualizarEstoque(item.ProdutoId, novoEstoque);
                }
            }
        }

        /// <summary>
        /// Cria lançamento financeiro do tipo Receita para venda à vista
        /// </summary>
        private void CriarLancamentoFinanceiroReceita(Venda venda)
        {
            var lancamento = new LancamentoFinanceiro
            {
                Tipo = Constants.TipoReceita,
                Categoria = "Vendas",
                Descricao = $"Venda {venda.NumeroVenda} - {venda.FormaPagamento}",
                Valor = venda.ValorTotal,
                DataLancamento = venda.DataVenda,
                DataPagamento = venda.DataVenda,
                Status = Constants.StatusPago,
                FormaPagamento = venda.FormaPagamento,
                Origem = "Venda",
                OrigemId = venda.Id,
                Observacoes = $"Venda à vista - {venda.NumeroVenda}"
            };

            _financeiroService.RegistrarLancamento(lancamento);
        }

        /// <summary>
        /// Inicializa o arquivo de vendas com cabeçalhos se não existir
        /// </summary>
        private void InicializarArquivo()
        {
            if (!_excelService.FileExists(Constants.VendasFile))
            {
                using var package = new ExcelPackage();
                
                // Sheet: Vendas
                var vendasWorksheet = package.Workbook.Worksheets.Add(VendasSheetName);
                vendasWorksheet.Cells[1, 1].Value = "Id";
                vendasWorksheet.Cells[1, 2].Value = "NumeroVenda";
                vendasWorksheet.Cells[1, 3].Value = "ClienteId";
                vendasWorksheet.Cells[1, 4].Value = "DataVenda";
                vendasWorksheet.Cells[1, 5].Value = "ValorSubtotal";
                vendasWorksheet.Cells[1, 6].Value = "ValorDesconto";
                // Nota: Na documentação é "ValorDesconto", mas no Model é "Desconto"
                vendasWorksheet.Cells[1, 7].Value = "ValorTotal";
                vendasWorksheet.Cells[1, 8].Value = "FormaPagamento";
                vendasWorksheet.Cells[1, 9].Value = "Status";
                vendasWorksheet.Cells[1, 10].Value = "Vendedor";
                vendasWorksheet.Cells[1, 11].Value = "Observacoes";

                // Sheet: ItensVenda
                var itensWorksheet = package.Workbook.Worksheets.Add(ItensVendaSheetName);
                itensWorksheet.Cells[1, 1].Value = "Id";
                itensWorksheet.Cells[1, 2].Value = "VendaId";
                itensWorksheet.Cells[1, 3].Value = "ProdutoId";
                itensWorksheet.Cells[1, 4].Value = "Quantidade";
                itensWorksheet.Cells[1, 5].Value = "PrecoUnitario";
                itensWorksheet.Cells[1, 6].Value = "Desconto";
                itensWorksheet.Cells[1, 7].Value = "ValorTotal";

                // Formatação dos cabeçalhos
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension?.End.Column ?? 1])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }
                }

                _excelService.SavePackage(package, Constants.VendasFile);
            }
        }

        /// <summary>
        /// Salva a venda e seus itens no Excel
        /// </summary>
        private int SalvarVenda(Venda venda)
        {
            using var package = _excelService.GetPackage(Constants.VendasFile) ?? new ExcelPackage();
            var vendasWorksheet = package.Workbook.Worksheets[VendasSheetName] ?? package.Workbook.Worksheets.Add(VendasSheetName);

            // Obtém próximo ID
            int novoId = ObterProximoIdVenda(vendasWorksheet);
            venda.Id = novoId;

            // Encontra próxima linha vazia
            int novaLinha = _excelService.GetNextRow(vendasWorksheet);

            // Escreve dados da venda
            vendasWorksheet.Cells[novaLinha, 1].Value = venda.Id;
            vendasWorksheet.Cells[novaLinha, 2].Value = venda.NumeroVenda;
            vendasWorksheet.Cells[novaLinha, 3].Value = venda.ClienteId > 0 ? venda.ClienteId : (object?)null;
            vendasWorksheet.Cells[novaLinha, 4].Value = venda.DataVenda;
            vendasWorksheet.Cells[novaLinha, 5].Value = venda.ValorSubtotal;
            vendasWorksheet.Cells[novaLinha, 6].Value = venda.Desconto;
            vendasWorksheet.Cells[novaLinha, 7].Value = venda.ValorTotal;
            vendasWorksheet.Cells[novaLinha, 8].Value = venda.FormaPagamento;
            vendasWorksheet.Cells[novaLinha, 9].Value = venda.Status;
            vendasWorksheet.Cells[novaLinha, 10].Value = venda.Vendedor;
            vendasWorksheet.Cells[novaLinha, 11].Value = venda.Observacoes;

            _excelService.SavePackage(package, Constants.VendasFile);

            // Salva itens da venda
            SalvarItensVenda(venda.Itens, venda.Id);

            return venda.Id;
        }

        /// <summary>
        /// Salva os itens da venda no Excel
        /// </summary>
        private void SalvarItensVenda(List<ItemVenda> itens, int vendaId)
        {
            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return;

            var itensWorksheet = package.Workbook.Worksheets[ItensVendaSheetName];
            if (itensWorksheet == null) return;

            int proximoIdItem = ObterProximoIdItem(itensWorksheet);

            foreach (var item in itens)
            {
                int novaLinha = _excelService.GetNextRow(itensWorksheet);
                item.Id = proximoIdItem++;
                item.VendaId = vendaId;

                itensWorksheet.Cells[novaLinha, 1].Value = item.Id;
                itensWorksheet.Cells[novaLinha, 2].Value = item.VendaId;
                itensWorksheet.Cells[novaLinha, 3].Value = item.ProdutoId;
                itensWorksheet.Cells[novaLinha, 4].Value = item.Quantidade;
                itensWorksheet.Cells[novaLinha, 5].Value = item.PrecoUnitario;
                itensWorksheet.Cells[novaLinha, 6].Value = item.Desconto;
                itensWorksheet.Cells[novaLinha, 7].Value = item.ValorTotal;
            }

            _excelService.SavePackage(package, Constants.VendasFile);
        }

        /// <summary>
        /// Gera um número único para a venda
        /// </summary>
        private string GerarNumeroVenda()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"VENDA-{timestamp}-{random}";
        }

        /// <summary>
        /// Obtém o próximo ID disponível para venda
        /// </summary>
        private int ObterProximoIdVenda(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension == null)
                return 1;

            int maxId = 0;
            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id.HasValue && id.Value > maxId)
                    maxId = id.Value;
            }

            return maxId + 1;
        }

        /// <summary>
        /// Obtém o próximo ID disponível para item de venda
        /// </summary>
        private int ObterProximoIdItem(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension == null)
                return 1;

            int maxId = 0;
            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id.HasValue && id.Value > maxId)
                    maxId = id.Value;
            }

            return maxId + 1;
        }

        /// <summary>
        /// Obtém todas as vendas de um cliente específico
        /// </summary>
        public List<Venda> ObterVendasPorCliente(int clienteId)
        {
            var vendas = new List<Venda>();

            if (!_excelService.FileExists(Constants.VendasFile))
                return vendas;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return vendas;

            var worksheet = package.Workbook.Worksheets[VendasSheetName];
            if (worksheet == null || worksheet.Dimension == null) return vendas;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var vendaClienteId = worksheet.Cells[row, 3].GetValue<int?>();
                if (vendaClienteId == clienteId)
                {
                    var venda = LerVendaDaLinha(worksheet, row);
                    if (venda != null)
                    {
                        venda.Itens = ObterItensVenda(venda.Id);
                        vendas.Add(venda);
                    }
                }
            }

            return vendas.OrderByDescending(v => v.DataVenda).ToList();
        }

        /// <summary>
        /// Obtém todas as vendas
        /// </summary>
        public List<Venda> ObterTodas()
        {
            var vendas = new List<Venda>();

            if (!_excelService.FileExists(Constants.VendasFile))
                return vendas;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return vendas;

            var worksheet = package.Workbook.Worksheets[VendasSheetName];
            if (worksheet == null || worksheet.Dimension == null) return vendas;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var venda = LerVendaDaLinha(worksheet, row);
                if (venda != null)
                {
                    venda.Itens = ObterItensVenda(venda.Id);
                    vendas.Add(venda);
                }
            }

            return vendas;
        }

        /// <summary>
        /// Obtém o custo total das vendas finalizadas em um período (para lucro estimado).
        /// </summary>
        public decimal ObterCustoTotalVendasPeriodo(DateTime inicio, DateTime fim)
        {
            var vendas = ObterTodas()
                .Where(v => v.Status == "Finalizada" && v.DataVenda >= inicio && v.DataVenda <= fim)
                .ToList();
            decimal custo = 0;
            foreach (var venda in vendas)
            {
                var itens = venda.Itens ?? ObterItensVenda(venda.Id);
                foreach (var item in itens)
                {
                    var produto = _produtoService.ObterPorId(item.ProdutoId);
                    if (produto != null)
                        custo += produto.Custo * item.Quantidade;
                }
            }
            return custo;
        }

        /// <summary>
        /// Obtém uma venda por número de venda
        /// </summary>
        public Venda? ObterPorNumeroVenda(string numeroVenda)
        {
            if (string.IsNullOrWhiteSpace(numeroVenda))
                return null;

            if (!_excelService.FileExists(Constants.VendasFile))
                return null;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return null;

            var worksheet = package.Workbook.Worksheets[VendasSheetName];
            if (worksheet == null || worksheet.Dimension == null) return null;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var numero = worksheet.Cells[row, 2].GetValue<string>();
                    if (numero == numeroVenda)
                    {
                        var venda = LerVendaDaLinha(worksheet, row);
                        if (venda != null)
                        {
                            venda.Itens = ObterItensVenda(venda.Id);
                        }
                        return venda;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém uma venda por ID
        /// </summary>
        public Venda? ObterPorId(int id)
        {
            if (!_excelService.FileExists(Constants.VendasFile))
                return null;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return null;

            var worksheet = package.Workbook.Worksheets[VendasSheetName];
            if (worksheet == null || worksheet.Dimension == null) return null;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var vendaId = worksheet.Cells[row, 1].GetValue<int?>();
                if (vendaId == id)
                {
                    var venda = LerVendaDaLinha(worksheet, row);
                    if (venda != null)
                    {
                        venda.Itens = ObterItensVenda(venda.Id);
                    }
                    return venda;
                }
            }

            return null;
        }

        /// <summary>
        /// Cancela uma venda
        /// </summary>
        /// <param name="vendaId">ID da venda a ser cancelada</param>
        /// <param name="motivo">Motivo do cancelamento</param>
        public void CancelarVenda(int vendaId, string motivo = "")
        {
            var venda = ObterPorId(vendaId);
            if (venda == null)
                throw new KeyNotFoundException($"Venda com ID {vendaId} não encontrada.");

            if (venda.Status == "Cancelada")
                throw new InvalidOperationException("Venda já está cancelada.");

            // Retornar produtos ao estoque
            foreach (var item in venda.Itens)
            {
                var produto = _produtoService.ObterPorId(item.ProdutoId);
                if (produto != null)
                {
                    int novoEstoque = produto.Estoque + item.Quantidade;
                    _produtoService.AtualizarEstoque(item.ProdutoId, novoEstoque);
                }
            }

            // Estornar lançamento financeiro (se existir)
            if (venda.Status == "Finalizada" && venda.FormaPagamento != Constants.PagamentoCredito)
            {
                var lancamentoEstorno = new LancamentoFinanceiro
                {
                    Tipo = Constants.TipoDespesa,
                    Categoria = "Estorno",
                    Descricao = $"Estorno - Venda cancelada {venda.NumeroVenda}",
                    Valor = venda.ValorTotal,
                    DataLancamento = DateTime.Now,
                    DataPagamento = DateTime.Now,
                    Status = Constants.StatusPago,
                    FormaPagamento = venda.FormaPagamento,
                    Origem = "Cancelamento de Venda",
                    OrigemId = venda.Id,
                    Observacoes = $"Estorno da venda {venda.NumeroVenda}. Motivo: {motivo}"
                };
                _financeiroService.RegistrarDespesa(lancamentoEstorno);
            }

            // Se fiado, devolver crédito ao cliente
            if (venda.FormaPagamento == Constants.PagamentoCredito && venda.Cliente != null)
            {
                _creditoService.RegistrarPagamento(venda.Cliente, venda.ValorTotal, 
                    $"Estorno - Venda cancelada {venda.NumeroVenda}");
            }

            // Atualizar status da venda
            AtualizarStatusVenda(vendaId, "Cancelada");
        }

        /// <summary>
        /// Devolve itens de uma venda (total ou parcial)
        /// </summary>
        /// <param name="vendaId">ID da venda</param>
        /// <param name="itensDevolver">Itens a serem devolvidos (ID do ItemVenda e quantidade)</param>
        /// <param name="observacao">Observação sobre a devolução</param>
        public void DevolverVenda(int vendaId, Dictionary<int, int> itensDevolver, string observacao = "")
        {
            var venda = ObterPorId(vendaId);
            if (venda == null)
                throw new KeyNotFoundException($"Venda com ID {vendaId} não encontrada.");

            if (venda.Status == "Cancelada")
                throw new InvalidOperationException("Não é possível devolver uma venda cancelada.");

            decimal valorDevolvido = 0;

            // Processar cada item devolvido
            foreach (var itemDevolver in itensDevolver)
            {
                var item = venda.Itens.FirstOrDefault(i => i.Id == itemDevolver.Key);
                if (item == null) continue;

                int quantidadeDevolver = Math.Min(itemDevolver.Value, item.Quantidade);
                if (quantidadeDevolver <= 0) continue;

                // Retornar ao estoque
                var produto = _produtoService.ObterPorId(item.ProdutoId);
                if (produto != null)
                {
                    int novoEstoque = produto.Estoque + quantidadeDevolver;
                    _produtoService.AtualizarEstoque(item.ProdutoId, novoEstoque);
                }

                decimal valorProporcional = _calculoFinanceiro.CalcularValorProporcionalDevolucao(item, quantidadeDevolver);
                valorDevolvido += valorProporcional;
            }

            if (valorDevolvido <= 0)
                throw new InvalidOperationException("Nenhum item válido para devolução.");

            // Gerar lançamento financeiro negativo (estorno)
            if (venda.Status == "Finalizada" && venda.FormaPagamento != Constants.PagamentoCredito)
            {
                var lancamentoEstorno = new LancamentoFinanceiro
                {
                    Tipo = Constants.TipoDespesa,
                    Categoria = "Devolução",
                    Descricao = $"Devolução - Venda {venda.NumeroVenda}",
                    Valor = valorDevolvido,
                    DataLancamento = DateTime.Now,
                    DataPagamento = DateTime.Now,
                    Status = Constants.StatusPago,
                    FormaPagamento = venda.FormaPagamento,
                    Origem = "Devolução de Venda",
                    OrigemId = venda.Id,
                    Observacoes = string.IsNullOrWhiteSpace(observacao) 
                        ? $"Devolução parcial da venda {venda.NumeroVenda}" 
                        : observacao
                };
                _financeiroService.RegistrarDespesa(lancamentoEstorno);
            }

            // Se fiado, ajustar crédito do cliente
            if (venda.FormaPagamento == Constants.PagamentoCredito && venda.Cliente != null)
            {
                _creditoService.RegistrarPagamento(venda.Cliente, valorDevolvido, 
                    $"Devolução - Venda {venda.NumeroVenda}");
            }

            // Atualizar observações da venda
            if (!string.IsNullOrWhiteSpace(observacao))
            {
                venda.Observacoes += $"\n[DEVOLUÇÃO] {DateTime.Now:dd/MM/yyyy HH:mm} - {observacao}";
                AtualizarObservacoesVenda(vendaId, venda.Observacoes);
            }
        }

        /// <summary>
        /// Realiza troca de itens de uma venda
        /// </summary>
        /// <param name="vendaId">ID da venda original</param>
        /// <param name="itensDevolver">Itens a serem devolvidos</param>
        /// <param name="itensNovos">Novos itens para troca</param>
        /// <param name="observacao">Observação sobre a troca</param>
        public void TrocarVenda(int vendaId, Dictionary<int, int> itensDevolver, List<ItemVenda> itensNovos, string observacao = "")
        {
            var venda = ObterPorId(vendaId);
            if (venda == null)
                throw new KeyNotFoundException($"Venda com ID {vendaId} não encontrada.");

            // Validar novos itens
            ValidarEstoqueDisponivel(itensNovos);

            // Calcular valor dos itens devolvidos
            decimal valorDevolvido = 0;
            foreach (var itemDevolver in itensDevolver)
            {
                var item = venda.Itens.FirstOrDefault(i => i.Id == itemDevolver.Key);
                if (item != null)
                {
                    int quantidade = Math.Min(itemDevolver.Value, item.Quantidade);
                    decimal valorProporcional = _calculoFinanceiro.CalcularValorProporcionalDevolucao(item, quantidade);
                    valorDevolvido += valorProporcional;

                    // Retornar ao estoque
                    var produto = _produtoService.ObterPorId(item.ProdutoId);
                    if (produto != null)
                    {
                        int novoEstoque = produto.Estoque + quantidade;
                        _produtoService.AtualizarEstoque(item.ProdutoId, novoEstoque);
                    }
                }
            }

            decimal valorNovos = _calculoFinanceiro.CalcularSubtotalItens(itensNovos);

            // Processar diferença de valores
            decimal diferenca = valorNovos - valorDevolvido;

            // Baixar estoque dos novos itens
            foreach (var itemNovo in itensNovos)
            {
                var produto = _produtoService.ObterPorId(itemNovo.ProdutoId);
                if (produto != null)
                {
                    int novoEstoque = produto.Estoque - itemNovo.Quantidade;
                    if (novoEstoque < 0) novoEstoque = 0;
                    _produtoService.AtualizarEstoque(itemNovo.ProdutoId, novoEstoque);
                }
            }

            // Se diferença positiva, gerar cobrança
            if (diferenca > 0)
            {
                if (venda.FormaPagamento == Constants.PagamentoCredito && venda.Cliente != null)
                {
                    // Adicionar ao crédito
                    var vendaAdicional = new Venda
                    {
                        Cliente = venda.Cliente,
                        ClienteId = venda.ClienteId,
                        Itens = itensNovos,
                        ValorTotal = diferenca,
                        FormaPagamento = Constants.PagamentoCredito,
                        Status = "Pendente"
                    };
                    _creditoService.RegistrarVendaFiada(venda.Cliente, vendaAdicional);
                }
                else
                {
                    // Gerar lançamento financeiro
                    var lancamento = new LancamentoFinanceiro
                    {
                        Tipo = Constants.TipoReceita,
                        Categoria = "Troca",
                        Descricao = $"Troca - Venda {venda.NumeroVenda}",
                        Valor = diferenca,
                        DataLancamento = DateTime.Now,
                        DataPagamento = DateTime.Now,
                        Status = Constants.StatusPago,
                        FormaPagamento = venda.FormaPagamento,
                        Origem = "Troca de Venda",
                        OrigemId = venda.Id,
                        Observacoes = $"Diferença positiva na troca da venda {venda.NumeroVenda}"
                    };
                    _financeiroService.RegistrarReceita(lancamento);
                }
            }
            // Se diferença negativa, gerar crédito ou devolução
            else if (diferenca < 0)
            {
                decimal valorCredito = Math.Abs(diferenca);
                
                if (venda.FormaPagamento == Constants.PagamentoCredito && venda.Cliente != null)
                {
                    _creditoService.RegistrarPagamento(venda.Cliente, valorCredito, 
                        $"Crédito - Troca venda {venda.NumeroVenda}");
                }
                else
                {
                    var lancamento = new LancamentoFinanceiro
                    {
                        Tipo = Constants.TipoDespesa,
                        Categoria = "Troca",
                        Descricao = $"Estorno - Troca Venda {venda.NumeroVenda}",
                        Valor = valorCredito,
                        DataLancamento = DateTime.Now,
                        DataPagamento = DateTime.Now,
                        Status = Constants.StatusPago,
                        FormaPagamento = venda.FormaPagamento,
                        Origem = "Troca de Venda",
                        OrigemId = venda.Id,
                        Observacoes = $"Diferença negativa na troca da venda {venda.NumeroVenda}"
                    };
                    _financeiroService.RegistrarDespesa(lancamento);
                }
            }

            // Atualizar observações da venda
            venda.Observacoes += $"\n[TROCA] {DateTime.Now:dd/MM/yyyy HH:mm} - {observacao}";
            AtualizarObservacoesVenda(vendaId, venda.Observacoes);
        }

        /// <summary>
        /// Lê uma venda de uma linha do Excel
        /// </summary>
        private Venda? LerVendaDaLinha(ExcelWorksheet worksheet, int row)
        {
            try
            {
                var clienteId = worksheet.Cells[row, 3].GetValue<int?>() ?? 0;

                return new Venda
                {
                    Id = worksheet.Cells[row, 1].GetValue<int>(),
                    NumeroVenda = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty,
                    ClienteId = clienteId,
                    Cliente = clienteId > 0 ? _clienteService.ObterPorId(clienteId) : null,
                    DataVenda = worksheet.Cells[row, 4].GetValue<DateTime>(),
                    ValorSubtotal = worksheet.Cells[row, 5].GetValue<decimal>(),
                    Desconto = worksheet.Cells[row, 6].GetValue<decimal>(),
                    ValorTotal = worksheet.Cells[row, 7].GetValue<decimal>(),
                    FormaPagamento = worksheet.Cells[row, 8].GetValue<string>() ?? string.Empty,
                    Status = worksheet.Cells[row, 9].GetValue<string>() ?? "Pendente",
                    Vendedor = worksheet.Cells[row, 10].GetValue<string>() ?? string.Empty,
                    Observacoes = worksheet.Cells[row, 11].GetValue<string>() ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtém os itens de uma venda
        /// </summary>
        private List<ItemVenda> ObterItensVenda(int vendaId)
        {
            var itens = new List<ItemVenda>();

            if (!_excelService.FileExists(Constants.VendasFile))
                return itens;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return itens;

            var worksheet = package.Workbook.Worksheets[ItensVendaSheetName];
            if (worksheet == null || worksheet.Dimension == null) return itens;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var itemVendaId = worksheet.Cells[row, 2].GetValue<int>();
                    if (itemVendaId == vendaId)
                    {
                        var item = new ItemVenda
                        {
                            Id = worksheet.Cells[row, 1].GetValue<int>(),
                            VendaId = itemVendaId,
                            ProdutoId = worksheet.Cells[row, 3].GetValue<int>(),
                            Quantidade = worksheet.Cells[row, 4].GetValue<int>(),
                            PrecoUnitario = worksheet.Cells[row, 5].GetValue<decimal>(),
                            Desconto = worksheet.Cells[row, 6].GetValue<decimal>()
                        };
                        itens.Add(item);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return itens;
        }

        /// <summary>
        /// Atualiza o status de uma venda
        /// </summary>
        private void AtualizarStatusVenda(int vendaId, string novoStatus)
        {
            if (!_excelService.FileExists(Constants.VendasFile))
                return;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return;

            var worksheet = package.Workbook.Worksheets[VendasSheetName];
            if (worksheet == null || worksheet.Dimension == null) return;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id == vendaId)
                {
                    worksheet.Cells[row, 9].Value = novoStatus; // Status na coluna 9
                    _excelService.SavePackage(package, Constants.VendasFile);
                    return;
                }
            }
        }

        /// <summary>
        /// Atualiza as observações de uma venda
        /// </summary>
        private void AtualizarObservacoesVenda(int vendaId, string observacoes)
        {
            if (!_excelService.FileExists(Constants.VendasFile))
                return;

            using var package = _excelService.GetPackage(Constants.VendasFile);
            if (package == null) return;

            var worksheet = package.Workbook.Worksheets[VendasSheetName];
            if (worksheet == null || worksheet.Dimension == null) return;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var id = worksheet.Cells[row, 1].GetValue<int?>();
                if (id == vendaId)
                {
                    worksheet.Cells[row, 11].Value = observacoes; // Observacoes na coluna 11
                    _excelService.SavePackage(package, Constants.VendasFile);
                    return;
                }
            }
        }
    }
}
