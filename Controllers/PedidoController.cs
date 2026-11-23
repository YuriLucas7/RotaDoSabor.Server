using RotaDoSabor.Models;
using RotaDoSabor.Services;

namespace RotaDoSabor.Controllers;

public static class PedidoController
{
    public static void MapPedidoEndpoints(this WebApplication app)
    {
        app.MapPost("v1/pedidos", async (PedidoCadastro pedido, PedidoService service) =>
        {
            Guid pedidoId = await service.CadastrarPedido(pedido);
            return Results.Created($"/v1/pedidos/{pedidoId}", "Pedido cadastrado com sucesso.");
        })
        .WithTags("pedidos")
        .WithSummary("Registra um pedido")
        .Produces(StatusCodes.Status201Created);

        app.MapGet("v1/pedidos/{pedidoId:guid}", async (Guid pedidoId, PedidoService service) =>
        {
            var result = await service.ObterPedido(pedidoId);
            return Results.Ok(result);
        })
        .WithTags("pedidos")
        .WithSummary("Retorna um pedido por ID")
        .Produces<Pedido>(StatusCodes.Status200OK);

        app.MapGet("v1/clientes/{clienteId:guid}/pedidos", async (Guid clienteId, PedidoService service) =>
        {
            var result = await service.ObterPedidosPorCliente(clienteId);
            return Results.Ok(result);
        })
        .WithTags("pedidos")
        .WithSummary("Retorna os pedidos do cliente")
        .Produces<List<Pedido>>(StatusCodes.Status200OK);

        app.MapPatch("v1/pedidos/{pedidoId:guid}/status", async (Guid pedidoId, StatusPedidoAtualizado status, PedidoService service) =>
        {
            await service.AtualizarStatusPedido(pedidoId, status);
            return Results.Ok("Status do pedido atualizado com sucesso.");
        })
        .WithTags("pedidos")
        .WithSummary("Atualiza o status do pedido")
        .Produces(StatusCodes.Status200OK);

        app.MapPatch("v1/pedidos/{pedidoId:guid}/pagamento", async (Guid pedidoId, StatusPagamento status, string? transacaoId, PedidoService service) =>
        {
            await service.AtualizarPagamento(pedidoId, status, transacaoId);
            return Results.Ok("Pagamento atualizado com sucesso.");
        })
        .WithTags("pedidos")
        .WithSummary("Atualiza o pagamento do pedido")
        .Produces(StatusCodes.Status200OK);

        app.MapPatch("v1/pedidos/{pedidoId:guid}/entrega", async (Guid pedidoId, EntregaAtualizada entrega, PedidoService service) =>
        {
            await service.AtualizarEntrega(pedidoId, entrega);
            return Results.Ok("Entrega atualizada com sucesso.");
        })
        .WithTags("pedidos")
        .WithSummary("Atualiza a entrega do pedido")
        .Produces(StatusCodes.Status200OK);
    }
}
