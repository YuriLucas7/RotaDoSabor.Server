using Npgsql;
using RotaDoSabor.Models;
using System.Data;

namespace RotaDoSabor.Repositories;

public class ProdutoRepository
{
    private readonly NpgsqlConnection _connection;

    public ProdutoRepository(IDbConnection connection)
    {
        _connection = (NpgsqlConnection)connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Guid> CadastrarProduto(Produto produto)
    {
        const string sql = @"
            INSERT INTO rota_sabor.produto
            (
                restaurante_id,
                nome,
                descricao,
                categoria,
                imagem_url,
                preco,
                preco_promocional,
                disponivel,
                ordem
            )
            VALUES
            (
                @restaurante_id,
                @nome,
                @descricao,
                @categoria,
                @imagem_url,
                @preco,
                @preco_promocional,
                @disponivel,
                @ordem
            )
            RETURNING produto_id;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            AdicionarParametros(command, produto);

            return (Guid)(await command.ExecuteScalarAsync())!;
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<Produto>> ObterProdutosPorRestaurante(Guid restauranteId, bool somenteDisponiveis)
    {
        const string sql = @"
            SELECT produto_id,
                   restaurante_id,
                   nome,
                   descricao,
                   categoria,
                   imagem_url,
                   preco,
                   preco_promocional,
                   disponivel,
                   ordem
              FROM rota_sabor.produto
             WHERE restaurante_id = @restaurante_id
               AND (@somente_disponiveis = false OR disponivel = true)
             ORDER BY categoria,
                      ordem,
                      nome;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("restaurante_id", restauranteId);
            command.Parameters.AddWithValue("somente_disponiveis", somenteDisponiveis);
            await using var reader = await command.ExecuteReaderAsync();
            var produtos = new List<Produto>();

            while (await reader.ReadAsync())
                produtos.Add(MapearProduto(reader));

            return produtos;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Produto?> ObterProduto(Guid produtoId)
    {
        const string sql = @"
            SELECT produto_id,
                   restaurante_id,
                   nome,
                   descricao,
                   categoria,
                   imagem_url,
                   preco,
                   preco_promocional,
                   disponivel,
                   ordem
              FROM rota_sabor.produto
             WHERE produto_id = @produto_id
             LIMIT 1;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddWithValue("produto_id", produtoId);
            await using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapearProduto(reader) : null;
        }
        catch
        {
            throw;
        }
    }

    public async Task<int> AtualizarProduto(Produto produto, Guid produtoId)
    {
        const string sql = @"
            UPDATE rota_sabor.produto
               SET nome = @nome,
                   descricao = @descricao,
                   categoria = @categoria,
                   imagem_url = @imagem_url,
                   preco = @preco,
                   preco_promocional = @preco_promocional,
                   disponivel = @disponivel,
                   ordem = @ordem
             WHERE produto_id = @produto_id;
        ";

        try
        {
            await EnsureOpenAsync();
            await using var command = new NpgsqlCommand(sql, _connection);
            AdicionarParametros(command, produto, false);
            command.Parameters.AddWithValue("produto_id", produtoId);

            return await command.ExecuteNonQueryAsync();
        }
        catch
        {
            throw;
        }
    }

    private static void AdicionarParametros(NpgsqlCommand command, Produto produto, bool incluirRestaurante = true)
    {
        if (incluirRestaurante)
            command.Parameters.AddWithValue("restaurante_id", produto.RestauranteId);

        command.Parameters.AddWithValue("nome", produto.Nome);
        command.Parameters.AddWithValue("descricao", produto.Descricao);
        command.Parameters.AddWithValue("categoria", produto.Categoria);
        command.Parameters.AddWithValue("imagem_url", produto.ImagemUrl);
        command.Parameters.AddWithValue("preco", produto.Preco);
        command.Parameters.AddWithValue("preco_promocional", (object?)produto.PrecoPromocional ?? DBNull.Value);
        command.Parameters.AddWithValue("disponivel", produto.Disponivel);
        command.Parameters.AddWithValue("ordem", produto.Ordem);
    }

    private static Produto MapearProduto(NpgsqlDataReader reader)
    {
        return new Produto
        {
            ProdutoId = reader.GetGuid("produto_id"),
            RestauranteId = reader.GetGuid("restaurante_id"),
            Nome = reader.GetString("nome"),
            Descricao = reader.GetSafeString("descricao"),
            Categoria = reader.GetString("categoria"),
            ImagemUrl = reader.GetSafeString("imagem_url"),
            Preco = reader.GetDecimal("preco"),
            PrecoPromocional = reader.GetNullableDecimal("preco_promocional"),
            Disponivel = reader.GetBoolean("disponivel"),
            Ordem = reader.GetInt32("ordem")
        };
    }

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();
    }
}
