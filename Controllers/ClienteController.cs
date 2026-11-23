using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;

namespace RotaDoSabor.Server.Controllers;

public static class ClienteController
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/clientes").WithTags("clientes");

        group.MapGet("/{clienteId:guid}", async (Guid clienteId, ClienteService service) => Results.Ok(await service.Obter(clienteId)))
            .WithSummary("Retorna os dados do cliente").Produces<ClienteResponse>();

        group.MapPut("/{clienteId:guid}", async (Guid clienteId, ClienteAtualizacaoRequest request, ClienteService service) =>
        {
            await service.Atualizar(clienteId, request);
            return Results.NoContent();
        }).WithSummary("Atualiza os dados do cliente").Produces(StatusCodes.Status204NoContent);
    }
}
