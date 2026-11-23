namespace RotaDoSabor.Server.Models;

public class Produto
{
    public Guid ProdutoId { get; set; }
    public Guid RestauranteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public bool Disponivel { get; set; }
    public int Ordem { get; set; }
}

public record ProdutoRequest(
    string Nome,
    string Descricao,
    string Categoria,
    string? ImagemUrl,
    decimal Preco,
    decimal? PrecoPromocional,
    bool Disponivel,
    int Ordem);
