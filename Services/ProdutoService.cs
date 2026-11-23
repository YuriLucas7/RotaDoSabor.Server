using RotaDoSabor.Models;
using RotaDoSabor.Repositories;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Services;

public class ProdutoService : BaseService
{
    private readonly ProdutoRepository _repository;
    private readonly RestauranteRepository _restauranteRepository;

    public ProdutoService(ProdutoRepository repository, RestauranteRepository restauranteRepository)
    {
        _repository = repository;
        _restauranteRepository = restauranteRepository;
    }

    public async Task<Guid> CadastrarProduto(Produto produto, Guid restauranteId)
    {
        try
        {
            if (await _restauranteRepository.ObterRestaurante(restauranteId) == null)
                throw new NotFoundException("Restaurante não encontrado para o ID informado.");

            produto.RestauranteId = restauranteId;
            ValidarProduto(produto);

            return await _repository.CadastrarProduto(produto);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao cadastrar produto: {ex.Message}");
        }
    }

    public async Task<List<Produto>> ObterProdutosPorRestaurante(Guid restauranteId, bool somenteDisponiveis)
    {
        try
        {
            return await _repository.ObterProdutosPorRestaurante(restauranteId, somenteDisponiveis);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao recuperar produtos: {ex.Message}");
        }
    }

    public async Task<Produto> ObterProduto(Guid produtoId)
    {
        try
        {
            return await _repository.ObterProduto(produtoId)
                ?? throw new NotFoundException("Produto não encontrado para o ID informado.");
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao recuperar produto por ID: {ex.Message}");
        }
    }

    public async Task<int> AtualizarProduto(Produto produto, Guid produtoId)
    {
        try
        {
            ValidarProduto(produto);
            var affected = await _repository.AtualizarProduto(produto, produtoId);

            if (affected <= 0)
                throw new NotFoundException("Produto não encontrado para o ID informado.");

            return affected;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao atualizar produto: {ex.Message}");
        }
    }

    private void ValidarProduto(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Nome)) AddError(nameof(produto.Nome), "Informe o nome.");
        if (string.IsNullOrWhiteSpace(produto.Categoria)) AddError(nameof(produto.Categoria), "Informe a categoria.");
        if (produto.Preco <= 0) AddError(nameof(produto.Preco), "O preço deve ser maior que zero.");
        if (produto.PrecoPromocional is <= 0) AddError(nameof(produto.PrecoPromocional), "O preço promocional deve ser maior que zero.");
        if (produto.PrecoPromocional >= produto.Preco) AddError(nameof(produto.PrecoPromocional), "O preço promocional deve ser menor que o preço normal.");

        if (Errors.Any())
            throw new ValidationException(Errors);
    }
}
