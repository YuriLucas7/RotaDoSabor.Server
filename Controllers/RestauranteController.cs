using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;

namespace RotaDoSabor.Server.Controllers;

public static class RestauranteController
{
    public static void MapRestauranteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/restaurantes").WithTags("restaurantes");

        group.MapPost("/", async (RestauranteRequest request, RestauranteService service) =>
        {
            var id = await service.Cadastrar(request);
            return Results.Created($"/v1/restaurantes/{id}", new { restauranteId = id });
        }).WithSummary("Cadastra um restaurante");

        group.MapGet("/", async (string? busca, bool? aberto, bool? ofertas, RestauranteService service) =>
            Results.Ok(await service.ObterTodos(busca, aberto, ofertas)))
            .WithSummary("Pesquisa restaurantes e ofertas").Produces<List<Restaurante>>();

        group.MapGet("/{restauranteId:guid}", async (Guid restauranteId, RestauranteService service) =>
            Results.Ok(await service.Obter(restauranteId)))
            .WithSummary("Retorna um restaurante").Produces<Restaurante>();

        group.MapPatch("/{restauranteId:guid}/status", async (Guid restauranteId, StatusRestaurante status, bool aberto, RestauranteService service) =>
        {
            await service.AtualizarStatus(restauranteId, status, aberto);
            return Results.NoContent();
        }).WithSummary("Atualiza situação e disponibilidade do restaurante");
    }
}
