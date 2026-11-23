using System.Text.Json.Serialization;

namespace RotaDoSabor.Server.Models;

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

public record ClienteCadastroRequest(string Nome, string Email, string Telefone, string Senha);
public record ClienteAtualizacaoRequest(string Nome, string Telefone);
public record ClienteResponse(Guid ClienteId, string Nome, string Email, string Telefone, bool Ativo, DateTime CriadoEm);
