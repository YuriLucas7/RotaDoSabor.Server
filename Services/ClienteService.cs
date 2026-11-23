using Microsoft.AspNetCore.Identity;
using RotaDoSabor.Models;
using RotaDoSabor.Repositories;
using RotaDoSabor.Server.Services;
using RotaDoSabor.Server.Validations;

namespace RotaDoSabor.Services;

public class ClienteService : BaseService
{
    private readonly ClienteRepository _repository;
    private readonly PasswordHasher<Cliente> _passwordHasher = new();

    public ClienteService(ClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CadastrarCliente(ClienteCadastro cliente)
    {
        try
        {
            await ValidarCliente(cliente);
            cliente.Nome = cliente.Nome.Trim();
            cliente.Email = cliente.Email.Trim();
            cliente.Telefone = cliente.Telefone.Trim();
            cliente.SenhaHash = _passwordHasher.HashPassword(cliente, cliente.Senha);

            return await _repository.CadastrarCliente(cliente);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao cadastrar cliente: {ex.Message}");
        }
    }

    public async Task<LoginRecuperado> AutenticarCliente(Login login, TokenService tokenService)
    {
        try
        {
            var cliente = await _repository.ObterClientePorLogin(login.EmailOuTelefone)
                ?? throw new InvalidOperationException("E-mail/telefone ou senha inválidos.");

            if (!cliente.Ativo)
                throw new InvalidOperationException("Conta desativada.");

            var resultado = _passwordHasher.VerifyHashedPassword(cliente, cliente.SenhaHash, login.Senha);

            if (resultado == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("E-mail/telefone ou senha inválidos.");

            return tokenService.GerarToken(cliente);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao autenticar cliente: {ex.Message}");
        }
    }

    public async Task<ClienteRecuperado> ObterCliente(Guid clienteId)
    {
        try
        {
            var cliente = await _repository.ObterCliente(clienteId)
                ?? throw new NotFoundException("Cliente não encontrado para o ID informado.");

            return ConverterCliente(cliente);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao recuperar cliente por ID: {ex.Message}");
        }
    }

    public async Task<int> AtualizarCliente(ClienteAtualizado cliente, Guid clienteId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cliente.Nome))
                AddError(nameof(cliente.Nome), "Informe o nome.");

            if (!RuleValidation.TelefoneValido(cliente.Telefone))
                AddError(nameof(cliente.Telefone), "Informe um telefone válido.");

            if (Errors.Any())
                throw new ValidationException(Errors);

            var affected = await _repository.AtualizarCliente(cliente, clienteId);

            if (affected <= 0)
                throw new NotFoundException("Cliente não encontrado para o ID informado.");

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
            throw new Exception($"Erro ao atualizar cliente: {ex.Message}");
        }
    }

    public static ClienteRecuperado ConverterCliente(Cliente cliente)
    {
        return new ClienteRecuperado
        {
            ClienteId = cliente.ClienteId,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Ativo = cliente.Ativo,
            CriadoEm = cliente.CriadoEm
        };
    }

    private async Task ValidarCliente(ClienteCadastro cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome))
            AddError(nameof(cliente.Nome), "Informe o nome.");

        if (!RuleValidation.EmailValido(cliente.Email))
            AddError(nameof(cliente.Email), "Informe um e-mail válido.");

        if (!RuleValidation.TelefoneValido(cliente.Telefone))
            AddError(nameof(cliente.Telefone), "Informe um telefone válido.");

        if (string.IsNullOrWhiteSpace(cliente.Senha) || cliente.Senha.Length < 8)
            AddError(nameof(cliente.Senha), "A senha deve possuir pelo menos 8 caracteres.");

        if (!Errors.Any() && await _repository.VerificarClienteExiste(cliente.Email, cliente.Telefone))
            AddError(nameof(cliente.Email), "E-mail ou telefone já cadastrado.");

        if (Errors.Any())
            throw new ValidationException(Errors);
    }
}
