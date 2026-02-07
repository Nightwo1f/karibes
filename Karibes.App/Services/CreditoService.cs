using OfficeOpenXml;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço responsável pelo controle de crédito (fiado) dos clientes
    /// </summary>
    public class CreditoService
    {
        private readonly ExcelService _excelService;
        private const string HistoricoSheetName = "HistoricoCredito";
        private const string ClientesSheetName = "Clientes";

        public CreditoService()
        {
            _excelService = new ExcelService();
            InicializarArquivoHistorico();
        }

        /// <summary>
        /// Valida se um cliente pode comprar no crédito (fiado)
        /// </summary>
        /// <param name="cliente">Cliente a ser validado</param>
        /// <param name="valor">Valor da compra</param>
        /// <returns>True se pode comprar, False caso contrário</returns>
        public bool PodeComprarNoCredito(Cliente? cliente, decimal valor)
        {
            // Retorna false se cliente for null
            if (cliente == null)
                return false;

            // Retorna false se valor <= 0
            if (valor <= 0)
                return false;

            // Retorna false se cliente.SaldoDevedor + valor > cliente.LimiteCredito
            if (cliente.SaldoDevedor + valor > cliente.LimiteCredito)
                return false;

            // Caso contrário, retorna true
            return true;
        }

        /// <summary>
        /// Registra débito de uma venda fiada
        /// </summary>
        /// <param name="cliente">Cliente que realizou a compra</param>
        /// <param name="venda">Venda realizada</param>
        public void RegistrarVendaFiada(Cliente cliente, Venda venda)
        {
            // Validar PodeComprarNoCredito
            if (!PodeComprarNoCredito(cliente, venda.ValorTotal))
            {
                throw new InvalidOperationException(
                    $"Cliente não pode comprar no crédito. Saldo devedor atual: {cliente.SaldoDevedor:C}, " +
                    $"Limite: {cliente.LimiteCredito:C}, Valor da compra: {venda.ValorTotal:C}");
            }

            // Salvar saldo anterior
            decimal saldoAnterior = cliente.SaldoDevedor;

            // Atualizar cliente.SaldoDevedor
            cliente.SaldoDevedor += venda.ValorTotal;
            cliente.DataUltimaAtualizacao = DateTime.Now;

            // Atualizar cliente no Excel
            AtualizarSaldoCliente(cliente);

            // Criar registro em HistoricoCredito
            var historico = new HistoricoCredito
            {
                ClienteId = cliente.Id,
                Cliente = cliente,
                DataMovimento = DateTime.Now,
                TipoMovimento = "Compra",
                Valor = venda.ValorTotal,
                SaldoAnterior = saldoAnterior,
                SaldoAtual = cliente.SaldoDevedor,
                VendaId = venda.Id,
                Observacoes = $"Venda fiada - {venda.NumeroVenda}"
            };

            SalvarHistoricoCredito(historico);
        }

        /// <summary>
        /// Registra crédito de um pagamento mensal
        /// </summary>
        /// <param name="cliente">Cliente que realizou o pagamento</param>
        /// <param name="valor">Valor do pagamento</param>
        /// <param name="observacao">Observação sobre o pagamento</param>
        public void RegistrarPagamento(Cliente cliente, decimal valor, string observacao = "")
        {
            // Validar valor > 0
            if (valor <= 0)
            {
                throw new ArgumentException("Valor do pagamento deve ser maior que zero.", nameof(valor));
            }

            // Salvar saldo anterior
            decimal saldoAnterior = cliente.SaldoDevedor;

            // Reduzir cliente.SaldoDevedor
            cliente.SaldoDevedor -= valor;

            // Garantir que saldo não fique negativo
            if (cliente.SaldoDevedor < 0)
            {
                cliente.SaldoDevedor = 0;
            }

            cliente.DataUltimaAtualizacao = DateTime.Now;

            // Atualizar cliente no Excel
            AtualizarSaldoCliente(cliente);

            // Criar registro em HistoricoCredito
            var historico = new HistoricoCredito
            {
                ClienteId = cliente.Id,
                Cliente = cliente,
                DataMovimento = DateTime.Now,
                TipoMovimento = "Pagamento",
                Valor = valor,
                SaldoAnterior = saldoAnterior,
                SaldoAtual = cliente.SaldoDevedor,
                VendaId = null,
                Observacoes = string.IsNullOrWhiteSpace(observacao) ? "Pagamento de crédito" : observacao
            };

            SalvarHistoricoCredito(historico);
        }

        /// <summary>
        /// Inicializa o arquivo de histórico de crédito com cabeçalhos
        /// </summary>
        private void InicializarArquivoHistorico()
        {
            if (!_excelService.FileExists(Constants.HistoricoCreditoFile))
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(HistoricoSheetName);

                // Cabeçalhos
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "ClienteId";
                worksheet.Cells[1, 3].Value = "DataMovimento";
                worksheet.Cells[1, 4].Value = "TipoMovimento";
                worksheet.Cells[1, 5].Value = "Valor";
                worksheet.Cells[1, 6].Value = "SaldoAnterior";
                worksheet.Cells[1, 7].Value = "SaldoAtual";
                worksheet.Cells[1, 8].Value = "VendaId";
                worksheet.Cells[1, 9].Value = "Observacoes";

                // Formatação do cabeçalho
                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                _excelService.SavePackage(package, Constants.HistoricoCreditoFile);
            }
        }

        /// <summary>
        /// Atualiza o saldo devedor do cliente na planilha
        /// </summary>
        private void AtualizarSaldoCliente(Cliente cliente)
        {
            if (!_excelService.FileExists(Constants.ClientesFile))
                return;

            using var package = _excelService.GetPackage(Constants.ClientesFile);
            if (package == null) return;

            var worksheet = package.Workbook.Worksheets[ClientesSheetName];
            if (worksheet == null || worksheet.Dimension == null) return;

            int rowCount = worksheet.Dimension.End.Row;

            // Encontra a linha do cliente
            for (int row = 2; row <= rowCount; row++)
            {
                var clienteId = worksheet.Cells[row, 1].GetValue<int?>();
                if (clienteId == cliente.Id)
                {
                    // Atualiza SaldoDevedor (coluna 17) e DataUltimaAtualizacao (coluna 19)
                    // Estrutura: Id(1), Codigo(2), Nome(3), TipoDocumento(4), Documento(5),
                    // Email(6), Telefone(7), Celular(8), CEP(9), Endereco(10), Numero(11),
                    // Complemento(12), Bairro(13), Cidade(14), Estado(15), LimiteCredito(16),
                    // SaldoDevedor(17), DataCadastro(18), DataUltimaAtualizacao(19), Ativo(20), Observacoes(21)
                    worksheet.Cells[row, 17].Value = cliente.SaldoDevedor;
                    worksheet.Cells[row, 19].Value = cliente.DataUltimaAtualizacao;
                    _excelService.SavePackage(package, Constants.ClientesFile);
                    return;
                }
            }
        }

        /// <summary>
        /// Salva um registro de histórico de crédito
        /// </summary>
        private void SalvarHistoricoCredito(HistoricoCredito historico)
        {
            using var package = _excelService.GetPackage(Constants.HistoricoCreditoFile) ?? new ExcelPackage();
            var worksheet = package.Workbook.Worksheets[HistoricoSheetName] ?? package.Workbook.Worksheets.Add(HistoricoSheetName);

            // Obtém próximo ID
            int novoId = ObterProximoIdHistorico(worksheet);
            historico.Id = novoId;

            // Encontra próxima linha vazia
            int novaLinha = _excelService.GetNextRow(worksheet);

            // Escreve dados
            worksheet.Cells[novaLinha, 1].Value = historico.Id;
            worksheet.Cells[novaLinha, 2].Value = historico.ClienteId;
            worksheet.Cells[novaLinha, 3].Value = historico.DataMovimento;
            worksheet.Cells[novaLinha, 4].Value = historico.TipoMovimento;
            worksheet.Cells[novaLinha, 5].Value = historico.Valor;
            worksheet.Cells[novaLinha, 6].Value = historico.SaldoAnterior;
            worksheet.Cells[novaLinha, 7].Value = historico.SaldoAtual;
            worksheet.Cells[novaLinha, 8].Value = historico.VendaId?.ToString() ?? "";
            worksheet.Cells[novaLinha, 9].Value = historico.Observacoes;

            _excelService.SavePackage(package, Constants.HistoricoCreditoFile);
        }

        /// <summary>
        /// Obtém o próximo ID disponível para histórico
        /// </summary>
        private int ObterProximoIdHistorico(ExcelWorksheet worksheet)
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
        /// Obtém histórico de crédito de um cliente específico
        /// </summary>
        public List<HistoricoCredito> ObterHistoricoPorCliente(int clienteId)
        {
            var historico = new List<HistoricoCredito>();

            if (!_excelService.FileExists(Constants.HistoricoCreditoFile))
                return historico;

            using var package = _excelService.GetPackage(Constants.HistoricoCreditoFile);
            if (package == null) return historico;

            var worksheet = package.Workbook.Worksheets[HistoricoSheetName];
            if (worksheet == null || worksheet.Dimension == null) return historico;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var historicoClienteId = worksheet.Cells[row, 2].GetValue<int?>();
                if (historicoClienteId == clienteId)
                {
                    try
                    {
                        var item = new HistoricoCredito
                        {
                            Id = worksheet.Cells[row, 1].GetValue<int>(),
                            ClienteId = worksheet.Cells[row, 2].GetValue<int>(),
                            DataMovimento = worksheet.Cells[row, 3].GetValue<DateTime>(),
                            TipoMovimento = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
                            Valor = worksheet.Cells[row, 5].GetValue<decimal>(),
                            SaldoAnterior = worksheet.Cells[row, 6].GetValue<decimal>(),
                            SaldoAtual = worksheet.Cells[row, 7].GetValue<decimal>(),
                            VendaId = worksheet.Cells[row, 8].GetValue<int?>(),
                            Observacoes = worksheet.Cells[row, 9].GetValue<string>() ?? string.Empty
                        };
                        historico.Add(item);
                    }
                    catch
                    {
                        // Ignora linhas com erro
                    }
                }
            }

            return historico.OrderByDescending(h => h.DataMovimento).ToList();
        }
    }
}

