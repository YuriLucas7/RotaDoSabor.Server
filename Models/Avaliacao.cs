namespace RotaDoSabor.Models;

public class Avaliacao
{
    public Guid AvaliacaoId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid RestauranteId { get; set; }
    public int NotaRestaurante { get; set; }
    public int NotaEntrega { get; set; }
    public int NotaComida { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

