using Microsoft.IdentityModel.Tokens;
using RotaDoSabor.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RotaDoSabor.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginRecuperado GerarToken(Cliente cliente)
    {
        int expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 120);
        DateTime expiraEm = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, cliente.ClienteId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, cliente.Email),
            new Claim(ClaimTypes.Name, cliente.Nome),
            new Claim(ClaimTypes.Role, PerfilUsuario.Cliente.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginRecuperado
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEm = expiraEm,
            Cliente = ClienteService.ConverterCliente(cliente)
        };
    }
}
