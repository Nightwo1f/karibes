📘 ERP Desktop WhiteLabel
Roadmap Oficial de Desenvolvimento

Base tecnológica: .NET 8 + WPF (MVVM)
Banco: SQLite Local
Modelo comercial: WhiteLabel para pequenas lojas

1️⃣ VISÃO GERAL DO PRODUTO
🎯 Objetivo

Criar um ERP desktop local para pequenas lojas, com:

Instalação simples

Banco local SQLite

Interface profissional

Dashboard executivo

Estrutura WhiteLabel

Fácil replicação para múltiplos clientes

2️⃣ SITUAÇÃO ATUAL DO PROJETO
✔ O que já temos

Estrutura WPF funcional

Padrão MVVM aplicado

Telas operacionais (financeiro, etc.)

Navegação básica funcionando

Compilação funcional

MVP apresentável

⚠ Problemas atuais

Sem persistência real (ou parcial)

Sem camada Repository estruturada

Sem separação clara entre domínio e dados

Falta de logs

WhiteLabel inexistente

Dashboard ainda não executivo

Tema não 100% dinâmico

Executável grande demais

3️⃣ ARQUITETURA DEFINITIVA DO PRODUTO

Estrutura final desejada:

📁 ERP.Core        → Regras de negócio
📁 ERP.Data        → SQLite + Repositories
📁 ERP.UI          → WPF (MVVM)
📁 ERP.Shared      → Configuração WhiteLabel
📁 ERP.Infrastructure → Logs, utilidades
4️⃣ FASES DE DESENVOLVIMENTO
🔹 FASE 1 — Fundação Comercial
Objetivo:

Transformar o MVP em base sólida.

4.1 Implementar SQLite
Por que?

Persistência real

Backup simples

Ideal para pequenas lojas

Zero necessidade de servidor

O que será feito:

Instalar pacote:

Microsoft.EntityFrameworkCore.Sqlite

Criar DbContext

Criar arquivo database.db

Criar migrations

Criar estrutura inicial das tabelas:

Produtos

Vendas

MovimentacoesFinanceiras

4.2 Criar Camada Repository
Por que?

Separar UI de acesso a dados.

O que será feito:

Criar interfaces:

IProdutoRepository
IVendaRepository
IFinanceiroRepository

Implementações concretas:

ProdutoRepository
VendaRepository
FinanceiroRepository

ViewModels não acessam mais o DbContext direto.

4.3 Sistema de Logs
Por que?

Produto profissional precisa rastreabilidade.

O que será feito:

Criar pasta /Logs

Criar classe Logger

Registrar:

Erros

Exceções

Operações críticas

Formato:

[DATA] [TIPO] Mensagem
4.4 Configuração WhiteLabel
Por que?

Escalabilidade comercial.

O que será feito:

Criar:

appsettings.json

Estrutura:

{
  "AppName": "ERP Comercial",
  "CompanyName": "Empresa Modelo",
  "PrimaryColor": "#1E3A8A",
  "SecondaryColor": "#2563EB",
  "LogoPath": "Assets/logo.png"
}

Criar classe:

AppConfiguration

Carregar no startup.

Toda UI usará esses valores dinamicamente.

🔹 FASE 2 — Dashboard Executivo
Objetivo:

Transformar dados em visão gerencial.

5.1 Indicadores principais

Receita total do mês

Despesa total

Lucro líquido

Saldo atual do caixa

Número de vendas

Ticket médio

5.2 Filtros por período

Hoje

Semana

Mês

Personalizado

5.3 Indicadores visuais

Verde para lucro positivo

Vermelho para prejuízo

Cards padronizados

Layout limpo

🔹 FASE 3 — Padronização Visual Profissional
Objetivo:

Tornar o sistema comercialmente apresentável.

6.1 Padronização de cores

Remover cores hardcoded

Usar DynamicResource

Centralizar paleta

6.2 Dark Mode real

Troca de ResourceDictionary

Atualização automática

Sem elementos quebrando contraste

6.3 Padronização de espaçamento

Margens consistentes

Botões alinhados

Tipografia uniforme

🔹 FASE 4 — Robustez Operacional
7.1 Validações

Campos obrigatórios

Valores negativos inválidos

Confirmação de exclusão

7.2 Backup manual

Botão:

"Gerar Backup"

Copia o arquivo .db para pasta escolhida.

7.3 Tratamento global de exceções

Capturar erros não tratados no App.xaml.cs

Registrar no log.

🔹 FASE 5 — Preparação Comercial
8.1 Otimização do Executável

Alterar publicação para:

Framework-dependent

Reduz tamanho drasticamente.

8.2 Personalização por cliente

Checklist para nova empresa:

Alterar AppName

Alterar CompanyName

Trocar Logo

Ajustar cores

Compilar Release

8.3 Tela de Splash

Exibir:

Logo da empresa
Nome do sistema
Versão

5️⃣ PADRÃO DE TRABALHO

Cada fase seguirá:

Planejamento

Implementação técnica

Testes manuais

Validação de UI

Commit organizado

6️⃣ CONTROLE DE VERSÕES

Sugestão de versões:

v0.9 → MVP atual

v1.0 → SQLite + Repository

v1.1 → Dashboard Executivo

v1.2 → Padronização visual

v1.3 → WhiteLabel completo

v1.5 → Comercial estável

7️⃣ META FINAL

Criar um produto que:

Funcione offline

Não quebre

Seja rápido

Seja bonito

Seja replicável

Seja vendável

8️⃣ STATUS ATUAL

Estamos entre:

MVP funcional
Início da Fase 1

9️⃣ PRÓXIMO PASSO REAL

Iniciar:

FASE 1
Implementação do SQLite + Camada Repository