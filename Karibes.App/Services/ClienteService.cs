using OfficeOpenXml;
using Karibes.App.Models;
using Karibes.App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço para gerenciamento de clientes usando Excel como banco de dados
    /// </summary>
    public class ClienteService
    {
        private readonly ExcelService _excelService;
        private const string SheetName = "Clientes";

        public ClienteService()
        {
            _excelService = new ExcelService();
            InicializarArquivo();
        }

        /// <summary>
        /// Inicializa o arquivo Excel com cabeçalhos se não existir
        /// </summary>
        private void InicializarArquivo()
        {
            if (!_excelService.FileExists(Constants.ClientesFile))
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(SheetName);
                
                // Cabeçalhos conforme documentação
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Codigo";
                worksheet.Cells[1, 3].Value = "Nome";
                worksheet.Cells[1, 4].Value = "TipoDocumento";
                worksheet.Cells[1, 5].Value = "Documento";
                worksheet.Cells[1, 6].Value = "Email";
                worksheet.Cells[1, 7].Value = "Telefone";
                worksheet.Cells[1, 8].Value = "Celular";
                worksheet.Cells[1, 9].Value = "CEP";
                worksheet.Cells[1, 10].Value = "Endereco";
                worksheet.Cells[1, 11].Value = "Numero";
                worksheet.Cells[1, 12].Value = "Complemento";
                worksheet.Cells[1, 13].Value = "Bairro";
                worksheet.Cells[1, 14].Value = "Cidade";
                worksheet.Cells[1, 15].Value = "Estado";
                worksheet.Cells[1, 16].Value = "LimiteCredito";
                worksheet.Cells[1, 17].Value = "SaldoDevedor";
                worksheet.Cells[1, 18].Value = "DataCadastro";
                worksheet.Cells[1, 19].Value = "DataUltimaAtualizacao";
                worksheet.Cells[1, 20].Value = "Ativo";
                worksheet.Cells[1, 21].Value = "Observacoes";
                worksheet.Cells[1, 22].Value = "TotalPago";

                // Formatação do cabeçalho
                using (var range = worksheet.Cells[1, 1, 1, 22])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                _excelService.SavePackage(package, Constants.ClientesFile);
            }
        }

        /// <summary>
        /// Obtém todos os clientes ativos
        /// </summary>
        public List<Cliente> ObterTodos()
        {
            var clientes = new List<Cliente>();

            if (!_excelService.FileExists(Constants.ClientesFile))
                return clientes;

            using var package = _excelService.GetPackage(Constants.ClientesFile);
            if (package == null) return clientes;

            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet == null || worksheet.Dimension == null) return clientes;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var cliente = LerClienteDaLinha(worksheet, row);
                if (cliente != null)
                    clientes.Add(cliente);
            }

            return clientes;
        }

        /// <summary>
        /// Obtém um cliente por ID
        /// </summary>
        public Cliente? ObterPorId(int id)
        {
            if (!_excelService.FileExists(Constants.ClientesFile))
                return null;

            using var package = _excelService.GetPackage(Constants.ClientesFile);
            if (package == null) return null;

            var worksheet = package.Workbook.Worksheets[SheetName];
            if (worksheet == null || worksheet.Dimension == null) return null;

            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var clienteId = worksheet.Cells[row, 1].GetValue<int?>();
                if (clienteId == id)
                {
                    return LerClienteDaLinha(worksheet, row);
                }
            }

            return null;
        }

        /// <summary>
        /// Busca clientes por nome ou código
        /// </summary>
        public List<Cliente> Buscar(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return ObterTodos();

            var todos = ObterTodos();
            var termoLower = termo.ToLower();

            return todos.Where(c =>
                c.Nome.ToLower().Contains(termoLower) ||
                c.Codigo.ToLower().Contains(termoLower) ||
                c.Documento.Contains(termo)
            ).ToList();
        }

        /// <summary>
        /// Salva ou atualiza um cliente
        /// </summary>
        public void Salvar(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            InicializarArquivo();

            using var package = _excelService.GetPackage(Constants.ClientesFile) ?? new ExcelPackage();
            var worksheet = package.Workbook.Worksheets[SheetName] ?? package.Workbook.Worksheets.Add(SheetName);

            int rowCount = worksheet.Dimension?.End.Row ?? 1;
            int row = -1;

            // Se é um cliente existente, encontra a linha
            if (cliente.Id > 0)
            {
                for (int r = 2; r <= rowCount; r++)
                {
                    var id = worksheet.Cells[r, 1].GetValue<int?>();
                    if (id == cliente.Id)
                    {
                        row = r;
                        break;
                    }
                }
            }

            // Se não encontrou, é um novo cliente
            if (row == -1)
            {
                row = rowCount + 1;
                cliente.Id = row - 1; // ID = linha - 1 (linha 2 = ID 1)
                cliente.DataCadastro = DateTime.Now;
            }

            cliente.DataUltimaAtualizacao = DateTime.Now;

            // Salva os dados
            EscreverClienteNaLinha(worksheet, row, cliente);

            _excelService.SavePackage(package, Constants.ClientesFile);
        }

        /// <summary>
        /// Escreve um cliente em uma linha do Excel
        /// </summary>
        private void EscreverClienteNaLinha(ExcelWorksheet worksheet, int row, Cliente cliente)
        {
            worksheet.Cells[row, 1].Value = cliente.Id;
            worksheet.Cells[row, 2].Value = cliente.Codigo;
            worksheet.Cells[row, 3].Value = cliente.Nome;
            worksheet.Cells[row, 4].Value = cliente.TipoDocumento;
            worksheet.Cells[row, 5].Value = cliente.Documento;
            worksheet.Cells[row, 6].Value = cliente.Email;
            worksheet.Cells[row, 7].Value = cliente.Telefone;
            worksheet.Cells[row, 8].Value = cliente.Celular;
            worksheet.Cells[row, 9].Value = cliente.CEP;
            worksheet.Cells[row, 10].Value = cliente.Endereco;
            worksheet.Cells[row, 11].Value = cliente.Numero;
            worksheet.Cells[row, 12].Value = cliente.Complemento;
            worksheet.Cells[row, 13].Value = cliente.Bairro;
            worksheet.Cells[row, 14].Value = cliente.Cidade;
            worksheet.Cells[row, 15].Value = cliente.Estado;
            worksheet.Cells[row, 16].Value = cliente.LimiteCredito;
            worksheet.Cells[row, 17].Value = cliente.SaldoDevedor;
            worksheet.Cells[row, 18].Value = cliente.DataCadastro;
            worksheet.Cells[row, 19].Value = cliente.DataUltimaAtualizacao;
            worksheet.Cells[row, 20].Value = cliente.Ativo;
            worksheet.Cells[row, 21].Value = cliente.Observacoes;
            worksheet.Cells[row, 22].Value = cliente.TotalPago;
        }

        /// <summary>
        /// Lê um cliente de uma linha do Excel
        /// </summary>
        private Cliente? LerClienteDaLinha(ExcelWorksheet worksheet, int row)
        {
            try
            {
                return new Cliente
                {
                    Id = worksheet.Cells[row, 1].GetValue<int>(),
                    Codigo = worksheet.Cells[row, 2].GetValue<string>() ?? string.Empty,
                    Nome = worksheet.Cells[row, 3].GetValue<string>() ?? string.Empty,
                    TipoDocumento = worksheet.Cells[row, 4].GetValue<string>() ?? string.Empty,
                    Documento = worksheet.Cells[row, 5].GetValue<string>() ?? string.Empty,
                    Email = worksheet.Cells[row, 6].GetValue<string>() ?? string.Empty,
                    Telefone = worksheet.Cells[row, 7].GetValue<string>() ?? string.Empty,
                    Celular = worksheet.Cells[row, 8].GetValue<string>() ?? string.Empty,
                    CEP = worksheet.Cells[row, 9].GetValue<string>() ?? string.Empty,
                    Endereco = worksheet.Cells[row, 10].GetValue<string>() ?? string.Empty,
                    Numero = worksheet.Cells[row, 11].GetValue<string>() ?? string.Empty,
                    Complemento = worksheet.Cells[row, 12].GetValue<string>() ?? string.Empty,
                    Bairro = worksheet.Cells[row, 13].GetValue<string>() ?? string.Empty,
                    Cidade = worksheet.Cells[row, 14].GetValue<string>() ?? string.Empty,
                    Estado = worksheet.Cells[row, 15].GetValue<string>() ?? string.Empty,
                    LimiteCredito = worksheet.Cells[row, 16].GetValue<decimal>(),
                    SaldoDevedor = worksheet.Cells[row, 17].GetValue<decimal>(),
                    TotalPago = worksheet.Dimension?.End.Column >= 22 ? worksheet.Cells[row, 22].GetValue<decimal>() : 0,
                    DataCadastro = worksheet.Cells[row, 18].GetValue<DateTime>(),
                    DataUltimaAtualizacao = worksheet.Cells[row, 19].GetValue<DateTime>(),
                    Ativo = worksheet.Cells[row, 20].GetValue<bool>(),
                    Observacoes = worksheet.Cells[row, 21].GetValue<string>() ?? string.Empty,
                    Pagamentos = new List<PagamentoCliente>()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}





