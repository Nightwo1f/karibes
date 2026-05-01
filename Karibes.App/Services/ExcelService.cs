using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using Karibes.App.Utils;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço base para manipulação de arquivos Excel
    /// </summary>
    public class ExcelService
    {
        private readonly string _dataPath;

        public ExcelService()
        {
            var baseDirectory = AppContext.BaseDirectory;
            _dataPath = Path.Combine(baseDirectory, Constants.DataFolder, Constants.ExcelFolder);
            Directory.CreateDirectory(_dataPath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Obtém o caminho completo do arquivo Excel
        /// </summary>
        public string GetFilePath(string fileName)
        {
            return Path.Combine(_dataPath, fileName);
        }

        /// <summary>
        /// Verifica se um arquivo Excel existe
        /// </summary>
        public bool FileExists(string fileName)
        {
            var filePath = GetFilePath(fileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Cria um arquivo Excel vazio com a estrutura básica
        /// </summary>
        public void CreateExcelFile(string fileName, string sheetName = "Sheet1")
        {
            var filePath = GetFilePath(fileName);
            
            if (File.Exists(filePath))
                return;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            package.SaveAs(new FileInfo(filePath));
        }

        /// <summary>
        /// Obtém uma worksheet do arquivo Excel
        /// </summary>
        public ExcelWorksheet? GetWorksheet(string fileName, string sheetName = "Sheet1")
        {
            var filePath = GetFilePath(fileName);
            
            if (!File.Exists(filePath))
                return null;

            try
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                return package.Workbook.Worksheets[sheetName];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Obtém um ExcelPackage para manipulação
        /// </summary>
        public ExcelPackage? GetPackage(string fileName)
        {
            var filePath = GetFilePath(fileName);
            
            if (!File.Exists(filePath))
                return null;

            try
            {
                return new ExcelPackage(new FileInfo(filePath));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Salva um ExcelPackage
        /// </summary>
        public void SavePackage(ExcelPackage package, string fileName)
        {
            var filePath = GetFilePath(fileName);
            package.SaveAs(new FileInfo(filePath));
        }

        /// <summary>
        /// Obtém o número da próxima linha vazia na worksheet
        /// </summary>
        public int GetNextRow(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension == null)
                return 2; // Linha 1 é cabeçalho, começa na 2

            return worksheet.Dimension.End.Row + 1;
        }

        /// <summary>
        /// Valida se o arquivo Excel não está corrompido
        /// </summary>
        public bool ValidateFile(string fileName)
        {
            var filePath = GetFilePath(fileName);
            
            if (!File.Exists(filePath))
                return false;

            try
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                return package.Workbook.Worksheets.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
