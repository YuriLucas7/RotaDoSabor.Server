using RotaDoSabor.Models;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class ProdutoController
{
    public static void MapProdutoEndpoints(this WebApplication app)
    {
        app.MapPost("v1/restaurantes/{restauranteId:guid}/produtos", async (Guid restauranteId, Produto produto, ProdutoService service) =>
        {
            Guid produtoId = await service.CadastrarProduto(produto, restauranteId);
            return Results.Created($"/v1/produtos/{produtoId}", "Produto cadastrado com sucesso.");
        })
        .WithTags("produtos")
        .WithSummary("Registra um produto")
        .Produces(StatusCodes.Status201Created);

        app.MapGet("v1/restaurantes/{restauranteId:guid}/produtos", async (Guid restauranteId, bool somenteDisponiveis, ProdutoService service) =>
        {
            var result = await service.ObterProdutosPorRestaurante(restauranteId, somenteDisponiveis);
            return Results.Ok(result);
        })
        .WithTags("produtos")
        .WithSummary("Retorna o cardápio do restaurante")
        .Produces<List<Produto>>(StatusCodes.Status200OK);

        app.MapGet("v1/produtos/{produtoId:guid}", async (Guid produtoId, ProdutoService service) =>
        {
            var result = await service.ObterProduto(produtoId);
            return Results.Ok(result);
        })
        .WithTags("produtos")
        .WithSummary("Retorna um produto por ID")
        .Produces<Produto>(StatusCodes.Status200OK);

        app.MapPut("v1/produtos/{produtoId:guid}", async (Guid produtoId, Produto produto, ProdutoService service) =>
        {
            await service.AtualizarProduto(produto, produtoId);
            return Results.Ok("Produto atualizado com sucesso.");
        })
        .WithTags("produtos")
        .WithSummary("Atualiza um produto")
        .Produces(StatusCodes.Status200OK);
    }
}
