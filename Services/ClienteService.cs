using Microsoft.AspNetCore.Identity;
using RotaDoSabor.Server.Models;
using RotaDoSabor.Server.Repositories;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Server.Services;

public class ClienteService(ClienteRepository repository) : BaseService
{
    private readonly PasswordHasher<Cliente> _passwordHasher = new();

    public async Task<Guid> Cadastrar(ClienteCadastroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) AddError(nameof(request.Nome), "Informe o nome.");

        if (!RuleValidation.EmailValido(request.Email)) AddError(nameof(request.Email), "Informe um e-mail válido.");

        if (!RuleValidation.TelefoneValido(request.Telefone)) AddError(nameof(request.Telefone), "Informe um telefone válido.");

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 8)
            AddError(nameof(request.Senha), "A senha deve possuir pelo menos 8 caracteres.");

        if (!Errors.Any() && await repository.EmailOuTelefoneExiste(request.Email, request.Telefone))
            AddError(nameof(request.Email), "E-mail ou telefone já cadastrado.");

        ThrowIfInvalid();

        var cliente = new Cliente { Nome=request.Nome.Trim(), Email=request.Email.Trim(), Telefone=request.Telefone.Trim() };
        cliente.SenhaHash = _passwordHasher.HashPassword(cliente, request.Senha);
        return await repository.Cadastrar(cliente);
    }

    public async Task<LoginResponse> Login(LoginRequest request, TokenService tokenService)
    {
        var cliente = await repository.ObterPorLogin(request.EmailOuTelefone)
            ?? throw new InvalidOperationException("E-mail/telefone ou senha inválidos.");

        if (!cliente.Ativo) throw new InvalidOperationException("Conta desativada.");

        var result = _passwordHasher.VerifyHashedPassword(cliente, cliente.SenhaHash, request.Senha);

        if (result == PasswordVerificationResult.Failed) throw new InvalidOperationException("E-mail/telefone ou senha inválidos.");

        return tokenService.Gerar(cliente);
    }

    public async Task<ClienteResponse> Obter(Guid id)
    {
        var cliente = await repository.Obter(id) ?? throw new NotFoundException("Cliente não encontrado.");
        return ToResponse(cliente);
    }

    public async Task Atualizar(Guid id, ClienteAtualizacaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) AddError(nameof(request.Nome), "Informe o nome.");
        if (!RuleValidation.TelefoneValido(request.Telefone)) AddError(nameof(request.Telefone), "Informe um telefone válido.");
        ThrowIfInvalid();
        if (await repository.Atualizar(id, request) == 0) throw new NotFoundException("Cliente não encontrado.");
    }

    public static ClienteResponse ToResponse(Cliente c) => new(c.ClienteId, c.Nome, c.Email, c.Telefone, c.Ativo, c.CriadoEm);
}
