namespace Karibes.App.Models
{
    /// <summary>
    /// Formas de pagamento disponíveis
    /// </summary>
    public enum FormaPagamento
    {
        Pix = 1,
        Dinheiro = 2,
        Cartao = 3,
        Credito = 4 // Fiado
    }

    /// <summary>
    /// Status de uma venda
    /// </summary>
    public enum StatusVenda
    {
        Pendente = 1,
        Finalizada = 2,
        Cancelada = 3
    }

    /// <summary>
    /// Tipo de lançamento financeiro
    /// </summary>
    public enum TipoLancamentoFinanceiro
    {
        Receita = 1,
        Despesa = 2
    }

    /// <summary>
    /// Status de um lançamento financeiro
    /// </summary>
    public enum StatusLancamentoFinanceiro
    {
        Pendente = 1,
        Pago = 2,
        Cancelado = 3
    }
}





