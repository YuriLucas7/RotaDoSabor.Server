namespace RotaDoSabor.Models;

public class Pedido
{
    public Guid PedidoId { get; set; }
    public long Numero { get; set; }
    public Guid ClienteId { get; set; }
    public Guid RestauranteId { get; set; }
    public Guid? EnderecoId { get; set; }
    public TipoRecebimento TipoRecebimento { get; set; }
    public StatusPedido Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxaEntrega { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public string Observacao { get; set; } = string.Empty;
    public string? MotivoCancelamento { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public List<PedidoItem> Itens { get; set; } = [];
    public Pagamento? Pagamento { get; set; }
    public Entrega? Entrega { get; set; }
}

public class PedidoItem
{
    public Guid PedidoItemId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Total { get; set; }
    public string Observacao { get; set; } = string.Empty;
}

public class Pagamento
{
    public Guid PagamentoId { get; set; }
    public Guid PedidoId { get; set; }
    public FormaPagamento Forma { get; set; }
    public StatusPagamento Status { get; set; }
    public decimal Valor { get; set; }
    public string TransacaoExternaId { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class Entrega
{
    public Guid EntregaId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? EntregadorId { get; set; }
    public StatusEntrega Status { get; set; }
    public string CodigoConfirmacao { get; set; } = string.Empty;
    public decimal? LatitudeAtual { get; set; }
    public decimal? LongitudeAtual { get; set; }
    public DateTime? ColetadoEm { get; set; }
    public DateTime? EntregueEm { get; set; }
}

public class PedidoCadastro
{
    public Guid ClienteId { get; set; }
    public Guid RestauranteId { get; set; }
    public Guid? EnderecoId { get; set; }
    public TipoRecebimento TipoRecebimento { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public string Observacao { get; set; } = string.Empty;
    public List<PedidoItemCadastro> Itens { get; set; } = [];
}

public class PedidoItemCadastro
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public string Observacao { get; set; } = string.Empty;
}

public class StatusPedidoAtualizado
{
    public StatusPedido Status { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class EntregaAtualizada
{
    public StatusEntrega Status { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
