namespace RotaDoSabor.Server.Models;

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

public record PedidoItemRequest(Guid ProdutoId, int Quantidade, string? Observacao);
public record PedidoRequest(
    Guid ClienteId,
    Guid RestauranteId,
    Guid? EnderecoId,
    TipoRecebimento TipoRecebimento,
    FormaPagamento FormaPagamento,
    string? Observacao,
    List<PedidoItemRequest> Itens);
public record AtualizarStatusPedidoRequest(StatusPedido Status, string? Motivo);
public record AtualizarEntregaRequest(StatusEntrega Status, decimal? Latitude, decimal? Longitude);
