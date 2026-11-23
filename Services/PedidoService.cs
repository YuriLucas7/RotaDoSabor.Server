using RotaDoSabor.Models;
using RotaDoSabor.Repositories;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Services;

public class PedidoService : BaseService
{
    private readonly PedidoRepository _repository;
    private readonly ClienteRepository _clienteRepository;
    private readonly RestauranteRepository _restauranteRepository;
    private readonly EnderecoRepository _enderecoRepository;
    private readonly ProdutoRepository _produtoRepository;

    public PedidoService(PedidoRepository repository, ClienteRepository clienteRepository, RestauranteRepository restauranteRepository, EnderecoRepository enderecoRepository, ProdutoRepository produtoRepository)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _restauranteRepository = restauranteRepository;
        _enderecoRepository = enderecoRepository;
        _produtoRepository = produtoRepository;
    }

    public async Task<Guid> CadastrarPedido(PedidoCadastro pedido)
    {
        try
        {
            if (await _clienteRepository.ObterCliente(pedido.ClienteId) == null)
                throw new NotFoundException("Cliente não encontrado para o ID informado.");

            var restaurante = await _restauranteRepository.ObterRestaurante(pedido.RestauranteId)
                ?? throw new NotFoundException("Restaurante não encontrado para o ID informado.");

            if (!restaurante.Aberto || restaurante.Status != StatusRestaurante.Ativo)
                throw new InvalidOperationException("Restaurante indisponível para novos pedidos.");

            await ValidarEnderecoEntrega(pedido);
            var itens = await ValidarItens(pedido);
            decimal subtotal = itens.Sum(x => (x.Produto.PrecoPromocional ?? x.Produto.Preco) * x.Item.Quantidade);

            if (subtotal < restaurante.PedidoMinimo)
                throw new InvalidOperationException($"O pedido mínimo deste restaurante é {restaurante.PedidoMinimo:C}.");

            return await _repository.CadastrarPedido(pedido, itens, restaurante.TaxaEntrega);
        }
        catch (NotFoundException) { throw; }
        catch (ValidationException) { throw; }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Erro ao cadastrar pedido: {ex.Message}"); }
    }

    public async Task<Pedido> ObterPedido(Guid pedidoId)
    {
        try
        {
            return await _repository.ObterPedido(pedidoId)
                ?? throw new NotFoundException("Pedido não encontrado para o ID informado.");
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex) { throw new Exception($"Erro ao recuperar pedido por ID: {ex.Message}"); }
    }

    public async Task<List<Pedido>> ObterPedidosPorCliente(Guid clienteId)
    {
        try
        {
            return await _repository.ObterPedidosPorCliente(clienteId);
        }
        catch (Exception ex) { throw new Exception($"Erro ao recuperar pedidos: {ex.Message}"); }
    }

    public async Task AtualizarStatusPedido(Guid pedidoId, StatusPedidoAtualizado statusAtualizado)
    {
        try
        {
            var pedido = await ObterPedido(pedidoId);

            if (!TransicaoValida(pedido.Status, statusAtualizado.Status))
                throw new InvalidOperationException($"Transição de {pedido.Status} para {statusAtualizado.Status} não permitida.");

            if (statusAtualizado.Status == StatusPedido.Cancelado && string.IsNullOrWhiteSpace(statusAtualizado.Motivo))
                throw new ValidationException([new ValidationError(nameof(statusAtualizado.Motivo), "Informe o motivo do cancelamento.")]);

            await _repository.AtualizarStatusPedido(pedidoId, statusAtualizado.Status, statusAtualizado.Motivo);
        }
        catch (NotFoundException) { throw; }
        catch (ValidationException) { throw; }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Erro ao atualizar status do pedido: {ex.Message}"); }
    }

    public async Task AtualizarPagamento(Guid pedidoId, StatusPagamento status, string? transacaoId)
    {
        try
        {
            var pedido = await ObterPedido(pedidoId);

            if (pedido.Status != StatusPedido.AguardandoPagamento)
                throw new InvalidOperationException("O pedido não está aguardando pagamento.");

            if (await _repository.AtualizarPagamento(pedidoId, status, transacaoId) <= 0)
                throw new NotFoundException("Pagamento não encontrado.");

            if (status == StatusPagamento.Aprovado)
                await _repository.AtualizarStatusPedido(pedidoId, StatusPedido.AguardandoRestaurante, null);
            else if (status == StatusPagamento.Recusado)
                await _repository.AtualizarStatusPedido(pedidoId, StatusPedido.Cancelado, "Pagamento recusado.");
        }
        catch (NotFoundException) { throw; }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Erro ao atualizar pagamento: {ex.Message}"); }
    }

    public async Task AtualizarEntrega(Guid pedidoId, EntregaAtualizada entrega)
    {
        try
        {
            var pedido = await ObterPedido(pedidoId);

            if (pedido.TipoRecebimento != TipoRecebimento.Entrega)
                throw new InvalidOperationException("Este pedido é para retirada.");

            if (await _repository.AtualizarEntrega(pedidoId, entrega) <= 0)
                throw new NotFoundException("Entrega não encontrada.");

            StatusPedido statusPedido = entrega.Status switch
            {
                StatusEntrega.Coletado => StatusPedido.EmColeta,
                StatusEntrega.EmRota => StatusPedido.EmEntrega,
                StatusEntrega.Entregue => StatusPedido.Entregue,
                _ => pedido.Status
            };

            if (statusPedido != pedido.Status)
                await _repository.AtualizarStatusPedido(pedidoId, statusPedido, null);
        }
        catch (NotFoundException) { throw; }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) { throw new Exception($"Erro ao atualizar entrega: {ex.Message}"); }
    }

    private async Task ValidarEnderecoEntrega(PedidoCadastro pedido)
    {
        if (pedido.TipoRecebimento != TipoRecebimento.Entrega) return;

        if (!pedido.EnderecoId.HasValue)
            AddError(nameof(pedido.EnderecoId), "Informe o endereço de entrega.");
        else
        {
            var endereco = await _enderecoRepository.ObterEndereco(pedido.EnderecoId.Value);
            if (endereco == null || endereco.ClienteId != pedido.ClienteId)
                AddError(nameof(pedido.EnderecoId), "Endereço inválido para este cliente.");
        }

        if (Errors.Any()) throw new ValidationException(Errors);
    }

    private async Task<List<(Produto Produto, PedidoItemCadastro Item)>> ValidarItens(PedidoCadastro pedido)
    {
        var itens = new List<(Produto Produto, PedidoItemCadastro Item)>();

        if (pedido.Itens.Count == 0) AddError(nameof(pedido.Itens), "Adicione pelo menos um produto.");

        foreach (var item in pedido.Itens)
        {
            if (item.Quantidade <= 0) AddError(nameof(item.Quantidade), "A quantidade deve ser maior que zero.");
            var produto = await _produtoRepository.ObterProduto(item.ProdutoId);

            if (produto == null || produto.RestauranteId != pedido.RestauranteId)
                AddError(nameof(item.ProdutoId), "Produto não pertence ao restaurante informado.");
            else if (!produto.Disponivel)
                AddError(nameof(item.ProdutoId), $"O produto '{produto.Nome}' está indisponível.");
            else
                itens.Add((produto, item));
        }

        if (Errors.Any()) throw new ValidationException(Errors);
        return itens;
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
