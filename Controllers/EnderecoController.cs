using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;

namespace RotaDoSabor.Server.Controllers;

public static class EnderecoController
{
    public static void MapEnderecoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/clientes/{clienteId:guid}/enderecos").WithTags("endereços");

        group.MapPost("/", async (Guid clienteId, EnderecoRequest request, EnderecoService service) =>
        {
            var id = await service.Cadastrar(clienteId, request);
            return Results.Created($"/v1/clientes/{clienteId}/enderecos/{id}", new { enderecoId = id });
        }).WithSummary("Cadastra um endereço");

        group.MapGet("/", async (Guid clienteId, EnderecoService service) => Results.Ok(await service.ObterPorCliente(clienteId)))
            .WithSummary("Lista os endereços do cliente").Produces<List<Endereco>>();

        group.MapDelete("/{enderecoId:guid}", async (Guid enderecoId, EnderecoService service) =>
        {
            await service.Excluir(enderecoId);
            return Results.NoContent();
        }).WithSummary("Exclui um endereço");
    }
}
