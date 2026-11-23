using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Controllers;

public static class AuthController
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/auth").WithTags("autenticação");

        group.MapPost("/cadastro", async (ClienteCadastroRequest request, ClienteService service) =>
        {
            var id = await service.Cadastrar(request);
            return Results.Created($"/v1/clientes/{id}", new { clienteId = id });
        })
        .WithSummary("Cadastra um novo cliente")
        .Produces(StatusCodes.Status201Created)
        .Produces<List<ValidationError>>(StatusCodes.Status400BadRequest);

        group.MapPost("/login", async (LoginRequest request, ClienteService service, TokenService tokenService) =>
            Results.Ok(await service.Login(request, tokenService)))
        .WithSummary("Autentica o cliente")
        .Produces<LoginResponse>()
        .Produces<string>(StatusCodes.Status400BadRequest);
    }
}
