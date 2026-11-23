using Npgsql;
using RotaDoSabor.Models;
using System.Data;

namespace RotaDoSabor.Repositories;

public class AvaliacaoRepository
{
    private readonly NpgsqlConnection _connection;

    public AvaliacaoRepository(IDbConnection connection)
    {
        _connection = (NpgsqlConnection)connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Guid> CadastrarAvaliacao(Pedido pedido, Avaliacao avaliacao)
    {
        const string sql = @"
            INSERT INTO rota_sabor.avaliacao
            (
                pedido_id,
                cliente_id,
                restaurante_id,
                nota_restaurante,
                nota_entrega,
                nota_comida,
                comentario
            )
            VALUES
            (
                @pedido_id,
                @cliente_id,
                @restaurante_id,
                @nota_restaurante,
                @nota_entrega,
                @nota_comida,
                @comentario
            )
            RETURNING avaliacao_id;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("pedido_id", pedido.PedidoId);
            command.Parameters.AddWithValue("cliente_id", pedido.ClienteId);
            command.Parameters.AddWithValue("restaurante_id", pedido.RestauranteId);
            command.Parameters.AddWithValue("nota_restaurante", avaliacao.NotaRestaurante);
            command.Parameters.AddWithValue("nota_entrega", avaliacao.NotaEntrega);
            command.Parameters.AddWithValue("nota_comida", avaliacao.NotaComida);
            command.Parameters.AddWithValue("comentario", avaliacao.Comentario);

            return (Guid)(await command.ExecuteScalarAsync())!;
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> VerificarPedidoAvaliado(Guid pedidoId)
    {
        const string sql = @"
            SELECT 1
              FROM rota_sabor.avaliacao
             WHERE pedido_id = @pedido_id
             LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("pedido_id", pedidoId);

            return await command.ExecuteScalarAsync() is not null;
        }
        catch
        {
            throw;
        }
    }

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
    }
}
