namespace RotaDoSabor.Server.Models;

public record LoginRequest(string EmailOuTelefone, string Senha);
public record LoginResponse(string Token, DateTime ExpiraEm, ClienteResponse Cliente);
