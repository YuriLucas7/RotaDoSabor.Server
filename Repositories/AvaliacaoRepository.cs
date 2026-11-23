using Npgsql;
using RotaDoSabor.Server.Models;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public class AvaliacaoRepository : BaseRepository
{
    public AvaliacaoRepository(IDbConnection connection) : base(connection) { }

    public async Task<Guid> Cadastrar(Pedido pedido, AvaliacaoRequest request)
    {
        const string sql = """
            INSERT INTO rota_sabor.avaliacao
            (pedido_id,cliente_id,restaurante_id,nota_restaurante,nota_entrega,nota_comida,comentario)
            VALUES (@pedido_id,@cliente_id,@restaurante_id,@restaurante,@entrega,@comida,@comentario)
            RETURNING avaliacao_id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("pedido_id", pedido.PedidoId); cmd.Parameters.AddWithValue("cliente_id", pedido.ClienteId);
        cmd.Parameters.AddWithValue("restaurante_id", pedido.RestauranteId); cmd.Parameters.AddWithValue("restaurante", request.NotaRestaurante);
        cmd.Parameters.AddWithValue("entrega", request.NotaEntrega); cmd.Parameters.AddWithValue("comida", request.NotaComida);
        cmd.Parameters.AddWithValue("comentario", request.Comentario ?? string.Empty);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<bool> PedidoAvaliado(Guid pedidoId)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT 1 FROM rota_sabor.avaliacao WHERE pedido_id=@id LIMIT 1", Connection);
        cmd.Parameters.AddWithValue("id", pedidoId);
        return await cmd.ExecuteScalarAsync() is not null;
    }
}
