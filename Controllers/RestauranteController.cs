using RotaDoSabor.Models;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class RestauranteController
{
    public static void MapRestauranteEndpoints(this WebApplication app)
    {
        app.MapPost("v1/restaurantes", async (Restaurante restaurante, RestauranteService service) =>
        {
            Guid restauranteId = await service.CadastrarRestaurante(restaurante);
            return Results.Created($"/v1/restaurantes/{restauranteId}", "Restaurante cadastrado com sucesso.");
        })
        .WithTags("restaurantes")
        .WithSummary("Registra um restaurante")
        .Produces(StatusCodes.Status201Created);

        app.MapGet("v1/restaurantes", async (string? busca, bool? aberto, bool? ofertas, RestauranteService service) =>
        {
            var result = await service.ObterRestaurantes(busca, aberto, ofertas);
            return Results.Ok(result);
        })
        .WithTags("restaurantes")
        .WithSummary("Retorna restaurantes e ofertas")
        .Produces<List<Restaurante>>(StatusCodes.Status200OK);

        app.MapGet("v1/restaurantes/{restauranteId:guid}", async (Guid restauranteId, RestauranteService service) =>
        {
            var result = await service.ObterRestaurante(restauranteId);
            return Results.Ok(result);
        })
        .WithTags("restaurantes")
        .WithSummary("Retorna um restaurante por ID")
        .Produces<Restaurante>(StatusCodes.Status200OK);

        app.MapPatch("v1/restaurantes/{restauranteId:guid}/status", async (Guid restauranteId, StatusRestaurante status, bool aberto, RestauranteService service) =>
        {
            await service.AtualizarStatusRestaurante(restauranteId, status, aberto);
            return Results.Ok("Status do restaurante atualizado com sucesso.");
        })
        .WithTags("restaurantes")
        .WithSummary("Atualiza o status do restaurante")
        .Produces(StatusCodes.Status200OK);
    }
}
