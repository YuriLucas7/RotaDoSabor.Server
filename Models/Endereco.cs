namespace RotaDoSabor.Models;

public class Endereco
{
    public Guid EnderecoId { get; set; }
    public Guid ClienteId { get; set; }
    public string Apelido { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool Principal { get; set; }
}

