# Ajustes Incrementais nos Models

## ✅ Alterações Realizadas

### **1. Novo Arquivo: Enums.cs** ✅
Criado arquivo com enums para padronização:
- `FormaPagamento`: Pix, Dinheiro, Cartao, Credito
- `StatusVenda`: Pendente, Finalizada, Cancelada
- `TipoLancamentoFinanceiro`: Receita, Despesa
- `StatusLancamentoFinanceiro`: Pendente, Pago, Cancelado

### **2. Model Cliente - Propriedades Adicionadas** ✅
**Mantidas todas as propriedades existentes** + adicionadas:
- ✅ `Codigo` (string) - Código único do cliente
- ✅ `TipoDocumento` (string) - CPF ou CNPJ
- ✅ `Celular` (string) - Telefone celular
- ✅ `Numero` (string) - Número do endereço
- ✅ `Complemento` (string) - Complemento do endereço
- ✅ `Bairro` (string) - Bairro
- ✅ `LimiteCredito` (decimal) - Limite de crédito (fiado)
- ✅ `SaldoDevedor` (decimal) - Saldo atual devedor
- ✅ `DataUltimaAtualizacao` (DateTime) - Última atualização
- ✅ `Observacoes` (string) - Observações gerais

### **3. Model Venda - Propriedades Adicionadas** ✅
**Mantidas todas as propriedades existentes** + adicionadas:
- ✅ `NumeroVenda` (string) - Número único da venda
- ✅ `ValorSubtotal` (decimal) - Subtotal antes do desconto
- ✅ `Vendedor` (string) - Nome do vendedor
- ✅ `Observacoes` (string) - Observações da venda

### **4. Model LancamentoFinanceiro - Propriedades Adicionadas** ✅
**Mantidas todas as propriedades existentes** + adicionadas:
- ✅ `Origem` (string) - Origem do lançamento (Venda, Manual, etc)
- ✅ `OrigemId` (int?) - ID da origem (ex: VendaId)
- ✅ `Observacoes` (string) - Observações

### **5. Novo Model: HistoricoCredito.cs** ✅
Criado modelo para rastreamento de crédito:
- `Id` (int)
- `ClienteId` (int)
- `Cliente` (Cliente?)
- `DataMovimento` (DateTime)
- `TipoMovimento` (string) - Compra, Pagamento, Ajuste
- `Valor` (decimal)
- `SaldoAnterior` (decimal)
- `SaldoAtual` (decimal)
- `VendaId` (int?) - ID da venda relacionada
- `Observacoes` (string)

---

## 🔄 Compatibilidade

### ✅ Mantida Compatibilidade:
- Todas as propriedades existentes foram preservadas
- Propriedades antigas continuam funcionando
- Strings para FormaPagamento e Status mantidas (compatibilidade com código existente)
- Enums criados mas não substituem strings (uso opcional)

### 📝 Notas:
- Os enums foram criados para uso futuro, mas as propriedades string continuam funcionando
- Código existente que usa `FormaPagamento` como string continua funcionando
- Código existente que usa `Status` como string continua funcionando
- Novos códigos podem optar por usar os enums ou continuar com strings

---

## 🎯 Suporte Implementado

### Vendas à Vista (Pix, Dinheiro, Cartão):
- ✅ `FormaPagamento` enum com opções: Pix, Dinheiro, Cartao
- ✅ `ValorSubtotal` para cálculo antes do desconto
- ✅ `NumeroVenda` para rastreamento único

### Vendas no Fiado (Crédito Mensal):
- ✅ `FormaPagamento.Credito` no enum
- ✅ `Cliente.LimiteCredito` e `Cliente.SaldoDevedor` para controle
- ✅ `HistoricoCredito` para rastreamento de movimentações

### Histórico de Crédito por Cliente:
- ✅ Model `HistoricoCredito` criado
- ✅ Campos para rastrear: TipoMovimento, Valor, Saldos anterior/atual
- ✅ Vinculação com VendaId para rastreabilidade

### Bloqueio Automático por Limite:
- ✅ `Cliente.LimiteCredito` - Define o limite
- ✅ `Cliente.SaldoDevedor` - Rastreia o saldo atual
- ✅ Validação pode ser feita: `SaldoDevedor <= LimiteCredito`

---

## 📋 Próximos Passos (Para Implementação)

1. **ClienteService**: Usar `LimiteCredito` e `SaldoDevedor` para validações
2. **VendaService**: Validar limite antes de permitir venda a crédito
3. **HistoricoCreditoService**: Criar serviço para registrar movimentações
4. **ClientesViewModel**: Implementar lógica de bloqueio por limite

---

**Status**: ✅ Ajustes Incrementais - COMPLETOS E COMPATÍVEIS





