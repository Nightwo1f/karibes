# KARIBES - Sistema de Gestão Comercial
## Documentação Técnica e Requisitos

---

## 1. LEVANTAMENTO DE REQUISITOS FUNCIONAIS

### RF-001: Gestão de Produtos
- **RF-001.1**: Cadastrar produto com código, nome, descrição, preço, custo, estoque inicial, estoque mínimo, unidade de medida
- **RF-001.2**: Editar informações de produto existente
- **RF-001.3**: Excluir produto (soft delete - marca como inativo)
- **RF-001.4**: Consultar produtos por código, nome ou categoria
- **RF-001.5**: Visualizar estoque atual e histórico de movimentações
- **RF-001.6**: Alertar quando estoque estiver abaixo do mínimo
- **RF-001.7**: Registrar entrada e saída de estoque manualmente

### RF-002: Gestão de Clientes
- **RF-002.1**: Cadastrar cliente com dados pessoais completos (nome, CPF/CNPJ, contatos, endereço)
- **RF-002.2**: Editar dados do cliente
- **RF-002.3**: Excluir cliente (soft delete)
- **RF-002.4**: Consultar cliente por nome, documento ou código
- **RF-002.5**: Visualizar histórico completo de compras do cliente
- **RF-002.6**: Definir limite de crédito (fiado) por cliente
- **RF-002.7**: Consultar saldo devedor do cliente
- **RF-002.8**: Registrar pagamento de crédito do cliente

### RF-003: Gestão de Vendas
- **RF-003.1**: Criar nova venda (carrinho de compras)
- **RF-003.2**: Adicionar produtos ao carrinho com quantidade
- **RF-003.3**: Aplicar desconto por item ou na venda total
- **RF-003.4**: Associar cliente à venda (opcional)
- **RF-003.5**: Selecionar forma de pagamento (Dinheiro, Cartão, Crédito/Fiado)
- **RF-003.6**: Finalizar venda (impacta estoque e financeiro automaticamente)
- **RF-003.7**: Cancelar venda (estorno de estoque e financeiro)
- **RF-003.8**: Consultar vendas por período, cliente ou produto
- **RF-003.9**: Visualizar detalhes completos da venda
- **RF-003.10**: Imprimir cupom/nota da venda

### RF-004: Gestão Financeira
- **RF-004.1**: Registrar entrada financeira (venda, recebimento de crédito, outros)
- **RF-004.2**: Registrar saída financeira (despesa fixa, variável, pagamento)
- **RF-004.3**: Categorizar lançamentos (Vendas, Despesas, Salários, Impostos, etc.)
- **RF-004.4**: Consultar fluxo de caixa por período
- **RF-004.5**: Calcular lucro bruto (receitas - custos de produtos vendidos)
- **RF-004.6**: Calcular lucro líquido (receitas - todas as despesas)
- **RF-004.7**: Visualizar saldo atual do caixa
- **RF-004.8**: Gerar relatório financeiro mensal

### RF-005: Dashboard
- **RF-005.1**: Exibir vendas do dia (quantidade e valor)
- **RF-005.2**: Exibir vendas do mês atual
- **RF-005.3**: Exibir produtos com estoque crítico (abaixo do mínimo)
- **RF-005.4**: Exibir lucro estimado do mês
- **RF-005.5**: Exibir top 5 produtos mais vendidos do mês
- **RF-005.6**: Exibir gráfico de vendas dos últimos 7 dias
- **RF-005.7**: Exibir saldo atual do caixa

### RF-006: Relatórios
- **RF-006.1**: Gerar balanço mensal automático (receitas, despesas, lucro)
- **RF-006.2**: Relatório de produtos mais vendidos (por período)
- **RF-006.3**: Relatório de clientes que mais compraram (por período)
- **RF-006.4**: Relatório de estoque (todos os produtos com quantidades)
- **RF-006.5**: Relatório de movimentação financeira (por período)
- **RF-006.6**: Exportar relatórios para PDF ou Excel
- **RF-006.7**: Imprimir qualquer relatório

### RF-007: Sistema de Temas
- **RF-007.1**: Alternar entre tema claro, escuro e Karibes
- **RF-007.2**: Aplicar tema em tempo real (sem reiniciar aplicação)
- **RF-007.3**: Salvar preferência de tema do usuário

### RF-008: Backup e Segurança
- **RF-008.1**: Criar backup automático diário dos arquivos Excel
- **RF-008.2**: Criar backup manual sob demanda
- **RF-008.3**: Restaurar backup selecionado
- **RF-008.4**: Manter histórico de backups (últimos 30 dias)
- **RF-008.5**: Validar integridade dos dados antes de operações críticas

---

## 2. LEVANTAMENTO DE REQUISITOS NÃO FUNCIONAIS

### RNF-001: Performance
- **RNF-001.1**: Aplicação deve iniciar em menos de 3 segundos
- **RNF-001.2**: Operações de leitura/escrita em Excel devem ser otimizadas (máximo 2s)
- **RNF-001.3**: Interface deve permanecer responsiva durante operações
- **RNF-001.4**: Suportar até 10.000 produtos sem degradação de performance
- **RNF-001.5**: Suportar até 5.000 clientes sem degradação de performance

### RNF-002: Confiabilidade
- **RNF-002.1**: Sistema deve validar dados antes de salvar
- **RNF-002.2**: Implementar tratamento de erros robusto
- **RNF-002.3**: Prevenir perda de dados em caso de falha
- **RNF-002.4**: Validar integridade dos arquivos Excel antes de operações
- **RNF-002.5**: Sistema deve funcionar mesmo se um arquivo Excel estiver corrompido (isolar erro)

### RNF-003: Usabilidade
- **RNF-003.1**: Interface intuitiva e autoexplicativa
- **RNF-003.2**: Mensagens de erro claras e objetivas
- **RNF-003.3**: Confirmação para operações destrutivas (exclusão)
- **RNF-003.4**: Atalhos de teclado para operações frequentes
- **RNF-003.5**: Busca rápida em todas as listagens

### RNF-004: Manutenibilidade
- **RNF-004.1**: Código organizado seguindo padrão MVVM
- **RNF-004.2**: Código comentado e documentado
- **RNF-004.3**: Separação clara de responsabilidades
- **RNF-004.4**: Preparado para migração futura para SQL Server
- **RNF-004.5**: Uso de interfaces para desacoplamento

### RNF-005: Portabilidade
- **RNF-005.1**: Funcionar em Windows 10 e superiores
- **RNF-005.2**: Não depender de instalação de Excel no sistema
- **RNF-005.3**: Arquivos Excel devem ser legíveis por qualquer leitor de planilhas

### RNF-006: Segurança
- **RNF-006.1**: Validação de dados de entrada
- **RNF-006.2**: Prevenção de SQL Injection (não aplicável, mas princípio)
- **RNF-006.3**: Validação de tipos de arquivo em importações
- **RNF-006.4**: Logs de operações críticas

---

## 3. DEFINIÇÃO DOS MÓDULOS DO SISTEMA

### Módulo 1: Dashboard
**Responsabilidade**: Apresentar visão consolidada do negócio
**Componentes**:
- View: DashboardView.xaml
- ViewModel: DashboardViewModel.cs
- Service: DashboardService.cs (agrega dados de outros serviços)

### Módulo 2: Produtos
**Responsabilidade**: Gestão completa do catálogo de produtos
**Componentes**:
- View: ProdutosView.xaml
- ViewModel: ProdutosViewModel.cs
- Service: ProdutoService.cs
- Model: Produto.cs

### Módulo 3: Clientes
**Responsabilidade**: Gestão de cadastro e relacionamento com clientes
**Componentes**:
- View: ClientesView.xaml
- ViewModel: ClientesViewModel.cs
- Service: ClienteService.cs
- Model: Cliente.cs

### Módulo 4: Vendas
**Responsabilidade**: Processamento de vendas e carrinho
**Componentes**:
- View: VendasView.xaml
- ViewModel: VendasViewModel.cs
- Service: VendaService.cs
- Models: Venda.cs, ItemVenda.cs

### Módulo 5: Financeiro
**Responsabilidade**: Controle financeiro e fluxo de caixa
**Componentes**:
- View: FinanceiroView.xaml
- ViewModel: FinanceiroViewModel.cs
- Service: FinanceiroService.cs
- Model: LancamentoFinanceiro.cs

### Módulo 6: Relatórios
**Responsabilidade**: Geração e visualização de relatórios
**Componentes**:
- View: RelatoriosView.xaml
- ViewModel: RelatoriosViewModel.cs
- Service: RelatorioService.cs
- Model: BalancoMensal.cs

### Módulo 7: Estoque
**Responsabilidade**: Controle de movimentações de estoque
**Componentes**:
- Service: EstoqueService.cs
- Model: MovimentoEstoque.cs
- Integrado aos módulos de Produtos e Vendas

### Módulo 8: Sistema
**Responsabilidade**: Configurações e utilitários do sistema
**Componentes**:
- Service: TemaService.cs
- Service: BackupService.cs
- Service: ImpressaoService.cs
- Service: ExcelService.cs (base para todos)

---

## 4. ESTRUTURA DETALHADA DAS PLANILHAS EXCEL

### Planilha: produtos.xlsx
**Sheet: Produtos**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único (auto-incremento) |
| Codigo | String(50) | Sim | Código do produto (único) |
| Nome | String(200) | Sim | Nome do produto |
| Descricao | String(500) | Não | Descrição detalhada |
| Categoria | String(100) | Não | Categoria do produto |
| PrecoVenda | Decimal | Sim | Preço de venda |
| PrecoCusto | Decimal | Sim | Preço de custo |
| EstoqueAtual | Int | Sim | Quantidade em estoque |
| EstoqueMinimo | Int | Sim | Estoque mínimo (alerta) |
| Unidade | String(10) | Sim | Unidade de medida (UN, KG, etc) |
| DataCadastro | DateTime | Sim | Data de cadastro |
| DataUltimaAtualizacao | DateTime | Sim | Última atualização |
| Ativo | Boolean | Sim | Status (ativo/inativo) |
| Observacoes | String(1000) | Não | Observações gerais |

### Planilha: clientes.xlsx
**Sheet: Clientes**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único |
| Codigo | String(50) | Sim | Código do cliente (único) |
| Nome | String(200) | Sim | Nome completo |
| TipoDocumento | String(10) | Sim | CPF ou CNPJ |
| Documento | String(20) | Sim | CPF/CNPJ (sem formatação) |
| Email | String(200) | Não | E-mail |
| Telefone | String(20) | Não | Telefone |
| Celular | String(20) | Não | Celular |
| CEP | String(10) | Não | CEP |
| Endereco | String(200) | Não | Endereço |
| Numero | String(20) | Não | Número |
| Complemento | String(100) | Não | Complemento |
| Bairro | String(100) | Não | Bairro |
| Cidade | String(100) | Não | Cidade |
| Estado | String(2) | Não | Estado (UF) |
| LimiteCredito | Decimal | Sim | Limite de crédito (fiado) |
| SaldoDevedor | Decimal | Sim | Saldo atual devedor |
| DataCadastro | DateTime | Sim | Data de cadastro |
| DataUltimaAtualizacao | DateTime | Sim | Última atualização |
| Ativo | Boolean | Sim | Status (ativo/inativo) |
| Observacoes | String(1000) | Não | Observações |

### Planilha: vendas.xlsx
**Sheet: Vendas**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único |
| NumeroVenda | String(50) | Sim | Número da venda (único) |
| ClienteId | Int | Não | ID do cliente (null se venda avulsa) |
| DataVenda | DateTime | Sim | Data e hora da venda |
| ValorSubtotal | Decimal | Sim | Subtotal (sem desconto) |
| ValorDesconto | Decimal | Sim | Valor do desconto |
| ValorTotal | Decimal | Sim | Valor total da venda |
| FormaPagamento | String(50) | Sim | Dinheiro, Cartão, Crédito |
| Status | String(20) | Sim | Finalizada, Cancelada |
| Vendedor | String(100) | Não | Nome do vendedor |
| Observacoes | String(500) | Não | Observações da venda |

**Sheet: ItensVenda**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único |
| VendaId | Int | Sim | ID da venda |
| ProdutoId | Int | Sim | ID do produto |
| Quantidade | Int | Sim | Quantidade vendida |
| PrecoUnitario | Decimal | Sim | Preço unitário no momento da venda |
| Desconto | Decimal | Sim | Desconto aplicado no item |
| ValorTotal | Decimal | Sim | Valor total do item |

### Planilha: estoque_movimentacoes.xlsx
**Sheet: Movimentacoes**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único |
| ProdutoId | Int | Sim | ID do produto |
| TipoMovimento | String(20) | Sim | Entrada, Saída, Ajuste |
| Quantidade | Int | Sim | Quantidade (positiva para entrada, negativa para saída) |
| EstoqueAnterior | Int | Sim | Estoque antes da movimentação |
| EstoqueAtual | Int | Sim | Estoque após movimentação |
| DataMovimento | DateTime | Sim | Data e hora da movimentação |
| Origem | String(50) | Sim | Venda, Compra, Ajuste Manual, etc |
| OrigemId | Int | Não | ID da origem (ex: VendaId) |
| Motivo | String(200) | Não | Motivo da movimentação |
| Observacoes | String(500) | Não | Observações |

### Planilha: financeiro.xlsx
**Sheet: Lancamentos**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único |
| Tipo | String(20) | Sim | Receita ou Despesa |
| Categoria | String(100) | Sim | Categoria do lançamento |
| Descricao | String(500) | Sim | Descrição do lançamento |
| Valor | Decimal | Sim | Valor do lançamento |
| DataLancamento | DateTime | Sim | Data do lançamento |
| DataVencimento | DateTime | Não | Data de vencimento |
| DataPagamento | DateTime | Não | Data de pagamento |
| Status | String(20) | Sim | Pendente, Pago, Cancelado |
| FormaPagamento | String(50) | Não | Forma de pagamento |
| Origem | String(50) | Não | Venda, Manual, etc |
| OrigemId | Int | Não | ID da origem |
| Observacoes | String(500) | Não | Observações |

### Planilha: balancos.xlsx
**Sheet: Balancos**
| Coluna | Tipo | Obrigatório | Descrição |
|--------|------|-------------|-----------|
| Id | Int | Sim | Identificador único |
| Ano | Int | Sim | Ano do balanço |
| Mes | Int | Sim | Mês do balanço (1-12) |
| ReceitaTotal | Decimal | Sim | Total de receitas |
| CustoProdutosVendidos | Decimal | Sim | Custo dos produtos vendidos |
| DespesaTotal | Decimal | Sim | Total de despesas |
| LucroBruto | Decimal | Sim | Receita - Custo Produtos |
| LucroLiquido | Decimal | Sim | Receita - Despesas Totais |
| TotalVendas | Int | Sim | Quantidade de vendas |
| DataGeracao | DateTime | Sim | Data de geração do balanço |

---

## 5. REGRAS DE NEGÓCIO

### RN-001: Produtos
- **RN-001.1**: Código do produto deve ser único no sistema
- **RN-001.2**: Estoque não pode ficar negativo
- **RN-001.3**: Ao cadastrar produto, estoque inicial deve ser >= 0
- **RN-001.4**: Produto inativo não pode ser vendido
- **RN-001.5**: Preço de venda deve ser >= preço de custo (recomendado, não obrigatório)

### RN-002: Clientes
- **RN-002.1**: Documento (CPF/CNPJ) deve ser único e válido
- **RN-002.2**: Limite de crédito deve ser >= 0
- **RN-002.3**: Saldo devedor não pode ultrapassar limite de crédito
- **RN-002.4**: Cliente inativo não pode realizar novas compras a crédito
- **RN-002.5**: Ao registrar pagamento, saldo devedor não pode ficar negativo

### RN-003: Vendas
- **RN-003.1**: Venda deve ter pelo menos 1 item
- **RN-003.2**: Quantidade vendida não pode ser <= 0
- **RN-003.3**: Estoque deve ser suficiente antes de finalizar venda
- **RN-003.4**: Venda a crédito só é permitida se cliente tiver limite disponível
- **RN-003.5**: Ao finalizar venda:
  - Estoque é reduzido automaticamente
  - Lançamento financeiro é criado (se à vista ou cartão)
  - Se a crédito, saldo devedor do cliente é atualizado
- **RN-003.6**: Venda cancelada deve estornar estoque e lançamento financeiro
- **RN-003.7**: Desconto não pode ser maior que o valor total da venda

### RN-004: Estoque
- **RN-004.1**: Toda movimentação de estoque deve ser registrada
- **RN-004.2**: Movimentação de saída só é permitida se estoque for suficiente
- **RN-004.3**: Movimentações não podem ser excluídas, apenas ajustadas
- **RN-004.4**: Estoque atual = soma de todas as movimentações

### RN-005: Financeiro
- **RN-005.1**: Toda venda à vista/cartão gera lançamento de receita automaticamente
- **RN-005.2**: Venda a crédito gera receita apenas quando cliente pagar
- **RN-005.3**: Despesas devem ter categoria definida
- **RN-005.4**: Lançamentos não podem ser excluídos, apenas cancelados
- **RN-005.5**: Saldo do caixa = soma de todas as receitas - todas as despesas (pagos)

### RN-006: Integridade de Dados
- **RN-006.1**: Operações críticas devem ser transacionais (tudo ou nada)
- **RN-006.2**: Validação de dados antes de salvar
- **RN-006.3**: Backup automático antes de operações críticas
- **RN-006.4**: Logs de todas as operações importantes

---

## 6. CASOS DE USO PRINCIPAIS

### CU-001: Realizar Venda à Vista
**Ator**: Vendedor
**Pré-condições**: Produtos cadastrados, estoque disponível
**Fluxo Principal**:
1. Vendedor acessa módulo de Vendas
2. Sistema exibe tela de nova venda
3. Vendedor adiciona produtos ao carrinho
4. Sistema valida estoque
5. Vendedor seleciona forma de pagamento (Dinheiro/Cartão)
6. Vendedor finaliza venda
7. Sistema reduz estoque
8. Sistema cria lançamento financeiro de receita
9. Sistema gera movimentação de estoque
10. Sistema exibe confirmação e opção de impressão

**Fluxo Alternativo 3a**: Estoque insuficiente
- Sistema exibe mensagem de erro
- Vendedor ajusta quantidade ou remove item

### CU-002: Realizar Venda a Crédito
**Ator**: Vendedor
**Pré-condições**: Cliente cadastrado com limite de crédito, produtos em estoque
**Fluxo Principal**:
1. Vendedor acessa módulo de Vendas
2. Vendedor seleciona cliente
3. Sistema valida limite de crédito disponível
4. Vendedor adiciona produtos ao carrinho
5. Sistema valida estoque e limite de crédito
6. Vendedor finaliza venda a crédito
7. Sistema reduz estoque
8. Sistema atualiza saldo devedor do cliente
9. Sistema gera movimentação de estoque
10. Sistema exibe confirmação

**Fluxo Alternativo 3a**: Limite de crédito insuficiente
- Sistema exibe mensagem de erro
- Vendedor informa cliente ou ajusta venda

### CU-003: Cadastrar Produto
**Ator**: Administrador
**Pré-condições**: Acesso ao módulo de Produtos
**Fluxo Principal**:
1. Administrador acessa módulo de Produtos
2. Administrador clica em "Novo Produto"
3. Sistema exibe formulário de cadastro
4. Administrador preenche dados obrigatórios
5. Sistema valida código único
6. Administrador salva produto
7. Sistema cria registro na planilha produtos.xlsx
8. Sistema exibe confirmação

### CU-004: Receber Pagamento de Crédito
**Ator**: Vendedor/Caixa
**Pré-condições**: Cliente com saldo devedor
**Fluxo Principal**:
1. Usuário acessa módulo de Clientes
2. Usuário localiza cliente
3. Usuário visualiza saldo devedor
4. Usuário clica em "Receber Pagamento"
5. Sistema exibe formulário de pagamento
6. Usuário informa valor e forma de pagamento
7. Sistema valida valor (não pode ser maior que saldo)
8. Sistema atualiza saldo devedor do cliente
9. Sistema cria lançamento financeiro de receita
10. Sistema exibe confirmação

### CU-005: Gerar Relatório Mensal
**Ator**: Administrador
**Pré-condições**: Dados do mês existentes
**Fluxo Principal**:
1. Administrador acessa módulo de Relatórios
2. Administrador seleciona mês e ano
3. Administrador clica em "Gerar Balanço Mensal"
4. Sistema calcula receitas do período
5. Sistema calcula custos dos produtos vendidos
6. Sistema calcula despesas do período
7. Sistema calcula lucro bruto e líquido
8. Sistema salva balanço na planilha balancos.xlsx
9. Sistema exibe relatório formatado
10. Administrador pode imprimir ou exportar

---

## 7. FLUXO DE DADOS ENTRE MÓDULOS

```
┌─────────────┐
│  Dashboard  │
└──────┬──────┘
       │ Lê dados agregados
       ▼
┌─────────────────────────────────────────┐
│         ExcelService (Base)              │
│  - produtos.xlsx                        │
│  - clientes.xlsx                        │
│  - vendas.xlsx                          │
│  - estoque_movimentacoes.xlsx           │
│  - financeiro.xlsx                      │
│  - balancos.xlsx                        │
└─────────────────────────────────────────┘
       ▲
       │
┌──────┴──────┐
│   Vendas    │───┐
└─────────────┘   │
                  │ Impacta
┌─────────────┐   │
│  Produtos   │───┤
└─────────────┘   │
                  │
┌─────────────┐   │
│  Clientes   │───┤
└─────────────┘   │
                  │
┌─────────────┐   │
│  Estoque    │◄──┘
└─────────────┘
       │
       │ Atualiza
       ▼
┌─────────────┐
│ Financeiro  │
└─────────────┘
```

**Fluxo de uma Venda**:
1. **Vendas** → Lê produtos.xlsx (valida estoque)
2. **Vendas** → Lê clientes.xlsx (valida crédito, se aplicável)
3. **Vendas** → Salva vendas.xlsx e itens_venda.xlsx
4. **Vendas** → Chama **EstoqueService** → Atualiza produtos.xlsx (estoque) e estoque_movimentacoes.xlsx
5. **Vendas** → Chama **FinanceiroService** → Atualiza financeiro.xlsx (se à vista/cartão) ou clientes.xlsx (se crédito)
6. **Dashboard** → Lê todas as planilhas para exibir indicadores

**Fluxo de Recebimento de Crédito**:
1. **Clientes** → Lê clientes.xlsx (saldo devedor)
2. **Clientes** → Atualiza clientes.xlsx (reduz saldo devedor)
3. **Clientes** → Chama **FinanceiroService** → Cria lançamento em financeiro.xlsx

**Fluxo de Relatório Mensal**:
1. **Relatórios** → Lê financeiro.xlsx (receitas e despesas do mês)
2. **Relatórios** → Lê vendas.xlsx e produtos.xlsx (calcula custo dos produtos vendidos)
3. **Relatórios** → Calcula indicadores
4. **Relatórios** → Salva balancos.xlsx

---

## 8. ESTRATÉGIA DE BACKUP AUTOMÁTICO

### Estratégia Implementada:
1. **Backup Diário Automático**:
   - Executado ao fechar a aplicação
   - Cria arquivo ZIP com timestamp: `backup_YYYYMMDD_HHMMSS.zip`
   - Contém todos os arquivos Excel da pasta Data/Excel
   - Salvo em pasta `Backups/` na raiz do aplicativo

2. **Backup Manual**:
   - Usuário pode acionar backup a qualquer momento
   - Menu: Sistema → Backup Manual

3. **Retenção de Backups**:
   - Manter últimos 30 dias de backups automáticos
   - Backups manuais são mantidos indefinidamente
   - Limpeza automática de backups antigos (executada no início da aplicação)

4. **Backup Antes de Operações Críticas**:
   - Backup automático antes de:
     - Finalizar venda
     - Cancelar venda
     - Importar dados em massa
     - Restaurar backup

5. **Validação de Integridade**:
   - Antes de restaurar, validar se arquivo ZIP não está corrompido
   - Validar se todos os arquivos Excel necessários estão presentes
   - Criar backup atual antes de restaurar

6. **Estrutura de Backup**:
```
Backups/
├── backup_20240115_120000.zip
├── backup_20240116_120000.zip
├── backup_manual_20240117_150000.zip
└── ...
```

---

## 9. ESTRATÉGIA DE IMPRESSÃO

### Tipos de Impressão:

1. **Cupom/Nota de Venda**:
   - Layout simplificado (80mm ou A4)
   - Cabeçalho com dados da loja
   - Itens da venda (produto, quantidade, valor)
   - Totais e forma de pagamento
   - Rodapé com data/hora

2. **Relatórios**:
   - Formato A4
   - Cabeçalho com título e data
   - Tabelas formatadas
   - Gráficos (quando aplicável)
   - Rodapé com informações do sistema

3. **Listagens** (Produtos, Clientes, etc):
   - Formato A4
   - Tabela com todas as colunas relevantes
   - Paginação automática
   - Cabeçalho e rodapé

### Implementação:
- **ImpressaoService**: Classe centralizada para impressão
- Usa `PrintDialog` do WPF para seleção de impressora
- Gera `FlowDocument` para formatação
- Suporta visualização prévia antes de imprimir
- Opção de salvar como PDF (futuro)

### Fluxo de Impressão:
1. Usuário solicita impressão
2. Sistema gera documento formatado (FlowDocument)
3. Sistema exibe preview (opcional)
4. Usuário confirma impressão
5. Sistema envia para impressora selecionada

---

## 10. ESTRUTURA DE PASTAS DO PROJETO

```
Karibes/
├── Karibes.App/
│   ├── App.xaml
│   ├── App.xaml.cs
│   │
│   ├── Resources/
│   │   ├── Colors.xaml
│   │   ├── Styles.xaml
│   │   └── Icons.xaml
│   │
│   ├── Themes/
│   │   ├── LightTheme.xaml
│   │   ├── DarkTheme.xaml
│   │   └── KaribesTheme.xaml
│   │
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── DashboardView.xaml
│   │   ├── DashboardView.xaml.cs
│   │   ├── ProdutosView.xaml
│   │   ├── ProdutosView.xaml.cs
│   │   ├── ClientesView.xaml
│   │   ├── ClientesView.xaml.cs
│   │   ├── VendasView.xaml
│   │   ├── VendasView.xaml.cs
│   │   ├── FinanceiroView.xaml
│   │   ├── FinanceiroView.xaml.cs
│   │   ├── RelatoriosView.xaml
│   │   └── RelatoriosView.xaml.cs
│   │
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs
│   │   ├── MainViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── ProdutosViewModel.cs
│   │   ├── ClientesViewModel.cs
│   │   ├── VendasViewModel.cs
│   │   ├── FinanceiroViewModel.cs
│   │   └── RelatoriosViewModel.cs
│   │
│   ├── Models/
│   │   ├── Produto.cs
│   │   ├── Cliente.cs
│   │   ├── Venda.cs
│   │   ├── ItemVenda.cs
│   │   ├── MovimentoEstoque.cs
│   │   ├── LancamentoFinanceiro.cs
│   │   └── BalancoMensal.cs
│   │
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IExcelService.cs
│   │   │   ├── IProdutoService.cs
│   │   │   ├── IClienteService.cs
│   │   │   ├── IVendaService.cs
│   │   │   ├── IEstoqueService.cs
│   │   │   ├── IFinanceiroService.cs
│   │   │   └── IRelatorioService.cs
│   │   │
│   │   ├── ExcelService.cs
│   │   ├── ProdutoService.cs
│   │   ├── ClienteService.cs
│   │   ├── VendaService.cs
│   │   ├── EstoqueService.cs
│   │   ├── FinanceiroService.cs
│   │   ├── RelatorioService.cs
│   │   ├── DashboardService.cs
│   │   ├── TemaService.cs
│   │   ├── BackupService.cs
│   │   └── ImpressaoService.cs
│   │
│   ├── Data/
│   │   └── Excel/
│   │       ├── produtos.xlsx
│   │       ├── clientes.xlsx
│   │       ├── vendas.xlsx
│   │       ├── estoque_movimentacoes.xlsx
│   │       ├── financeiro.xlsx
│   │       └── balancos.xlsx
│   │
│   ├── Utils/
│   │   ├── RelayCommand.cs
│   │   ├── Validators.cs
│   │   ├── Helpers.cs
│   │   └── Constants.cs
│   │
│   └── Converters/
│       ├── BooleanToVisibilityConverter.cs
│       ├── DecimalToCurrencyConverter.cs
│       └── DateTimeToStringConverter.cs
│
├── Backups/
│   └── (backups automáticos e manuais)
│
└── Karibes.sln
```

---

## 11. DIAGRAMA LÓGICO DO SISTEMA

### Descrição Textual:

```
┌─────────────────────────────────────────────────────────────┐
│                      CAMADA DE APRESENTAÇÃO                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │Dashboard │  │ Produtos │  │ Clientes │  │  Vendas  │  │
│  │   View   │  │   View   │  │   View   │  │   View   │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
│       │             │             │             │         │
│  ┌────▼─────┐  ┌────▼─────┐  ┌────▼─────┐  ┌────▼─────┐  │
│  │Dashboard │  │ Produtos │  │ Clientes │  │  Vendas  │  │
│  │ViewModel │  │ViewModel │  │ViewModel │  │ViewModel │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
└───────┼─────────────┼─────────────┼─────────────┼─────────┘
        │             │             │             │
        └─────────────┴─────────────┴─────────────┘
                          │
        ┌─────────────────┴─────────────────┐
        │                                   │
┌───────▼────────┐              ┌──────────▼──────────┐
│  CAMADA DE     │              │   CAMADA DE          │
│   SERVIÇOS     │              │    DADOS (Excel)     │
│                │              │                      │
│ ┌────────────┐ │              │ ┌──────────────────┐│
│ │Produto     │ │              │ │ produtos.xlsx     ││
│ │Service     │ │◄─────────────┤ │ clientes.xlsx     ││
│ └────────────┘ │              │ │ vendas.xlsx        ││
│                │              │ │ estoque_          ││
│ ┌────────────┐ │              │ │ movimentacoes.xlsx││
│ │Cliente     │ │              │ │ financeiro.xlsx   ││
│ │Service     │ │              │ │ balancos.xlsx     ││
│ └────────────┘ │              │ └──────────────────┘│
│                │              │                      │
│ ┌────────────┐ │              │                      │
│ │Venda      │ │              │                      │
│ │Service    │ │              │                      │
│ └────────────┘ │              │                      │
│                │              │                      │
│ ┌────────────┐ │              │                      │
│ │Estoque    │ │              │                      │
│ │Service    │ │              │                      │
│ └────────────┘ │              │                      │
│                │              │                      │
│ ┌────────────┐ │              │                      │
│ │Financeiro │ │              │                      │
│ │Service    │ │              │                      │
│ └────────────┘ │              │                      │
│                │              │                      │
│ ┌────────────┐ │              │                      │
│ │Excel      │ │──────────────►│                      │
│ │Service    │ │   (Base)      │                      │
│ │(Base)     │ │              │                      │
│ └────────────┘ │              │                      │
└────────────────┘              └──────────────────────┘
```

### Fluxo de Dados Detalhado:

1. **Usuário interage com View** → ViewModel recebe comando
2. **ViewModel valida dados** → Chama Service apropriado
3. **Service processa regra de negócio** → Chama ExcelService para persistência
4. **ExcelService lê/escreve Excel** → Retorna dados para Service
5. **Service retorna resultado** → ViewModel atualiza propriedades
6. **View atualiza automaticamente** (data binding)

### Integrações entre Módulos:

- **Vendas → Estoque**: Ao finalizar venda, VendaService chama EstoqueService para reduzir estoque
- **Vendas → Financeiro**: Ao finalizar venda à vista, VendaService chama FinanceiroService para criar receita
- **Vendas → Clientes**: Ao finalizar venda a crédito, VendaService atualiza saldo devedor do cliente
- **Clientes → Financeiro**: Ao receber pagamento, ClienteService chama FinanceiroService para criar receita
- **Dashboard → Todos**: DashboardService agrega dados de todos os outros serviços

---

## RESUMO TÉCNICO DO SISTEMA

### Arquitetura:
- **Padrão**: MVVM (Model-View-ViewModel)
- **Tecnologia**: WPF (.NET 8.0)
- **Persistência**: Arquivos Excel (.xlsx) via EPPlus
- **Comunicação**: 100% offline, sem dependências externas

### Componentes Principais:
1. **6 Módulos Funcionais**: Dashboard, Produtos, Clientes, Vendas, Financeiro, Relatórios
2. **7 Services**: Produto, Cliente, Venda, Estoque, Financeiro, Relatório, Dashboard
3. **1 Service Base**: ExcelService (abstração para manipulação de Excel)
4. **6 Planilhas Excel**: Uma por entidade principal
5. **Sistema de Temas**: 3 temas (Claro, Escuro, Karibes)

### Diferenciais Técnicos:
- **Preparado para Migração SQL**: Uso de interfaces e serviços desacoplados
- **Backup Automático**: Proteção contra perda de dados
- **Validações Robustas**: Regras de negócio implementadas em camada de serviço
- **Código Limpo**: Separação de responsabilidades, fácil manutenção

### Próximos Passos de Implementação:
1. Implementar ExcelService completo com CRUD genérico
2. Implementar Services específicos (Produto, Cliente, etc)
3. Desenvolver Views com interface moderna
4. Implementar ViewModels com lógica de apresentação
5. Integrar todos os módulos
6. Testes e validações finais

---

**Documento gerado em**: 2024
**Versão**: 1.0
**Status**: Aprovado para implementação





