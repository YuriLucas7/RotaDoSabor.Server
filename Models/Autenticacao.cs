namespace RotaDoSabor.Models;

public class Login
{
    public string EmailOuTelefone { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginRecuperado
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public ClienteRecuperado Cliente { get; set; } = new();
}
