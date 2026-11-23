using RotaDoSabor.Models;
using RotaDoSabor.Repositories;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Services;

public class AvaliacaoService : BaseService
{
    private readonly AvaliacaoRepository _repository;
    private readonly PedidoRepository _pedidoRepository;

    public AvaliacaoService(AvaliacaoRepository repository, PedidoRepository pedidoRepository)
    {
        _repository = repository;
        _pedidoRepository = pedidoRepository;
    }

    public async Task<Guid> CadastrarAvaliacao(Avaliacao avaliacao, Guid pedidoId)
    {
        try
        {
            var pedido = await _pedidoRepository.ObterPedido(pedidoId)
                ?? throw new NotFoundException("Pedido não encontrado para o ID informado.");

            if (pedido.Status != StatusPedido.Entregue)
                throw new InvalidOperationException("Somente pedidos entregues podem ser avaliados.");

            if (await _repository.VerificarPedidoAvaliado(pedidoId))
                throw new ConflictException("Este pedido já foi avaliado.");

            ValidarAvaliacao(avaliacao);

            return await _repository.CadastrarAvaliacao(pedido, avaliacao);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao cadastrar avaliação: {ex.Message}");
        }
    }

    private void ValidarAvaliacao(Avaliacao avaliacao)
    {
        if (avaliacao.NotaRestaurante is < 1 or > 5) AddError(nameof(avaliacao.NotaRestaurante), "A nota deve estar entre 1 e 5.");
        if (avaliacao.NotaEntrega is < 1 or > 5) AddError(nameof(avaliacao.NotaEntrega), "A nota deve estar entre 1 e 5.");
        if (avaliacao.NotaComida is < 1 or > 5) AddError(nameof(avaliacao.NotaComida), "A nota deve estar entre 1 e 5.");

        if (Errors.Any())
            throw new ValidationException(Errors);
    }
}
