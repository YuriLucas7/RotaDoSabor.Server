using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Repositories;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public class EnderecoService(EnderecoRepository repository, ClienteRepository clienteRepository) : BaseService
{
    public async Task<Guid> Cadastrar(Guid clienteId, EnderecoRequest request)
    {
        if (await clienteRepository.Obter(clienteId) is null) throw new NotFoundException("Cliente não encontrado.");
        if (string.IsNullOrWhiteSpace(request.Apelido)) AddError(nameof(request.Apelido), "Informe o apelido do endereço.");
        if (string.IsNullOrWhiteSpace(request.Cep)) AddError(nameof(request.Cep), "Informe o CEP.");
        if (string.IsNullOrWhiteSpace(request.Logradouro)) AddError(nameof(request.Logradouro), "Informe o logradouro.");
        if (string.IsNullOrWhiteSpace(request.Numero)) AddError(nameof(request.Numero), "Informe o número.");
        if (request.Estado?.Length != 2) AddError(nameof(request.Estado), "Informe a UF com 2 caracteres.");
        ThrowIfInvalid();
        return await repository.Cadastrar(clienteId, request);
    }

    public Task<List<Endereco>> ObterPorCliente(Guid clienteId) => repository.ObterPorCliente(clienteId);

    public async Task Excluir(Guid enderecoId)
    {
        if (await repository.Excluir(enderecoId) == 0) throw new NotFoundException("Endereço não encontrado.");
    }
}
