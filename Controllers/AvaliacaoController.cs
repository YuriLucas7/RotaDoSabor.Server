using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;

namespace RotaDoSabor.Server.Controllers;

public static class AvaliacaoController
{
    public static void MapAvaliacaoEndpoints(this WebApplication app)
    {
        app.MapPost("v1/pedidos/{pedidoId:guid}/avaliacao", async (Guid pedidoId, AvaliacaoRequest request, AvaliacaoService service) =>
        {
            var id = await service.Cadastrar(pedidoId, request);
            return Results.Created($"/v1/avaliacoes/{id}", new { avaliacaoId = id });
        }).WithTags("avaliações").WithSummary("Avalia restaurante, entrega e comida");
    }
}
