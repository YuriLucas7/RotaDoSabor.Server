namespace RotaDoSabor.Models;

public class Restaurante
{
    public Guid RestauranteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PedidoMinimo { get; set; }
    public decimal TaxaEntrega { get; set; }
    public int TempoEntregaMinimo { get; set; }
    public int TempoEntregaMaximo { get; set; }
    public decimal AvaliacaoMedia { get; set; }
    public bool Aberto { get; set; }
    public StatusRestaurante Status { get; set; }
    public bool PossuiOferta { get; set; }
}

