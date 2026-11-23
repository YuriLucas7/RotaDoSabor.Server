using Npgsql;
using RotaDoSabor.Server.Models;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public class EnderecoRepository : BaseRepository
{
    public EnderecoRepository(IDbConnection connection) : base(connection) { }

    public async Task<Guid> Cadastrar(Guid clienteId, EnderecoRequest request)
    {
        const string sql = """
            INSERT INTO rota_sabor.endereco
            (cliente_id, apelido, cep, logradouro, numero, complemento, bairro, cidade, estado, latitude, longitude, principal)
            VALUES (@cliente_id, @apelido, @cep, @logradouro, @numero, @complemento, @bairro, @cidade, @estado, @latitude, @longitude, @principal)
            RETURNING endereco_id;
            """;
        await EnsureOpenAsync();
        if (request.Principal)
        {
            await using var clear = new NpgsqlCommand("UPDATE rota_sabor.endereco SET principal=false WHERE cliente_id=@cliente_id", Connection);
            clear.Parameters.AddWithValue("cliente_id", clienteId);
            await clear.ExecuteNonQueryAsync();
        }
        await using var cmd = new NpgsqlCommand(sql, Connection);
        AddParameters(cmd, clienteId, request);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<List<Endereco>> ObterPorCliente(Guid clienteId)
    {
        const string sql = "SELECT * FROM rota_sabor.endereco WHERE cliente_id=@cliente_id ORDER BY principal DESC, apelido;";
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("cliente_id", clienteId);
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<Endereco>();
        while (await reader.ReadAsync()) result.Add(Map(reader));
        return result;
    }

    public async Task<Endereco?> Obter(Guid enderecoId)
    {
        const string sql = "SELECT * FROM rota_sabor.endereco WHERE endereco_id=@id LIMIT 1;";
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", enderecoId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<int> Excluir(Guid enderecoId)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM rota_sabor.endereco WHERE endereco_id=@id", Connection);
        cmd.Parameters.AddWithValue("id", enderecoId);
        return await cmd.ExecuteNonQueryAsync();
    }

    private static void AddParameters(NpgsqlCommand cmd, Guid clienteId, EnderecoRequest r)
    {
        cmd.Parameters.AddWithValue("cliente_id", clienteId);
        cmd.Parameters.AddWithValue("apelido", r.Apelido);
        cmd.Parameters.AddWithValue("cep", r.Cep);
        cmd.Parameters.AddWithValue("logradouro", r.Logradouro);
        cmd.Parameters.AddWithValue("numero", r.Numero);
        cmd.Parameters.AddWithValue("complemento", r.Complemento ?? string.Empty);
        cmd.Parameters.AddWithValue("bairro", r.Bairro);
        cmd.Parameters.AddWithValue("cidade", r.Cidade);
        cmd.Parameters.AddWithValue("estado", r.Estado);
        cmd.Parameters.AddWithValue("latitude", (object?)r.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("longitude", (object?)r.Longitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("principal", r.Principal);
    }

    private static Endereco Map(NpgsqlDataReader reader) => new()
    {
        EnderecoId = reader.GetGuid("endereco_id"), ClienteId = reader.GetGuid("cliente_id"),
        Apelido = reader.GetString("apelido"), Cep = reader.GetString("cep"), Logradouro = reader.GetString("logradouro"),
        Numero = reader.GetString("numero"), Complemento = reader.GetSafeString("complemento"), Bairro = reader.GetString("bairro"),
        Cidade = reader.GetString("cidade"), Estado = reader.GetString("estado"), Latitude = reader.GetNullableDecimal("latitude"),
        Longitude = reader.GetNullableDecimal("longitude"), Principal = reader.GetBoolean("principal")
    };
}
