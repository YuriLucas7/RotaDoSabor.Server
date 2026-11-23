using Npgsql;
using RotaDoSabor.Server.Models;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public class ProdutoRepository : BaseRepository
{
    public ProdutoRepository(IDbConnection connection) : base(connection) { }

    public async Task<Guid> Cadastrar(Guid restauranteId, ProdutoRequest r)
    {
        const string sql = """
            INSERT INTO rota_sabor.produto
            (restaurante_id,nome,descricao,categoria,imagem_url,preco,preco_promocional,disponivel,ordem)
            VALUES (@restaurante_id,@nome,@descricao,@categoria,@imagem_url,@preco,@preco_promocional,@disponivel,@ordem)
            RETURNING produto_id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        AddParameters(cmd, restauranteId, r);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<List<Produto>> ObterPorRestaurante(Guid restauranteId, bool somenteDisponiveis)
    {
        const string sql = """
            SELECT * FROM rota_sabor.produto
            WHERE restaurante_id=@restaurante_id AND (@somente=false OR disponivel=true)
            ORDER BY categoria, ordem, nome;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("restaurante_id", restauranteId);
        cmd.Parameters.AddWithValue("somente", somenteDisponiveis);
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<Produto>();
        while (await reader.ReadAsync()) result.Add(Map(reader));
        return result;
    }

    public async Task<Produto?> Obter(Guid produtoId)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT * FROM rota_sabor.produto WHERE produto_id=@id LIMIT 1", Connection);
        cmd.Parameters.AddWithValue("id", produtoId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<int> Atualizar(Guid produtoId, ProdutoRequest r)
    {
        const string sql = """
            UPDATE rota_sabor.produto SET nome=@nome,descricao=@descricao,categoria=@categoria,imagem_url=@imagem_url,
            preco=@preco,preco_promocional=@preco_promocional,disponivel=@disponivel,ordem=@ordem WHERE produto_id=@produto_id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        AddParameters(cmd, Guid.Empty, r);
        cmd.Parameters.RemoveAt("restaurante_id");
        cmd.Parameters.AddWithValue("produto_id", produtoId);
        return await cmd.ExecuteNonQueryAsync();
    }

    private static void AddParameters(NpgsqlCommand cmd, Guid restauranteId, ProdutoRequest r)
    {
        cmd.Parameters.AddWithValue("restaurante_id", restauranteId); cmd.Parameters.AddWithValue("nome", r.Nome);
        cmd.Parameters.AddWithValue("descricao", r.Descricao); cmd.Parameters.AddWithValue("categoria", r.Categoria);
        cmd.Parameters.AddWithValue("imagem_url", r.ImagemUrl ?? string.Empty); cmd.Parameters.AddWithValue("preco", r.Preco);
        cmd.Parameters.AddWithValue("preco_promocional", (object?)r.PrecoPromocional ?? DBNull.Value);
        cmd.Parameters.AddWithValue("disponivel", r.Disponivel); cmd.Parameters.AddWithValue("ordem", r.Ordem);
    }

    private static Produto Map(NpgsqlDataReader r) => new()
    {
        ProdutoId=r.GetGuid("produto_id"), RestauranteId=r.GetGuid("restaurante_id"), Nome=r.GetString("nome"),
        Descricao=r.GetSafeString("descricao"), Categoria=r.GetString("categoria"), ImagemUrl=r.GetSafeString("imagem_url"),
        Preco=r.GetDecimal("preco"), PrecoPromocional=r.GetNullableDecimal("preco_promocional"),
        Disponivel=r.GetBoolean("disponivel"), Ordem=r.GetInt32("ordem")
    };
}
