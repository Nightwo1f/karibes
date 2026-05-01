using Karibes.App.Data.Sqlite;

namespace Karibes.App.Data.Repositories
{
    public static class RepositoryFactory
    {
        private static readonly SqliteConnectionFactory ConnectionFactory = new();

        static RepositoryFactory()
        {
            new SqliteDatabaseInitializer(ConnectionFactory).Initialize();
            new ExcelToSqliteMigrationService(ConnectionFactory).MigrateIfEmpty();
        }

        public static IClienteRepository CriarClienteRepository()
        {
            return new SqliteClienteRepository(ConnectionFactory);
        }

        public static IProdutoRepository CriarProdutoRepository()
        {
            return new SqliteProdutoRepository(ConnectionFactory);
        }

        public static IFinanceiroRepository CriarFinanceiroRepository()
        {
            return new SqliteFinanceiroRepository(ConnectionFactory);
        }

        public static IVendaRepository CriarVendaRepository()
        {
            return new SqliteVendaRepository(
                ConnectionFactory,
                CriarProdutoRepository(),
                CriarClienteRepository(),
                CriarFinanceiroRepository());
        }
    }
}
