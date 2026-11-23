using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Repositories;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public class AvaliacaoService(AvaliacaoRepository repository, PedidoRepository pedidoRepository) : BaseService
{
    public async Task<Guid> Cadastrar(Guid pedidoId, AvaliacaoRequest request)
    {
        var pedido = await pedidoRepository.Obter(pedidoId) ?? throw new NotFoundException("Pedido não encontrado.");
        if (pedido.Status != StatusPedido.Entregue) throw new InvalidOperationException("Somente pedidos entregues podem ser avaliados.");
        if (await repository.PedidoAvaliado(pedidoId)) throw new ConflictException("Este pedido já foi avaliado.");
        if (request.NotaRestaurante is < 1 or > 5) AddError(nameof(request.NotaRestaurante), "A nota deve estar entre 1 e 5.");
        if (request.NotaEntrega is < 1 or > 5) AddError(nameof(request.NotaEntrega), "A nota deve estar entre 1 e 5.");
        if (request.NotaComida is < 1 or > 5) AddError(nameof(request.NotaComida), "A nota deve estar entre 1 e 5.");
        ThrowIfInvalid();
        return await repository.Cadastrar(pedido, request);
    }
}
