PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Produtos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    Nome TEXT NOT NULL,
    Descricao TEXT NOT NULL DEFAULT '',
    Categoria TEXT NOT NULL DEFAULT '',
    PrecoVenda NUMERIC NOT NULL DEFAULT 0,
    PrecoCusto NUMERIC NOT NULL DEFAULT 0,
    EstoqueAtual INTEGER NOT NULL DEFAULT 0,
    EstoqueMinimo INTEGER NOT NULL DEFAULT 0,
    Unidade TEXT NOT NULL DEFAULT 'UN',
    DataCadastro TEXT NOT NULL,
    DataUltimaAtualizacao TEXT NOT NULL,
    Ativo INTEGER NOT NULL DEFAULT 1,
    Observacoes TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Clientes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL,
    Nome TEXT NOT NULL,
    TipoDocumento TEXT NOT NULL DEFAULT '',
    Documento TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Telefone TEXT NOT NULL DEFAULT '',
    Celular TEXT NOT NULL DEFAULT '',
    CEP TEXT NOT NULL DEFAULT '',
    Endereco TEXT NOT NULL DEFAULT '',
    Numero TEXT NOT NULL DEFAULT '',
    Complemento TEXT NOT NULL DEFAULT '',
    Bairro TEXT NOT NULL DEFAULT '',
    Cidade TEXT NOT NULL DEFAULT '',
    Estado TEXT NOT NULL DEFAULT '',
    LimiteCredito NUMERIC NOT NULL DEFAULT 0,
    SaldoDevedor NUMERIC NOT NULL DEFAULT 0,
    TotalPago NUMERIC NOT NULL DEFAULT 0,
    DataVencimentoCredito TEXT NULL,
    DataCadastro TEXT NOT NULL,
    DataUltimaAtualizacao TEXT NOT NULL,
    Ativo INTEGER NOT NULL DEFAULT 1,
    Observacoes TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Vendas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroVenda TEXT NOT NULL UNIQUE,
    ClienteId INTEGER NULL,
    DataVenda TEXT NOT NULL,
    ValorSubtotal NUMERIC NOT NULL DEFAULT 0,
    ValorDesconto NUMERIC NOT NULL DEFAULT 0,
    ValorTotal NUMERIC NOT NULL DEFAULT 0,
    FormaPagamento TEXT NOT NULL,
    Status TEXT NOT NULL,
    Vendedor TEXT NOT NULL DEFAULT '',
    Observacoes TEXT NOT NULL DEFAULT '',
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);

CREATE TABLE IF NOT EXISTS ItensVenda (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    VendaId INTEGER NOT NULL,
    ProdutoId INTEGER NOT NULL,
    Quantidade INTEGER NOT NULL,
    PrecoUnitario NUMERIC NOT NULL,
    Desconto NUMERIC NOT NULL DEFAULT 0,
    ValorTotal NUMERIC NOT NULL,
    FOREIGN KEY (VendaId) REFERENCES Vendas(Id),
    FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id)
);

CREATE TABLE IF NOT EXISTS LancamentosFinanceiros (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo TEXT NOT NULL,
    Categoria TEXT NOT NULL,
    Descricao TEXT NOT NULL,
    Valor NUMERIC NOT NULL,
    DataLancamento TEXT NOT NULL,
    DataVencimento TEXT NULL,
    DataPagamento TEXT NULL,
    Status TEXT NOT NULL,
    FormaPagamento TEXT NOT NULL DEFAULT '',
    Origem TEXT NOT NULL DEFAULT '',
    OrigemId INTEGER NULL,
    Observacoes TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS IX_Vendas_ClienteId ON Vendas(ClienteId);
CREATE INDEX IF NOT EXISTS IX_Vendas_DataVenda ON Vendas(DataVenda);
CREATE INDEX IF NOT EXISTS IX_Lancamentos_DataLancamento ON LancamentosFinanceiros(DataLancamento);
CREATE INDEX IF NOT EXISTS IX_Lancamentos_DataPagamento ON LancamentosFinanceiros(DataPagamento);
