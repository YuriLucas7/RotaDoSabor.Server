using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Repositories;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public class RestauranteService(RestauranteRepository repository) : BaseService
{
    public async Task<Guid> Cadastrar(RestauranteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) AddError(nameof(request.Nome), "Informe o nome.");
        if (!RuleValidation.CnpjValido(request.Cnpj)) AddError(nameof(request.Cnpj), "Informe um CNPJ válido.");
        if (!RuleValidation.EmailValido(request.Email)) AddError(nameof(request.Email), "Informe um e-mail válido.");
        if (!RuleValidation.TelefoneValido(request.Telefone)) AddError(nameof(request.Telefone), "Informe um telefone válido.");
        if (request.PedidoMinimo < 0) AddError(nameof(request.PedidoMinimo), "O pedido mínimo não pode ser negativo.");
        if (!Errors.Any() && await repository.CnpjExiste(request.Cnpj)) AddError(nameof(request.Cnpj), "CNPJ já cadastrado.");
        ThrowIfInvalid();
        return await repository.Cadastrar(request);
    }

    public Task<List<Restaurante>> ObterTodos(string? busca, bool? aberto, bool? ofertas) => repository.ObterTodos(busca, aberto, ofertas);

    public async Task<Restaurante> Obter(Guid id) => await repository.Obter(id) ?? throw new NotFoundException("Restaurante não encontrado.");

    public async Task AtualizarStatus(Guid id, StatusRestaurante status, bool aberto)
    {
        if (await repository.AtualizarStatus(id, status, aberto) == 0) throw new NotFoundException("Restaurante não encontrado.");
    }
}
