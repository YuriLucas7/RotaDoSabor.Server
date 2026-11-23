using RotaDoSabor.Models;
using RotaDoSabor.Repositories;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Services;

public class RestauranteService : BaseService
{
    private readonly RestauranteRepository _repository;

    public RestauranteService(RestauranteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CadastrarRestaurante(Restaurante restaurante)
    {
        try
        {
            await ValidarRestaurante(restaurante);
            return await _repository.CadastrarRestaurante(restaurante);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao cadastrar restaurante: {ex.Message}");
        }
    }

    public async Task<List<Restaurante>> ObterRestaurantes(string? busca, bool? aberto, bool? ofertas)
    {
        try
        {
            return await _repository.ObterRestaurantes(busca, aberto, ofertas);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao recuperar restaurantes: {ex.Message}");
        }
    }

    public async Task<Restaurante> ObterRestaurante(Guid restauranteId)
    {
        try
        {
            return await _repository.ObterRestaurante(restauranteId)
                ?? throw new NotFoundException("Restaurante não encontrado para o ID informado.");
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao recuperar restaurante por ID: {ex.Message}");
        }
    }

    public async Task<int> AtualizarStatusRestaurante(Guid restauranteId, StatusRestaurante status, bool aberto)
    {
        try
        {
            var affected = await _repository.AtualizarStatusRestaurante(restauranteId, status, aberto);

            if (affected <= 0)
                throw new NotFoundException("Restaurante não encontrado para o ID informado.");

            return affected;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao atualizar restaurante: {ex.Message}");
        }
    }

    private async Task ValidarRestaurante(Restaurante restaurante)
    {
        if (string.IsNullOrWhiteSpace(restaurante.Nome)) AddError(nameof(restaurante.Nome), "Informe o nome.");
        if (!RuleValidation.CnpjValido(restaurante.Cnpj)) AddError(nameof(restaurante.Cnpj), "Informe um CNPJ válido.");
        if (!RuleValidation.EmailValido(restaurante.Email)) AddError(nameof(restaurante.Email), "Informe um e-mail válido.");
        if (!RuleValidation.TelefoneValido(restaurante.Telefone)) AddError(nameof(restaurante.Telefone), "Informe um telefone válido.");
        if (restaurante.PedidoMinimo < 0) AddError(nameof(restaurante.PedidoMinimo), "O pedido mínimo não pode ser negativo.");
        if (!Errors.Any() && await _repository.VerificarRestauranteExiste(restaurante.Cnpj)) AddError(nameof(restaurante.Cnpj), "CNPJ já cadastrado.");

        if (Errors.Any())
            throw new ValidationException(Errors);
    }
}
