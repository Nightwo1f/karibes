using System;
using System.IO;

namespace Karibes.App.Data.Sqlite
{
    public class SqliteDatabaseInitializer
    {
        private readonly SqliteConnectionFactory _connectionFactory;

        public SqliteDatabaseInitializer(SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Initialize()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = LoadSchema();
            command.ExecuteNonQuery();

            EnsureClienteColumns(connection);
        }

        private static string LoadSchema()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var schemaPath = FindSchemaPath(baseDirectory);
            return File.ReadAllText(schemaPath);
        }

        private static string FindSchemaPath(string baseDirectory)
        {
            var current = new DirectoryInfo(baseDirectory);
            while (current != null)
            {
                var outputPath = Path.Combine(current.FullName, "Data", "Sqlite", "schema.sql");
                if (File.Exists(outputPath))
                    return outputPath;

                var projectPath = Path.Combine(current.FullName, "Karibes.App", "Data", "Sqlite", "schema.sql");
                if (File.Exists(projectPath))
                    return projectPath;

                current = current.Parent;
            }

            return Path.Combine(baseDirectory, "Data", "Sqlite", "schema.sql");
        }

        private static void EnsureClienteColumns(Microsoft.Data.Sqlite.SqliteConnection connection)
        {
            var columns = new[]
            {
                "CEP",
                "Endereco",
                "Numero",
                "Complemento",
                "Bairro",
                "Cidade",
                "Estado",
                "DataVencimentoCredito"
            };

            foreach (var column in columns)
            {
                if (ClienteColumnExists(connection, column))
                    continue;

                using var command = connection.CreateCommand();
                command.CommandText = column == "DataVencimentoCredito"
                    ? $"ALTER TABLE Clientes ADD COLUMN {column} TEXT NULL"
                    : $"ALTER TABLE Clientes ADD COLUMN {column} TEXT NOT NULL DEFAULT ''";
                command.ExecuteNonQuery();
            }
        }

        private static bool ClienteColumnExists(Microsoft.Data.Sqlite.SqliteConnection connection, string columnName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(Clientes)";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1) == columnName)
                    return true;
            }

            return false;
        }
    }
}
