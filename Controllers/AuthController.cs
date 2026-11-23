using RotaDoSabor.Models;
using RotaDoSabor.Server.Validations;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class AuthController
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("v1/auth/cadastro", async (ClienteCadastro cliente, ClienteService service) =>
        {
            Guid clienteId = await service.CadastrarCliente(cliente);

            return Results.Created($"/v1/clientes/{clienteId}", "Cliente cadastrado com sucesso.");
        })
        .WithTags("autenticação")
        .WithSummary("Registra um novo cliente")
        .WithDescription("Registra um novo cliente com telefone, e-mail e senha.")
        .Produces(StatusCodes.Status201Created)
        .Produces<List<ValidationError>>(StatusCodes.Status400BadRequest)
        .Produces<string>(StatusCodes.Status500InternalServerError);

        app.MapPost("v1/auth/login", async (Login login, ClienteService service, TokenService tokenService) =>
        {
            var result = await service.AutenticarCliente(login, tokenService);

            return Results.Ok(result);
        })
        .WithTags("autenticação")
        .WithSummary("Autentica um cliente")
        .WithDescription("Autentica por telefone ou e-mail e retorna o token JWT.")
        .Produces<LoginRecuperado>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces<string>(StatusCodes.Status500InternalServerError);
    }
}
