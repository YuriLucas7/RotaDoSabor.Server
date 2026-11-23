using Microsoft.IdentityModel.Tokens;
using RotaDoSabor.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RotaDoSabor.Server.Services;

public class TokenService(IConfiguration configuration)
{
    public LoginResponse Gerar(Cliente cliente)
    {
        var expirationMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes", 120);
        var expiraEm = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, cliente.ClienteId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, cliente.Email),
            new Claim(ClaimTypes.Name, cliente.Nome),
            new Claim(ClaimTypes.Role, PerfilUsuario.Cliente.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], audience: configuration["Jwt:Audience"], claims: claims,
            expires: expiraEm, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiraEm, ClienteService.ToResponse(cliente));
    }
}
