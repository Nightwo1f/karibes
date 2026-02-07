using System;
using System.Windows;
using System.Linq;
using System.IO;
using System.Text.Json;
using Karibes.App.Utils;

namespace Karibes.App.Services
{
    /// <summary>
    /// Serviço para gerenciamento de temas da aplicação
    /// </summary>
    public class TemaService
    {
        private const string ConfigFileName = "tema_config.json";
        private string _configPath;

        public TemaService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Karibes");
            Directory.CreateDirectory(appDataPath);
            _configPath = Path.Combine(appDataPath, ConfigFileName);
        }

        /// <summary>
        /// Aplica um tema na aplicação
        /// </summary>
        public void AplicarTema(string tema)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                var resources = app.Resources;
                var mergedDictionaries = resources.MergedDictionaries;

                // Remove tema existente
                var themeToRemove = mergedDictionaries.FirstOrDefault(d =>
                    d.Source?.OriginalString?.Contains("Theme.xaml") == true ||
                    d.Source?.OriginalString?.Contains("LightTheme") == true ||
                    d.Source?.OriginalString?.Contains("DarkTheme") == true ||
                    d.Source?.OriginalString?.Contains("KaribesTheme") == true);

                if (themeToRemove != null)
                    mergedDictionaries.Remove(themeToRemove);

                // Adiciona novo tema
                ResourceDictionary? themeDict = tema switch
                {
                    Constants.TemaLight => new ResourceDictionary 
                    { 
                        Source = new Uri("/Karibes;component/Themes/LightTheme.xaml", UriKind.Relative) 
                    },
                    Constants.TemaDark => new ResourceDictionary 
                    { 
                        Source = new Uri("/Karibes;component/Themes/DarkTheme.xaml", UriKind.Relative) 
                    },
                    Constants.TemaKaribes => new ResourceDictionary 
                    { 
                        Source = new Uri("/Karibes;component/Themes/KaribesTheme.xaml", UriKind.Relative) 
                    },
                    _ => new ResourceDictionary 
                    { 
                        Source = new Uri("/Karibes;component/Themes/KaribesTheme.xaml", UriKind.Relative) 
                    }
                };

                mergedDictionaries.Add(themeDict);
                
                // Salva preferência
                SalvarPreferenciaTema(tema);
            }
            catch (System.Exception ex)
            {
                // Log do erro mas não interrompe a aplicação
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar tema '{tema}': {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém o tema atual salvo
        /// </summary>
        public string ObterTemaAtual()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<TemaConfig>(json);
                    return config?.Tema ?? Constants.TemaKaribes;
                }
            }
            catch
            {
                // Se houver erro, retorna tema padrão
            }

            return Constants.TemaKaribes;
        }

        /// <summary>
        /// Carrega o tema salvo na inicialização
        /// </summary>
        public void CarregarTemaSalvo()
        {
            var tema = ObterTemaAtual();
            AplicarTema(tema);
        }

        /// <summary>
        /// Salva a preferência de tema
        /// </summary>
        private void SalvarPreferenciaTema(string tema)
        {
            try
            {
                var config = new TemaConfig { Tema = tema };
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_configPath, json);
            }
            catch
            {
                // Falha silenciosa - não é crítico
            }
        }

        /// <summary>
        /// Classe para configuração de tema
        /// </summary>
        private class TemaConfig
        {
            public string Tema { get; set; } = Constants.TemaKaribes;
        }
    }
}

