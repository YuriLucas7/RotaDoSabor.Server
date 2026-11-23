using System.Text.Json.Serialization;

namespace RotaDoSabor.Models;

public class Cliente
{
    [JsonIgnore]
    public Guid ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    [JsonIgnore]
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}

public class ClienteCadastro : Cliente
{
    public string Senha { get; set; } = string.Empty;
}

public class ClienteAtualizado
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}

public class ClienteRecuperado
{
    public Guid ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
