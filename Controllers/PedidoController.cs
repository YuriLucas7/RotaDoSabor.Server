using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Services;

namespace RotaDoSabor.Server.Controllers;

public static class PedidoController
{
    public static void MapPedidoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("v1/pedidos").WithTags("pedidos");

        group.MapPost("/", async (PedidoRequest request, PedidoService service) =>
        {
            var id = await service.Cadastrar(request);
            return Results.Created($"/v1/pedidos/{id}", new { pedidoId = id });
        }).WithSummary("Cria e calcula um pedido");

        group.MapGet("/{pedidoId:guid}", async (Guid pedidoId, PedidoService service) => Results.Ok(await service.Obter(pedidoId)))
            .WithSummary("Retorna pedido, itens, pagamento e entrega").Produces<Pedido>();

        app.MapGet("v1/clientes/{clienteId:guid}/pedidos", async (Guid clienteId, PedidoService service) =>
            Results.Ok(await service.ObterPorCliente(clienteId)))
            .WithTags("pedidos").WithSummary("Lista os pedidos do cliente").Produces<List<Pedido>>();

        group.MapPatch("/{pedidoId:guid}/status", async (Guid pedidoId, AtualizarStatusPedidoRequest request, PedidoService service) =>
        {
            await service.AtualizarStatus(pedidoId, request);
            return Results.NoContent();
        }).WithSummary("Atualiza o estado do pedido");

        group.MapPatch("/{pedidoId:guid}/pagamento", async (Guid pedidoId, StatusPagamento status, string? transacaoId, PedidoService service) =>
        {
            await service.AtualizarPagamento(pedidoId, status, transacaoId);
            return Results.NoContent();
        }).WithSummary("Registra o resultado do pagamento");

        group.MapPatch("/{pedidoId:guid}/entrega", async (Guid pedidoId, AtualizarEntregaRequest request, PedidoService service) =>
        {
            await service.AtualizarEntrega(pedidoId, request);
            return Results.NoContent();
        }).WithSummary("Atualiza o rastreamento e o estado da entrega");
    }
}
