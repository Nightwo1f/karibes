using System;
using System.IO;
using System.IO.Compression;
using Karibes.App.Data.Sqlite;

namespace Karibes.App.Services
{
    public class BackupService
    {
        private readonly string _dataPath;
        private readonly string _backupPath;
        private readonly SqliteConnectionFactory _connectionFactory;

        public BackupService()
        {
            var baseDirectory = AppContext.BaseDirectory;
            _dataPath = Path.Combine(baseDirectory, "Data", "Excel");
            _connectionFactory = new SqliteConnectionFactory();
            _backupPath = Path.Combine(Path.GetDirectoryName(_connectionFactory.DatabasePath)!, "Backups");
            Directory.CreateDirectory(_backupPath);
        }

        public string DatabasePath => _connectionFactory.DatabasePath;
        public string DatabaseDirectory => Path.GetDirectoryName(_connectionFactory.DatabasePath)!;
        public string BackupDirectory => _backupPath;

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

        public string CriarBackupSqlite()
        {
            if (!File.Exists(DatabasePath))
                throw new FileNotFoundException("Banco SQLite ainda não foi criado.", DatabasePath);

            Directory.CreateDirectory(_backupPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFilePath = Path.Combine(_backupPath, $"karibes_{timestamp}.db");
            File.Copy(DatabasePath, backupFilePath, overwrite: false);
            return backupFilePath;
        }

        public string? CriarBackupAutomaticoSqlite()
        {
            if (!File.Exists(DatabasePath))
                return null;

            Directory.CreateDirectory(_backupPath);
            var dailyBackupPath = Path.Combine(_backupPath, $"karibes_{DateTime.Now:yyyyMMdd}.db");
            if (File.Exists(dailyBackupPath))
                return dailyBackupPath;

            File.Copy(DatabasePath, dailyBackupPath, overwrite: false);
            RemoverBackupsAntigos(30);
            return dailyBackupPath;
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

        private void RemoverBackupsAntigos(int diasRetencao)
        {
            var limite = DateTime.Now.AddDays(-diasRetencao);
            foreach (var arquivo in Directory.GetFiles(_backupPath, "karibes_*.db"))
            {
                var info = new FileInfo(arquivo);
                if (info.CreationTime < limite)
                    info.Delete();
            }
        }
    }
}
