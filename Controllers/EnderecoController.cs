using RotaDoSabor.Models;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class EnderecoController
{
    public static void MapEnderecoEndpoints(this WebApplication app)
    {
        app.MapPost("v1/clientes/{clienteId:guid}/enderecos", async (Guid clienteId, Endereco endereco, EnderecoService service) =>
        {
            Guid enderecoId = await service.CadastrarEndereco(endereco, clienteId);
            return Results.Created($"/v1/clientes/{clienteId}/enderecos/{enderecoId}", "Endereço cadastrado com sucesso.");
        })
        .WithTags("endereços")
        .WithSummary("Registra um endereço")
        .Produces(StatusCodes.Status201Created);

        app.MapGet("v1/clientes/{clienteId:guid}/enderecos", async (Guid clienteId, EnderecoService service) =>
        {
            var result = await service.ObterEnderecosPorCliente(clienteId);
            return Results.Ok(result);
        })
        .WithTags("endereços")
        .WithSummary("Retorna os endereços do cliente")
        .Produces<List<Endereco>>(StatusCodes.Status200OK);

        app.MapDelete("v1/enderecos/{enderecoId:guid}", async (Guid enderecoId, EnderecoService service) =>
        {
            await service.ExcluirEndereco(enderecoId);
            return Results.Ok("Endereço excluído com sucesso.");
        })
        .WithTags("endereços")
        .WithSummary("Exclui um endereço")
        .Produces(StatusCodes.Status200OK);
    }
}
