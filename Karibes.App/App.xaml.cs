using System.Windows;
using Karibes.App.Services;
using Karibes.App.Views;

namespace Karibes.App
{
    public partial class App : Application
    {
        private TemaService? _temaService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // DispatcherUnhandledException - captura exceções não tratadas
            // Flag para evitar loop de exceções
            bool _exceptionHandled = false;
            this.DispatcherUnhandledException += (sender, args) =>
            {
                // Previne loop de exceções
                if (_exceptionHandled)
                {
                    args.Handled = true;
                    return;
                }
                
                _exceptionHandled = true;
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Erro não tratado: {args.Exception.Message}\n{args.Exception.StackTrace}");
                    System.Windows.MessageBox.Show(
                        $"Erro não tratado: {args.Exception.Message}\n\nDetalhes no console de debug.",
                        "Erro Crítico",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    _exceptionHandled = false;
                    args.Handled = true; // Previne fechamento do app
                }
            };
            
            // Carrega tema ANTES de criar a MainWindow para garantir que os recursos estejam disponíveis
            try
            {
                _temaService = new TemaService();
                _temaService.CarregarTemaSalvo();
            }
            catch (System.Exception ex)
            {
                // Se houver erro ao carregar tema, continua com tema padrão
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar tema: {ex.Message}");
                // Não interrompe a execução
            }

            // Gera dados de teste se não existirem
            try
            {
                // var dadosTesteService = new DadosTesteService();
                // dadosTesteService.GerarDadosTeste();
            }
            catch (System.Exception ex)
            {
                // Se houver erro ao gerar dados de teste, continua normalmente
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar dados de teste: {ex.Message}");
                // Não interrompe a execução
            }

            try
            {
                Karibes.App.Data.Repositories.RepositoryFactory.CriarProdutoRepository();
                new BackupService().CriarBackupAutomaticoSqlite();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao criar backup automático do SQLite: {ex.Message}");
            }

            // Cria e exibe a MainWindow
            var window = new MainWindow();
            window.Show();
        }
    }
}
