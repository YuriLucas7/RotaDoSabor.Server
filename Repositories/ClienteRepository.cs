using Npgsql;
using RotaDoSabor.Server.Models;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public class ClienteRepository : BaseRepository
{
    public ClienteRepository(IDbConnection connection) : base(connection) { }

    public async Task<Guid> Cadastrar(Cliente cliente)
    {
        const string sql = """
            INSERT INTO rota_sabor.cliente (nome, email, telefone, senha_hash, ativo)
            VALUES (@nome, @email, @telefone, @senha_hash, true)
            RETURNING cliente_id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("nome", cliente.Nome);
        cmd.Parameters.AddWithValue("email", cliente.Email);
        cmd.Parameters.AddWithValue("telefone", cliente.Telefone);
        cmd.Parameters.AddWithValue("senha_hash", cliente.SenhaHash);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<Cliente?> Obter(Guid clienteId)
    {
        const string sql = """
            SELECT cliente_id, nome, email, telefone, senha_hash, ativo, criado_em
            FROM rota_sabor.cliente WHERE cliente_id = @id LIMIT 1;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", clienteId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<Cliente?> ObterPorLogin(string login)
    {
        const string sql = """
            SELECT cliente_id, nome, email, telefone, senha_hash, ativo, criado_em
            FROM rota_sabor.cliente
            WHERE lower(email) = lower(@login) OR telefone = @login
            LIMIT 1;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("login", login);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<bool> EmailOuTelefoneExiste(string email, string telefone)
    {
        const string sql = "SELECT 1 FROM rota_sabor.cliente WHERE lower(email)=lower(@email) OR telefone=@telefone LIMIT 1;";
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("telefone", telefone);
        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task<int> Atualizar(Guid clienteId, ClienteAtualizacaoRequest request)
    {
        const string sql = "UPDATE rota_sabor.cliente SET nome=@nome, telefone=@telefone WHERE cliente_id=@id;";
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", clienteId);
        cmd.Parameters.AddWithValue("nome", request.Nome);
        cmd.Parameters.AddWithValue("telefone", request.Telefone);
        return await cmd.ExecuteNonQueryAsync();
    }

    private static Cliente Map(NpgsqlDataReader reader) => new()
    {
        ClienteId = reader.GetGuid("cliente_id"),
        Nome = reader.GetString("nome"),
        Email = reader.GetString("email"),
        Telefone = reader.GetString("telefone"),
        SenhaHash = reader.GetString("senha_hash"),
        Ativo = reader.GetBoolean("ativo"),
        CriadoEm = reader.GetDateTime("criado_em")
    };
}
