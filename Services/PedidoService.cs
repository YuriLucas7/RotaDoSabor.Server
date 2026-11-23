using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Repositories;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public class PedidoService(
    PedidoRepository repository,
    ClienteRepository clienteRepository,
    RestauranteRepository restauranteRepository,
    EnderecoRepository enderecoRepository,
    ProdutoRepository produtoRepository) : BaseService
{
    public async Task<Guid> Cadastrar(PedidoRequest request)
    {
        if (await clienteRepository.Obter(request.ClienteId) is null) throw new NotFoundException("Cliente não encontrado.");
        var restaurante = await restauranteRepository.Obter(request.RestauranteId)
            ?? throw new NotFoundException("Restaurante não encontrado.");
        if (!restaurante.Aberto || restaurante.Status != StatusRestaurante.Ativo)
            throw new InvalidOperationException("Restaurante indisponível para novos pedidos.");
        if (request.TipoRecebimento == TipoRecebimento.Entrega)
        {
            if (!request.EnderecoId.HasValue) AddError(nameof(request.EnderecoId), "Informe o endereço de entrega.");
            else
            {
                var endereco = await enderecoRepository.Obter(request.EnderecoId.Value);
                if (endereco is null || endereco.ClienteId != request.ClienteId)
                    AddError(nameof(request.EnderecoId), "Endereço inválido para este cliente.");
            }
        }
        var itensSolicitados = request.Itens ?? [];
        if (itensSolicitados.Count == 0) AddError(nameof(request.Itens), "Adicione pelo menos um produto.");
        ThrowIfInvalid();

        var itens = new List<(Produto, PedidoItemRequest)>();
        foreach (var item in itensSolicitados)
        {
            if (item.Quantidade <= 0) AddError(nameof(item.Quantidade), "A quantidade deve ser maior que zero.");
            var produto = await produtoRepository.Obter(item.ProdutoId);
            if (produto is null || produto.RestauranteId != request.RestauranteId)
                AddError(nameof(item.ProdutoId), "Produto não pertence ao restaurante informado.");
            else if (!produto.Disponivel)
                AddError(nameof(item.ProdutoId), $"O produto '{produto.Nome}' está indisponível.");
            else itens.Add((produto, item));
        }
        ThrowIfInvalid();

        var subtotal = itens.Sum(x => (x.Item1.PrecoPromocional ?? x.Item1.Preco) * x.Item2.Quantidade);
        if (subtotal < restaurante.PedidoMinimo)
            throw new InvalidOperationException($"O pedido mínimo deste restaurante é {restaurante.PedidoMinimo:C}.");
        return await repository.Cadastrar(request, itens, restaurante.TaxaEntrega);
    }

    public async Task<Pedido> Obter(Guid id) => await repository.Obter(id) ?? throw new NotFoundException("Pedido não encontrado.");
    public Task<List<Pedido>> ObterPorCliente(Guid clienteId) => repository.ObterPorCliente(clienteId);

    public async Task AtualizarStatus(Guid id, AtualizarStatusPedidoRequest request)
    {
        var pedido = await Obter(id);
        if (!TransicaoValida(pedido.Status, request.Status))
            throw new InvalidOperationException($"Transição de {pedido.Status} para {request.Status} não permitida.");
        if (request.Status == StatusPedido.Cancelado && string.IsNullOrWhiteSpace(request.Motivo))
            throw new ValidationException([new ValidationError(nameof(request.Motivo), "Informe o motivo do cancelamento.")]);
        await repository.AtualizarStatus(id, request.Status, request.Motivo);
    }

    public async Task AtualizarPagamento(Guid id, StatusPagamento status, string? transacaoId)
    {
        var pedido = await Obter(id);
        if (pedido.Status != StatusPedido.AguardandoPagamento)
            throw new InvalidOperationException("O pedido não está aguardando pagamento.");
        if (await repository.AtualizarPagamento(id, status, transacaoId) == 0) throw new NotFoundException("Pagamento não encontrado.");
        if (status == StatusPagamento.Aprovado)
            await repository.AtualizarStatus(id, StatusPedido.AguardandoRestaurante, null);
        else if (status == StatusPagamento.Recusado)
            await repository.AtualizarStatus(id, StatusPedido.Cancelado, "Pagamento recusado.");
    }

    public async Task AtualizarEntrega(Guid id, AtualizarEntregaRequest request)
    {
        var pedido = await Obter(id);
        if (pedido.TipoRecebimento != TipoRecebimento.Entrega) throw new InvalidOperationException("Este pedido é para retirada.");
        if (await repository.AtualizarEntrega(id, request) == 0) throw new NotFoundException("Entrega não encontrada.");
        var statusPedido = request.Status switch
        {
            StatusEntrega.Coletado => StatusPedido.EmColeta,
            StatusEntrega.EmRota => StatusPedido.EmEntrega,
            StatusEntrega.Entregue => StatusPedido.Entregue,
            _ => pedido.Status
        };
        if (statusPedido != pedido.Status) await repository.AtualizarStatus(id, statusPedido, null);
    }

    private static bool TransicaoValida(StatusPedido atual, StatusPedido proximo)
    {
        if (proximo == StatusPedido.Cancelado) return atual is not StatusPedido.Entregue and not StatusPedido.Cancelado;
        return (atual, proximo) switch
        {
            (StatusPedido.AguardandoRestaurante, StatusPedido.EmPreparo) => true,
            (StatusPedido.EmPreparo, StatusPedido.Pronto) => true,
            (StatusPedido.Pronto, StatusPedido.EmColeta) => true,
            (StatusPedido.Pronto, StatusPedido.Entregue) => true,
            (StatusPedido.EmColeta, StatusPedido.EmEntrega) => true,
            (StatusPedido.EmEntrega, StatusPedido.Entregue) => true,
            _ => false
        };
    }
}
