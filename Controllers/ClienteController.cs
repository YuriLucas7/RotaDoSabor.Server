using RotaDoSabor.Models;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class ClienteController
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        app.MapGet("v1/clientes/{clienteId:guid}", async (Guid clienteId, ClienteService service) =>
        {
            var result = await service.ObterCliente(clienteId);
            return Results.Ok(result);
        })
        .WithTags("clientes")
        .WithSummary("Retorna um cliente por ID")
        .Produces<ClienteRecuperado>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status404NotFound);

        app.MapPut("v1/clientes/{clienteId:guid}", async (Guid clienteId, ClienteAtualizado cliente, ClienteService service) =>
        {
            await service.AtualizarCliente(cliente, clienteId);
            return Results.Ok("Cliente atualizado com sucesso.");
        })
        .WithTags("clientes")
        .WithSummary("Atualiza um cliente")
        .Produces(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status404NotFound);
    }
}
