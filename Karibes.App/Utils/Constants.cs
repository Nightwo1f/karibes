namespace Karibes.App.Utils
{
    public static class Constants
    {
        // Caminhos
        public const string DataFolder = "Data";
        public const string ExcelFolder = "Excel";
        public const string BackupsFolder = "Backups";
        
        // Arquivos Excel
        public const string ProdutosFile = "produtos.xlsx";
        public const string ClientesFile = "clientes.xlsx";
        public const string VendasFile = "vendas.xlsx";
        public const string EstoqueMovimentacoesFile = "estoque_movimentacoes.xlsx";
        public const string FinanceiroFile = "financeiro.xlsx";
        public const string BalancosFile = "balancos.xlsx";
        public const string HistoricoCreditoFile = "historico_credito.xlsx";
        
        // Temas
        public const string TemaLight = "Light";
        public const string TemaDark = "Dark";
        public const string TemaKaribes = "Karibes";
        
        // Configurações
        public const string ConfigFile = "config.json";
        public const int BackupRetentionDays = 30;
        
        // Status
        public const string StatusAtivo = "Ativo";
        public const string StatusInativo = "Inativo";
        public const string StatusPendente = "Pendente";
        public const string StatusPago = "Pago";
        public const string StatusCancelado = "Cancelado";
        
        // Formas de Pagamento
        public const string PagamentoDinheiro = "Dinheiro";
        public const string PagamentoCartao = "Cartão";
        public const string PagamentoPix = "Pix";
        public const string PagamentoCredito = "Crédito";
        
        // Tipos de Movimento Estoque
        public const string MovimentoEntrada = "Entrada";
        public const string MovimentoSaida = "Saída";
        public const string MovimentoAjuste = "Ajuste";
        
        // Tipos Financeiro
        public const string TipoReceita = "Receita";
        public const string TipoDespesa = "Despesa";
    }
}

