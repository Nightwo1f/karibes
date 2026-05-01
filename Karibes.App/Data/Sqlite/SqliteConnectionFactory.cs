using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Karibes.App.Data.Sqlite
{
    public class SqliteConnectionFactory
    {
        private const string DatabaseFileName = "karibes.db";
        private readonly string _databasePath;

        public SqliteConnectionFactory(string? databasePath = null)
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Karibes");
            Directory.CreateDirectory(appDataPath);
            _databasePath = databasePath ?? Path.Combine(appDataPath, DatabaseFileName);
        }

        public string DatabasePath => _databasePath;

        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection($"Data Source={_databasePath}");
        }
    }
}
