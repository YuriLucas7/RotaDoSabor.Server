using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Repositories;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public class ProdutoService(ProdutoRepository repository, RestauranteRepository restauranteRepository) : BaseService
{
    public async Task<Guid> Cadastrar(Guid restauranteId, ProdutoRequest request)
    {
        if (await restauranteRepository.Obter(restauranteId) is null) throw new NotFoundException("Restaurante não encontrado.");
        Validar(request);
        return await repository.Cadastrar(restauranteId, request);
    }

    public Task<List<Produto>> ObterPorRestaurante(Guid restauranteId, bool somenteDisponiveis) =>
        repository.ObterPorRestaurante(restauranteId, somenteDisponiveis);

    public async Task<Produto> Obter(Guid id) => await repository.Obter(id) ?? throw new NotFoundException("Produto não encontrado.");

    public async Task Atualizar(Guid id, ProdutoRequest request)
    {
        Validar(request);
        if (await repository.Atualizar(id, request) == 0) throw new NotFoundException("Produto não encontrado.");
    }

    private void Validar(ProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) AddError(nameof(request.Nome), "Informe o nome.");
        if (string.IsNullOrWhiteSpace(request.Categoria)) AddError(nameof(request.Categoria), "Informe a categoria.");
        if (request.Preco <= 0) AddError(nameof(request.Preco), "O preço deve ser maior que zero.");
        if (request.PrecoPromocional is <= 0) AddError(nameof(request.PrecoPromocional), "O preço promocional deve ser maior que zero.");
        if (request.PrecoPromocional >= request.Preco) AddError(nameof(request.PrecoPromocional), "O preço promocional deve ser menor que o preço normal.");
        ThrowIfInvalid();
    }
}
