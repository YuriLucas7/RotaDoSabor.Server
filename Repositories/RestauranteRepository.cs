using Npgsql;
using RotaDoSabor.Server.Models;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public class RestauranteRepository : BaseRepository
{
    public RestauranteRepository(IDbConnection connection) : base(connection) { }

    public async Task<Guid> Cadastrar(RestauranteRequest r)
    {
        const string sql = """
            INSERT INTO rota_sabor.restaurante
            (nome, descricao, cnpj, telefone, email, imagem_url, categoria, pedido_minimo, taxa_entrega,
             tempo_entrega_minimo, tempo_entrega_maximo, status, aberto, possui_oferta)
            VALUES (@nome,@descricao,@cnpj,@telefone,@email,@imagem_url,@categoria,@pedido_minimo,@taxa_entrega,
                    @tempo_min,@tempo_max,@status,false,false)
            RETURNING restaurante_id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        AddParameters(cmd, r);
        cmd.Parameters.AddWithValue("status", (int)StatusRestaurante.Pendente);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<List<Restaurante>> ObterTodos(string? busca, bool? aberto, bool? ofertas)
    {
        const string sql = """
            SELECT r.*, COALESCE(AVG(a.nota_restaurante),0) avaliacao_media
            FROM rota_sabor.restaurante r
            LEFT JOIN rota_sabor.avaliacao a ON a.restaurante_id=r.restaurante_id
            WHERE r.status=@ativo
              AND (@busca='' OR lower(r.nome) LIKE '%'||lower(@busca)||'%' OR lower(r.categoria) LIKE '%'||lower(@busca)||'%')
              AND (@filtrar_aberto=false OR r.aberto=@aberto)
              AND (@filtrar_oferta=false OR r.possui_oferta=@oferta)
            GROUP BY r.restaurante_id
            ORDER BY r.possui_oferta DESC, avaliacao_media DESC, r.nome;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("ativo", (int)StatusRestaurante.Ativo);
        cmd.Parameters.AddWithValue("busca", busca ?? string.Empty);
        cmd.Parameters.AddWithValue("filtrar_aberto", aberto.HasValue);
        cmd.Parameters.AddWithValue("aberto", aberto ?? false);
        cmd.Parameters.AddWithValue("filtrar_oferta", ofertas.HasValue);
        cmd.Parameters.AddWithValue("oferta", ofertas ?? false);
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<Restaurante>();
        while (await reader.ReadAsync()) result.Add(Map(reader));
        return result;
    }

    public async Task<Restaurante?> Obter(Guid id)
    {
        const string sql = """
            SELECT r.*, COALESCE(AVG(a.nota_restaurante),0) avaliacao_media
            FROM rota_sabor.restaurante r
            LEFT JOIN rota_sabor.avaliacao a ON a.restaurante_id=r.restaurante_id
            WHERE r.restaurante_id=@id GROUP BY r.restaurante_id LIMIT 1;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<bool> CnpjExiste(string cnpj)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1 FROM rota_sabor.restaurante WHERE cnpj=@cnpj LIMIT 1", Connection);
        cmd.Parameters.AddWithValue("cnpj", cnpj);
        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task<int> AtualizarStatus(Guid id, StatusRestaurante status, bool aberto)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand("UPDATE rota_sabor.restaurante SET status=@status, aberto=@aberto WHERE restaurante_id=@id", Connection);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("status", (int)status);
        cmd.Parameters.AddWithValue("aberto", aberto);
        return await cmd.ExecuteNonQueryAsync();
    }

    private static void AddParameters(NpgsqlCommand cmd, RestauranteRequest r)
    {
        cmd.Parameters.AddWithValue("nome", r.Nome); cmd.Parameters.AddWithValue("descricao", r.Descricao);
        cmd.Parameters.AddWithValue("cnpj", r.Cnpj); cmd.Parameters.AddWithValue("telefone", r.Telefone);
        cmd.Parameters.AddWithValue("email", r.Email); cmd.Parameters.AddWithValue("imagem_url", r.ImagemUrl ?? string.Empty);
        cmd.Parameters.AddWithValue("categoria", r.Categoria); cmd.Parameters.AddWithValue("pedido_minimo", r.PedidoMinimo);
        cmd.Parameters.AddWithValue("taxa_entrega", r.TaxaEntrega); cmd.Parameters.AddWithValue("tempo_min", r.TempoEntregaMinimo);
        cmd.Parameters.AddWithValue("tempo_max", r.TempoEntregaMaximo);
    }

    private static Restaurante Map(NpgsqlDataReader r) => new()
    {
        RestauranteId=r.GetGuid("restaurante_id"), Nome=r.GetString("nome"), Descricao=r.GetSafeString("descricao"),
        Cnpj=r.GetString("cnpj"), Telefone=r.GetString("telefone"), Email=r.GetString("email"), ImagemUrl=r.GetSafeString("imagem_url"),
        Categoria=r.GetString("categoria"), PedidoMinimo=r.GetDecimal("pedido_minimo"), TaxaEntrega=r.GetDecimal("taxa_entrega"),
        TempoEntregaMinimo=r.GetInt32("tempo_entrega_minimo"), TempoEntregaMaximo=r.GetInt32("tempo_entrega_maximo"),
        AvaliacaoMedia=r.GetDecimal("avaliacao_media"), Aberto=r.GetBoolean("aberto"), Status=(StatusRestaurante)r.GetInt32("status"),
        PossuiOferta=r.GetBoolean("possui_oferta")
    };
}
