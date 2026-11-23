using Npgsql;
using RotaDoSabor.Models;
using System.Data;

namespace RotaDoSabor.Repositories;

public class RestauranteRepository
{
    private readonly NpgsqlConnection _connection;

    public RestauranteRepository(IDbConnection connection)
    {
        _connection = (NpgsqlConnection)connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Guid> CadastrarRestaurante(Restaurante restaurante)
    {
        const string sql = @"
            INSERT INTO rota_sabor.restaurante
            (
                nome,
                descricao,
                cnpj,
                telefone,
                email,
                imagem_url,
                categoria,
                pedido_minimo,
                taxa_entrega,
                tempo_entrega_minimo,
                tempo_entrega_maximo,
                status,
                aberto,
                possui_oferta
            )
            VALUES
            (
                @nome,
                @descricao,
                @cnpj,
                @telefone,
                @email,
                @imagem_url,
                @categoria,
                @pedido_minimo,
                @taxa_entrega,
                @tempo_entrega_minimo,
                @tempo_entrega_maximo,
                @status,
                false,
                false
            )
            RETURNING restaurante_id;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            AdicionarParametros(command, restaurante);
            command.Parameters.AddWithValue("status", (int)StatusRestaurante.Pendente);

            return (Guid)(await command.ExecuteScalarAsync())!;
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<Restaurante>> ObterRestaurantes(string? busca, bool? aberto, bool? ofertas)
    {
        const string sql = @"
            SELECT r.*,
                   COALESCE(AVG(a.nota_restaurante), 0) AS avaliacao_media
              FROM rota_sabor.restaurante r
              LEFT JOIN rota_sabor.avaliacao a
                ON a.restaurante_id = r.restaurante_id
             WHERE r.status = @status_ativo
               AND (@busca = '' OR LOWER(r.nome) LIKE '%' || LOWER(@busca) || '%'
                    OR LOWER(r.categoria) LIKE '%' || LOWER(@busca) || '%')
               AND (@filtrar_aberto = false OR r.aberto = @aberto)
               AND (@filtrar_oferta = false OR r.possui_oferta = @oferta)
             GROUP BY r.restaurante_id
             ORDER BY r.possui_oferta DESC,
                      avaliacao_media DESC,
                      r.nome;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("status_ativo", (int)StatusRestaurante.Ativo);
            command.Parameters.AddWithValue("busca", busca ?? string.Empty);
            command.Parameters.AddWithValue("filtrar_aberto", aberto.HasValue);
            command.Parameters.AddWithValue("aberto", aberto ?? false);
            command.Parameters.AddWithValue("filtrar_oferta", ofertas.HasValue);
            command.Parameters.AddWithValue("oferta", ofertas ?? false);
            await using var reader = await command.ExecuteReaderAsync();
            var restaurantes = new List<Restaurante>();

            while (await reader.ReadAsync())
                restaurantes.Add(MapearRestaurante(reader));

            return restaurantes;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Restaurante?> ObterRestaurante(Guid restauranteId)
    {
        const string sql = @"
            SELECT r.*,
                   COALESCE(AVG(a.nota_restaurante), 0) AS avaliacao_media
              FROM rota_sabor.restaurante r
              LEFT JOIN rota_sabor.avaliacao a
                ON a.restaurante_id = r.restaurante_id
             WHERE r.restaurante_id = @restaurante_id
             GROUP BY r.restaurante_id
             LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("restaurante_id", restauranteId);
            await using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapearRestaurante(reader) : null;
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> VerificarRestauranteExiste(string cnpj)
    {
        const string sql = @"
            SELECT 1
              FROM rota_sabor.restaurante
             WHERE cnpj = @cnpj
             LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("cnpj", cnpj);

            return await command.ExecuteScalarAsync() is not null;
        }
        catch
        {
            throw;
        }
    }

    public async Task<int> AtualizarStatusRestaurante(Guid restauranteId, StatusRestaurante status, bool aberto)
    {
        const string sql = @"
            UPDATE rota_sabor.restaurante
               SET status = @status,
                   aberto = @aberto
             WHERE restaurante_id = @restaurante_id;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("restaurante_id", restauranteId);
            command.Parameters.AddWithValue("status", (int)status);
            command.Parameters.AddWithValue("aberto", aberto);

            return await command.ExecuteNonQueryAsync();
        }
        catch
        {
            throw;
        }
    }

    private static void AdicionarParametros(NpgsqlCommand command, Restaurante restaurante)
    {
        command.Parameters.AddWithValue("nome", restaurante.Nome);
        command.Parameters.AddWithValue("descricao", restaurante.Descricao);
        command.Parameters.AddWithValue("cnpj", restaurante.Cnpj);
        command.Parameters.AddWithValue("telefone", restaurante.Telefone);
        command.Parameters.AddWithValue("email", restaurante.Email);
        command.Parameters.AddWithValue("imagem_url", restaurante.ImagemUrl);
        command.Parameters.AddWithValue("categoria", restaurante.Categoria);
        command.Parameters.AddWithValue("pedido_minimo", restaurante.PedidoMinimo);
        command.Parameters.AddWithValue("taxa_entrega", restaurante.TaxaEntrega);
        command.Parameters.AddWithValue("tempo_entrega_minimo", restaurante.TempoEntregaMinimo);
        command.Parameters.AddWithValue("tempo_entrega_maximo", restaurante.TempoEntregaMaximo);
    }

    private static Restaurante MapearRestaurante(NpgsqlDataReader reader)
    {
        return new Restaurante
        {
            RestauranteId = reader.GetGuid("restaurante_id"),
            Nome = reader.GetString("nome"),
            Descricao = reader.GetSafeString("descricao"),
            Cnpj = reader.GetString("cnpj"),
            Telefone = reader.GetString("telefone"),
            Email = reader.GetString("email"),
            ImagemUrl = reader.GetSafeString("imagem_url"),
            Categoria = reader.GetString("categoria"),
            PedidoMinimo = reader.GetDecimal("pedido_minimo"),
            TaxaEntrega = reader.GetDecimal("taxa_entrega"),
            TempoEntregaMinimo = reader.GetInt32("tempo_entrega_minimo"),
            TempoEntregaMaximo = reader.GetInt32("tempo_entrega_maximo"),
            AvaliacaoMedia = reader.GetDecimal("avaliacao_media"),
            Aberto = reader.GetBoolean("aberto"),
            Status = (StatusRestaurante)reader.GetInt32("status"),
            PossuiOferta = reader.GetBoolean("possui_oferta")
        };
    }

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
    }
}
