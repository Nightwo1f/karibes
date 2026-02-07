using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Karibes.App.Services
{
    public class BackupService
    {
        private readonly string _dataPath;
        private readonly string _backupPath;

        public BackupService()
        {
            var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
            _dataPath = Path.Combine(baseDirectory, "Data", "Excel");
            _backupPath = Path.Combine(baseDirectory, "Backups");
            Directory.CreateDirectory(_backupPath);
        }

        public void CriarBackup()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"backup_{timestamp}.zip";
            var backupFilePath = Path.Combine(_backupPath, backupFileName);

            if (Directory.Exists(_dataPath))
            {
                ZipFile.CreateFromDirectory(_dataPath, backupFilePath);
            }
        }

        public void RestaurarBackup(string backupFilePath)
        {
            if (File.Exists(backupFilePath))
            {
                if (Directory.Exists(_dataPath))
                {
                    Directory.Delete(_dataPath, true);
                }
                ZipFile.ExtractToDirectory(backupFilePath, _dataPath);
            }
        }
    }
}

