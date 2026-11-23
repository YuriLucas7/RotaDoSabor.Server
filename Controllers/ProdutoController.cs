using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;

namespace RotaDoSabor.Server.Controllers;

public static class ProdutoController
{
    public static void MapProdutoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1").WithTags("cardápio e produtos");

        group.MapPost("/restaurantes/{restauranteId:guid}/produtos", async (Guid restauranteId, ProdutoRequest request, ProdutoService service) =>
        {
            var id = await service.Cadastrar(restauranteId, request);
            return Results.Created($"/v1/produtos/{id}", new { produtoId = id });
        }).WithSummary("Cadastra um produto no cardápio");

        group.MapGet("/restaurantes/{restauranteId:guid}/produtos", async (Guid restauranteId, bool somenteDisponiveis, ProdutoService service) =>
            Results.Ok(await service.ObterPorRestaurante(restauranteId, somenteDisponiveis)))
            .WithSummary("Lista o cardápio do restaurante").Produces<List<Produto>>();

        group.MapGet("/produtos/{produtoId:guid}", async (Guid produtoId, ProdutoService service) =>
            Results.Ok(await service.Obter(produtoId)))
            .WithSummary("Retorna o produto").Produces<Produto>();

        group.MapPut("/produtos/{produtoId:guid}", async (Guid produtoId, ProdutoRequest request, ProdutoService service) =>
        {
            await service.Atualizar(produtoId, request);
            return Results.NoContent();
        }).WithSummary("Atualiza produto, preço e disponibilidade");
    }
}
