using RotaDoSabor.Models;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class AvaliacaoController
{
    public static void MapAvaliacaoEndpoints(this WebApplication app)
    {
        app.MapPost("v1/pedidos/{pedidoId:guid}/avaliacao", async (Guid pedidoId, Avaliacao avaliacao, AvaliacaoService service) =>
        {
            Guid avaliacaoId = await service.CadastrarAvaliacao(avaliacao, pedidoId);
            return Results.Created($"/v1/avaliacoes/{avaliacaoId}", "Avaliação cadastrada com sucesso.");
        })
        .WithTags("avaliações")
        .WithSummary("Registra a avaliação do pedido")
        .Produces(StatusCodes.Status201Created);
    }
}
