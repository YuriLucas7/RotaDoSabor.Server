using Npgsql;
using RotaDoSabor.Models;
using System.Data;

namespace RotaDoSabor.Repositories;

public class EnderecoRepository
{
    private readonly NpgsqlConnection _connection;

    public EnderecoRepository(IDbConnection connection)
    {
        _connection = (NpgsqlConnection)connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Guid> CadastrarEndereco(Endereco endereco)
    {
        const string sql = @"
            INSERT INTO rota_sabor.endereco
            (
                cliente_id,
                apelido,
                cep,
                logradouro,
                numero,
                complemento,
                bairro,
                cidade,
                estado,
                latitude,
                longitude,
                principal
            )
            VALUES
            (
                @cliente_id,
                @apelido,
                @cep,
                @logradouro,
                @numero,
                @complemento,
                @bairro,
                @cidade,
                @estado,
                @latitude,
                @longitude,
                @principal
            )
            RETURNING endereco_id;
        ";

        try
        {
            await EnsureOpenAsync();

            if (endereco.Principal)
            {
                const string limparPrincipalSql = @"
                    UPDATE rota_sabor.endereco
                       SET principal = false
                     WHERE cliente_id = @cliente_id;
                ";

                await using var limparPrincipal = new NpgsqlCommand(limparPrincipalSql, _connection);
                limparPrincipal.Parameters.AddWithValue("cliente_id", endereco.ClienteId);
                await limparPrincipal.ExecuteNonQueryAsync();
            }

            await using var command = new NpgsqlCommand(sql, _connection);
            AdicionarParametros(command, endereco);

            return (Guid)(await command.ExecuteScalarAsync())!;
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<Endereco>> ObterEnderecosPorCliente(Guid clienteId)
    {
        const string sql = @"
            SELECT endereco_id,
                   cliente_id,
                   apelido,
                   cep,
                   logradouro,
                   numero,
                   complemento,
                   bairro,
                   cidade,
                   estado,
                   latitude,
                   longitude,
                   principal
              FROM rota_sabor.endereco
             WHERE cliente_id = @cliente_id
             ORDER BY principal DESC,
                      apelido;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("cliente_id", clienteId);
            await using var reader = await command.ExecuteReaderAsync();
            var enderecos = new List<Endereco>();

            while (await reader.ReadAsync())
                enderecos.Add(MapearEndereco(reader));

            return enderecos;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Endereco?> ObterEndereco(Guid enderecoId)
    {
        const string sql = @"
            SELECT endereco_id,
                   cliente_id,
                   apelido,
                   cep,
                   logradouro,
                   numero,
                   complemento,
                   bairro,
                   cidade,
                   estado,
                   latitude,
                   longitude,
                   principal
              FROM rota_sabor.endereco
             WHERE endereco_id = @endereco_id
             LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("endereco_id", enderecoId);
            await using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapearEndereco(reader) : null;
        }
        catch
        {
            throw;
        }
    }

    public async Task<int> ExcluirEndereco(Guid enderecoId)
    {
        const string sql = @"
            DELETE FROM rota_sabor.endereco
                  WHERE endereco_id = @endereco_id;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("endereco_id", enderecoId);

            return await command.ExecuteNonQueryAsync();
        }
        catch
        {
            throw;
        }
    }

    private static void AdicionarParametros(NpgsqlCommand command, Endereco endereco)
    {
        command.Parameters.AddWithValue("cliente_id", endereco.ClienteId);
        command.Parameters.AddWithValue("apelido", endereco.Apelido);
        command.Parameters.AddWithValue("cep", endereco.Cep);
        command.Parameters.AddWithValue("logradouro", endereco.Logradouro);
        command.Parameters.AddWithValue("numero", endereco.Numero);
        command.Parameters.AddWithValue("complemento", endereco.Complemento);
        command.Parameters.AddWithValue("bairro", endereco.Bairro);
        command.Parameters.AddWithValue("cidade", endereco.Cidade);
        command.Parameters.AddWithValue("estado", endereco.Estado);
        command.Parameters.AddWithValue("latitude", (object?)endereco.Latitude ?? DBNull.Value);
        command.Parameters.AddWithValue("longitude", (object?)endereco.Longitude ?? DBNull.Value);
        command.Parameters.AddWithValue("principal", endereco.Principal);
    }

    private static Endereco MapearEndereco(NpgsqlDataReader reader)
    {
        return new Endereco
        {
            EnderecoId = reader.GetGuid("endereco_id"),
            ClienteId = reader.GetGuid("cliente_id"),
            Apelido = reader.GetString("apelido"),
            Cep = reader.GetString("cep"),
            Logradouro = reader.GetString("logradouro"),
            Numero = reader.GetString("numero"),
            Complemento = reader.GetSafeString("complemento"),
            Bairro = reader.GetString("bairro"),
            Cidade = reader.GetString("cidade"),
            Estado = reader.GetString("estado"),
            Latitude = reader.GetNullableDecimal("latitude"),
            Longitude = reader.GetNullableDecimal("longitude"),
            Principal = reader.GetBoolean("principal")
        };
    }

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
    }
}
