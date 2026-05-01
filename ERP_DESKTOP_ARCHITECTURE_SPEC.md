📐 ERP Desktop WhiteLabel
Especificação Técnica de Arquitetura

Stack: .NET 8 + WPF
Padrão: MVVM
Banco: SQLite Local
Objetivo: ERP Desktop modular e comercializável para pequenas lojas

1️⃣ VISÃO ARQUITETURAL
1.1 Tipo de aplicação

Desktop

Offline-first

Banco embarcado

Single-user (v1)

Multi-empresa via WhiteLabel (compilação)

1.2 Arquitetura em Camadas

Arquitetura baseada em Separation of Concerns e Dependency Inversion Principle.

📦 ERP.UI
    ├── Views
    ├── ViewModels
    ├── Themes
    ├── Resources
    └── App.xaml

📦 ERP.Core
    ├── Entities
    ├── Enums
    ├── DTOs
    ├── Interfaces
    └── Services

📦 ERP.Data
    ├── Context
    ├── Migrations
    ├── Repositories
    └── Configurations

📦 ERP.Infrastructure
    ├── Logging
    ├── Backup
    ├── Helpers
    └── ConfigurationLoader

📦 ERP.Shared
    ├── Constants
    └── Contracts
2️⃣ PADRÃO ARQUITETURAL
2.1 MVVM Estrito

Cada tela terá:

View.xaml

ViewModel.cs

Sem lógica de negócio na View

Sem acesso direto a DbContext na ViewModel

Fluxo:

View → ViewModel → Service → Repository → DbContext
2.2 Injeção de Dependência

Será utilizado:

Microsoft.Extensions.DependencyInjection

Registro no App.xaml.cs.

Exemplo:

services.AddDbContext<AppDbContext>();
services.AddScoped<IProdutoRepository, ProdutoRepository>();
services.AddScoped<IProdutoService, ProdutoService>();
services.AddTransient<DashboardViewModel>();

Nenhuma classe deve instanciar dependências manualmente.

3️⃣ MODELO DE DADOS
3.1 Entidades Principais
Produto
public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public int Estoque { get; set; }
    public DateTime DataCadastro { get; set; }
}
Venda
public class Venda
{
    public Guid Id { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal ValorTotal { get; set; }
    public ICollection<VendaItem> Itens { get; set; }
}
MovimentacaoFinanceira
public class MovimentacaoFinanceira
{
    public Guid Id { get; set; }
    public TipoMovimentacao Tipo { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; }
    public DateTime Data { get; set; }
}
3.2 Relacionamentos

Venda 1:N VendaItem

VendaItem N:1 Produto

Configuração via Fluent API.

4️⃣ CAMADA DATA
4.1 DbContext
public class AppDbContext : DbContext
{
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<MovimentacaoFinanceira> Movimentacoes { get; set; }
}

Banco:

database.db

Localização:

%AppData%/NomeDoApp/

Nunca na pasta do executável.

4.2 Repositories

Interface exemplo:

public interface IProdutoRepository
{
    Task<List<Produto>> GetAllAsync();
    Task AddAsync(Produto produto);
    Task UpdateAsync(Produto produto);
    Task DeleteAsync(Guid id);
}

Implementação concreta no ERP.Data.

5️⃣ CAMADA SERVICE

Regra:

Services contêm regra de negócio

ViewModels apenas orquestram

Exemplo:

public class FinanceiroService
{
    public decimal CalcularLucroMensal(DateTime inicio, DateTime fim)
    {
        ...
    }
}
6️⃣ DASHBOARD EXECUTIVO
6.1 ViewModel Dashboard

Propriedades:

public decimal ReceitaTotal { get; set; }
public decimal DespesaTotal { get; set; }
public decimal LucroLiquido { get; set; }
public decimal SaldoCaixa { get; set; }
public int TotalVendas { get; set; }
public decimal TicketMedio { get; set; }

Todos implementando:

INotifyPropertyChanged
6.2 Atualização Reativa

Ao alterar período:

Recalcular métricas

Atualizar bindings

Sem recarregar janela

7️⃣ TEMA E ESTILO
7.1 Estrutura de Temas
Themes/
    LightTheme.xaml
    DarkTheme.xaml
    DedicatedTheme.xaml

Troca via:

ResourceDictionary.MergedDictionaries

Nenhuma cor hardcoded na UI.

7.2 WhiteLabel

Arquivo:

appsettings.json

Carregado no startup.

Classe:

public class BrandingSettings
{
    public string AppName { get; set; }
    public string CompanyName { get; set; }
    public string PrimaryColor { get; set; }
    public string LogoPath { get; set; }
}

Aplicado dinamicamente aos recursos.

8️⃣ LOGGING
8.1 Estrutura

Pasta:

Logs/

Formato:

yyyy-MM-dd.log

Conteúdo:

[HH:mm:ss] [ERROR] Mensagem

Capturar:

Exceções globais

Erros de banco

Falhas de gravação

9️⃣ BACKUP

Função:

Copiar database.db

Compactar opcionalmente

Nome com timestamp

🔟 PUBLICAÇÃO
10.1 Modo

Framework-dependent

Comando:

dotnet publish -c Release -r win-x64 --self-contained false

Reduz drasticamente tamanho do executável.

11️⃣ PADRÕES DE QUALIDADE
11.1 Convenções

PascalCase para classes

Async sempre que houver IO

Nenhum método com mais de 40 linhas

Sem lógica na code-behind

11.2 Tratamento de Erros

Try/catch em Services

Log obrigatório

Mensagem amigável para usuário

12️⃣ TESTES

Inicialmente:

Testes manuais guiados

Futuro:

Projeto separado ERP.Tests

xUnit

Testes de Service layer

13️⃣ ROADMAP TÉCNICO
Sprint 1

SQLite + Repository + DI

Sprint 2

Service Layer + Dashboard real

Sprint 3

Tema dinâmico + WhiteLabel

Sprint 4

Logs + Backup + Tratamento global

Sprint 5

Polimento visual + validações + release comercial

14️⃣ DEFINIÇÃO DE PRONTO (DoD)

Um módulo só é considerado finalizado se:

Persistência validada

Sem erros no log

UI atualiza corretamente

Sem cores fixas

Sem exceções não tratadas

15️⃣ META FINAL

Sistema:

Modular

Escalável

Comercializável

WhiteLabel-ready

Estável

Profissional
