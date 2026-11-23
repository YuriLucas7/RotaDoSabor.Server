using RotaDoSabor.Models;
using RotaDoSabor.Repositories;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Services;

public class EnderecoService : BaseService
{
    private readonly EnderecoRepository _repository;
    private readonly ClienteRepository _clienteRepository;

    public EnderecoService(EnderecoRepository repository, ClienteRepository clienteRepository)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
    }

    public async Task<Guid> CadastrarEndereco(Endereco endereco, Guid clienteId)
    {
        try
        {
            if (await _clienteRepository.ObterCliente(clienteId) == null)
                throw new NotFoundException("Cliente não encontrado para o ID informado.");

            endereco.ClienteId = clienteId;
            ValidarEndereco(endereco);

            return await _repository.CadastrarEndereco(endereco);
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
            throw new Exception($"Erro ao cadastrar endereço: {ex.Message}");
        }
    }

    public async Task<List<Endereco>> ObterEnderecosPorCliente(Guid clienteId)
    {
        try
        {
            return await _repository.ObterEnderecosPorCliente(clienteId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao recuperar endereços: {ex.Message}");
        }
    }

    public async Task<int> ExcluirEndereco(Guid enderecoId)
    {
        try
        {
            var affected = await _repository.ExcluirEndereco(enderecoId);

            if (affected <= 0)
                throw new NotFoundException("Endereço não encontrado para o ID informado.");

            return affected;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao excluir endereço: {ex.Message}");
        }
    }

    private void ValidarEndereco(Endereco endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco.Apelido)) AddError(nameof(endereco.Apelido), "Informe o apelido do endereço.");
        if (string.IsNullOrWhiteSpace(endereco.Cep)) AddError(nameof(endereco.Cep), "Informe o CEP.");
        if (string.IsNullOrWhiteSpace(endereco.Logradouro)) AddError(nameof(endereco.Logradouro), "Informe o logradouro.");
        if (string.IsNullOrWhiteSpace(endereco.Numero)) AddError(nameof(endereco.Numero), "Informe o número.");
        if (endereco.Estado.Length != 2) AddError(nameof(endereco.Estado), "Informe a UF com 2 caracteres.");

        if (Errors.Any())
            throw new ValidationException(Errors);
    }
}
