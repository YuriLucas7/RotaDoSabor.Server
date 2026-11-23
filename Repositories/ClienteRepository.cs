using Npgsql;
using RotaDoSabor.Models;
using System.Data;

namespace RotaDoSabor.Repositories;

public class ClienteRepository
{
    private readonly NpgsqlConnection _connection;

    public ClienteRepository(IDbConnection connection)
    {
        _connection = (NpgsqlConnection)connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Guid> CadastrarCliente(Cliente cliente)
    {
        const string sql = @"
            INSERT INTO rota_sabor.cliente
            (
                nome,
                email,
                telefone,
                senha_hash,
                ativo
            )
            VALUES
            (
                @nome,
                @email,
                @telefone,
                @senha_hash,
                true
            )
            RETURNING cliente_id;
        ";

        try
        {
            await EnsureOpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("nome", cliente.Nome);
            cmd.Parameters.AddWithValue("email", cliente.Email);
            cmd.Parameters.AddWithValue("telefone", cliente.Telefone);
            cmd.Parameters.AddWithValue("senha_hash", cliente.SenhaHash);

            var result = await cmd.ExecuteScalarAsync();

            return (Guid)result!;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Cliente?> ObterCliente(Guid clienteId)
    {
        const string sql = @"
            SELECT
                cliente_id,
                nome,
                email,
                telefone,
                senha_hash,
                ativo,
                criado_em
            FROM
                rota_sabor.cliente
            WHERE
                cliente_id = @cliente_id
            LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("cliente_id", clienteId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return Map(reader);
        }
        catch
        {
            throw;
        }
    }

    public async Task<Cliente?> ObterClientePorLogin(string login)
    {
        const string sql = @"
            SELECT
                cliente_id,
                nome,
                email,
                telefone,
                senha_hash,
                ativo,
                criado_em
            FROM
                rota_sabor.cliente
            WHERE
                LOWER(email) = LOWER(@login)
            OR
                telefone = @login
            LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("login", login);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return Map(reader);
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> VerificarClienteExiste(string email, string telefone)
    {
        const string sql = @"
            SELECT
                1
            FROM
                rota_sabor.cliente
            WHERE
                LOWER(email) = LOWER(@email)
            OR
                telefone = @telefone
            LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("telefone", telefone);

            var result = await cmd.ExecuteScalarAsync();

            return result != null;
        }
        catch
        {
            throw;
        }
    }

    public async Task<int> AtualizarCliente(ClienteAtualizado cliente, Guid clienteId)
    {
        const string sql = @"
            UPDATE
                rota_sabor.cliente
            SET
                nome = @nome,
                telefone = @telefone
            WHERE
                cliente_id = @cliente_id;
        ";

        try
        {
            await EnsureOpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("cliente_id", clienteId);
            cmd.Parameters.AddWithValue("nome", cliente.Nome);
            cmd.Parameters.AddWithValue("telefone", cliente.Telefone);

            return await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            throw;
        }
    }

    private static Cliente Map(NpgsqlDataReader reader)
    {
        return new Cliente
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

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
    }
}
