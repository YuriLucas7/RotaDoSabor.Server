using Npgsql;
using RotaDoSabor.Server.Models;
using System.Data;

namespace RotaDoSabor.Server.Repositories;

public class PedidoRepository : BaseRepository
{
    public PedidoRepository(IDbConnection connection) : base(connection) { }

    public async Task<Guid> Cadastrar(PedidoRequest request, IReadOnlyList<(Produto Produto, PedidoItemRequest Item)> itens, decimal taxaEntrega)
    {
        await EnsureOpenAsync();
        await using var tx = await Connection.BeginTransactionAsync();
        try
        {
            var subtotal = itens.Sum(x => (x.Produto.PrecoPromocional ?? x.Produto.Preco) * x.Item.Quantidade);
            var total = subtotal + (request.TipoRecebimento == TipoRecebimento.Entrega ? taxaEntrega : 0);
            const string pedidoSql = """
                INSERT INTO rota_sabor.pedido
                (cliente_id,restaurante_id,endereco_id,tipo_recebimento,status,subtotal,taxa_entrega,desconto,total,observacao)
                VALUES (@cliente_id,@restaurante_id,@endereco_id,@tipo_recebimento,@status,@subtotal,@taxa_entrega,0,@total,@observacao)
                RETURNING pedido_id;
                """;
            await using var pedidoCmd = new NpgsqlCommand(pedidoSql, Connection, tx);
            pedidoCmd.Parameters.AddWithValue("cliente_id", request.ClienteId);
            pedidoCmd.Parameters.AddWithValue("restaurante_id", request.RestauranteId);
            pedidoCmd.Parameters.AddWithValue("endereco_id", (object?)request.EnderecoId ?? DBNull.Value);
            pedidoCmd.Parameters.AddWithValue("tipo_recebimento", (int)request.TipoRecebimento);
            pedidoCmd.Parameters.AddWithValue("status", (int)StatusPedido.AguardandoPagamento);
            pedidoCmd.Parameters.AddWithValue("subtotal", subtotal);
            pedidoCmd.Parameters.AddWithValue("taxa_entrega", request.TipoRecebimento == TipoRecebimento.Entrega ? taxaEntrega : 0);
            pedidoCmd.Parameters.AddWithValue("total", total);
            pedidoCmd.Parameters.AddWithValue("observacao", request.Observacao ?? string.Empty);
            var pedidoId = (Guid)(await pedidoCmd.ExecuteScalarAsync())!;

            const string itemSql = """
                INSERT INTO rota_sabor.pedido_item
                (pedido_id,produto_id,nome_produto,quantidade,preco_unitario,total,observacao)
                VALUES (@pedido_id,@produto_id,@nome,@quantidade,@preco,@total,@observacao);
                """;
            foreach (var (produto, item) in itens)
            {
                var preco = produto.PrecoPromocional ?? produto.Preco;
                await using var itemCmd = new NpgsqlCommand(itemSql, Connection, tx);
                itemCmd.Parameters.AddWithValue("pedido_id", pedidoId);
                itemCmd.Parameters.AddWithValue("produto_id", produto.ProdutoId);
                itemCmd.Parameters.AddWithValue("nome", produto.Nome);
                itemCmd.Parameters.AddWithValue("quantidade", item.Quantidade);
                itemCmd.Parameters.AddWithValue("preco", preco);
                itemCmd.Parameters.AddWithValue("total", preco * item.Quantidade);
                itemCmd.Parameters.AddWithValue("observacao", item.Observacao ?? string.Empty);
                await itemCmd.ExecuteNonQueryAsync();
            }

            const string pagamentoSql = """
                INSERT INTO rota_sabor.pagamento (pedido_id,forma,status,valor)
                VALUES (@pedido_id,@forma,@status,@valor);
                """;
            await using var pagamentoCmd = new NpgsqlCommand(pagamentoSql, Connection, tx);
            pagamentoCmd.Parameters.AddWithValue("pedido_id", pedidoId);
            pagamentoCmd.Parameters.AddWithValue("forma", (int)request.FormaPagamento);
            pagamentoCmd.Parameters.AddWithValue("status", (int)StatusPagamento.Pendente);
            pagamentoCmd.Parameters.AddWithValue("valor", total);
            await pagamentoCmd.ExecuteNonQueryAsync();

            if (request.TipoRecebimento == TipoRecebimento.Entrega)
            {
                await using var entregaCmd = new NpgsqlCommand(
                    "INSERT INTO rota_sabor.entrega (pedido_id,status,codigo_confirmacao) VALUES (@pedido_id,@status,@codigo)", Connection, tx);
                entregaCmd.Parameters.AddWithValue("pedido_id", pedidoId);
                entregaCmd.Parameters.AddWithValue("status", (int)StatusEntrega.AguardandoEntregador);
                entregaCmd.Parameters.AddWithValue("codigo", Random.Shared.Next(1000, 10000).ToString());
                await entregaCmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return pedidoId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<Pedido?> Obter(Guid pedidoId)
    {
        await EnsureOpenAsync();
        const string sql = "SELECT * FROM rota_sabor.pedido WHERE pedido_id=@id LIMIT 1";
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", pedidoId);
        Pedido? pedido;
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            pedido = await reader.ReadAsync() ? MapPedido(reader) : null;
        }
        if (pedido is null) return null;

        pedido.Itens = await ObterItens(pedidoId);
        pedido.Pagamento = await ObterPagamento(pedidoId);
        pedido.Entrega = await ObterEntrega(pedidoId);
        return pedido;
    }

    public async Task<List<Pedido>> ObterPorCliente(Guid clienteId)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT * FROM rota_sabor.pedido WHERE cliente_id=@id ORDER BY criado_em DESC", Connection);
        cmd.Parameters.AddWithValue("id", clienteId);
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<Pedido>();
        while (await reader.ReadAsync()) result.Add(MapPedido(reader));
        return result;
    }

    public async Task<int> AtualizarStatus(Guid pedidoId, StatusPedido status, string? motivo)
    {
        const string sql = """
            UPDATE rota_sabor.pedido SET status=@status, motivo_cancelamento=@motivo, atualizado_em=now()
            WHERE pedido_id=@id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", pedidoId);
        cmd.Parameters.AddWithValue("status", (int)status);
        cmd.Parameters.AddWithValue("motivo", (object?)motivo ?? DBNull.Value);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> AtualizarPagamento(Guid pedidoId, StatusPagamento status, string? transacaoId)
    {
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE rota_sabor.pagamento SET status=@status, transacao_externa_id=@transacao WHERE pedido_id=@id", Connection);
        cmd.Parameters.AddWithValue("id", pedidoId);
        cmd.Parameters.AddWithValue("status", (int)status);
        cmd.Parameters.AddWithValue("transacao", transacaoId ?? string.Empty);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> AtualizarEntrega(Guid pedidoId, AtualizarEntregaRequest request)
    {
        const string sql = """
            UPDATE rota_sabor.entrega SET status=@status, latitude_atual=@latitude, longitude_atual=@longitude,
            coletado_em=CASE WHEN @status=@coletado THEN COALESCE(coletado_em,now()) ELSE coletado_em END,
            entregue_em=CASE WHEN @status=@entregue THEN COALESCE(entregue_em,now()) ELSE entregue_em END
            WHERE pedido_id=@id;
            """;
        await EnsureOpenAsync();
        await using var cmd = new NpgsqlCommand(sql, Connection);
        cmd.Parameters.AddWithValue("id", pedidoId);
        cmd.Parameters.AddWithValue("status", (int)request.Status);
        cmd.Parameters.AddWithValue("coletado", (int)StatusEntrega.Coletado);
        cmd.Parameters.AddWithValue("entregue", (int)StatusEntrega.Entregue);
        cmd.Parameters.AddWithValue("latitude", (object?)request.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("longitude", (object?)request.Longitude ?? DBNull.Value);
        return await cmd.ExecuteNonQueryAsync();
    }

    private async Task<List<PedidoItem>> ObterItens(Guid id)
    {
        await using var cmd = new NpgsqlCommand("SELECT * FROM rota_sabor.pedido_item WHERE pedido_id=@id ORDER BY nome_produto", Connection);
        cmd.Parameters.AddWithValue("id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        var result = new List<PedidoItem>();
        while (await r.ReadAsync()) result.Add(new PedidoItem
        {
            PedidoItemId=r.GetGuid("pedido_item_id"), PedidoId=r.GetGuid("pedido_id"), ProdutoId=r.GetGuid("produto_id"),
            NomeProduto=r.GetString("nome_produto"), Quantidade=r.GetInt32("quantidade"), PrecoUnitario=r.GetDecimal("preco_unitario"),
            Total=r.GetDecimal("total"), Observacao=r.GetSafeString("observacao")
        });
        return result;
    }

    private async Task<Pagamento?> ObterPagamento(Guid id)
    {
        await using var cmd = new NpgsqlCommand("SELECT * FROM rota_sabor.pagamento WHERE pedido_id=@id LIMIT 1", Connection);
        cmd.Parameters.AddWithValue("id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? new Pagamento
        {
            PagamentoId=r.GetGuid("pagamento_id"), PedidoId=r.GetGuid("pedido_id"), Forma=(FormaPagamento)r.GetInt32("forma"),
            Status=(StatusPagamento)r.GetInt32("status"), Valor=r.GetDecimal("valor"),
            TransacaoExternaId=r.GetSafeString("transacao_externa_id"), CriadoEm=r.GetDateTime("criado_em")
        } : null;
    }

    private async Task<Entrega?> ObterEntrega(Guid id)
    {
        await using var cmd = new NpgsqlCommand("SELECT * FROM rota_sabor.entrega WHERE pedido_id=@id LIMIT 1", Connection);
        cmd.Parameters.AddWithValue("id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? new Entrega
        {
            EntregaId=r.GetGuid("entrega_id"), PedidoId=r.GetGuid("pedido_id"), EntregadorId=r.GetNullableGuid("entregador_id"),
            Status=(StatusEntrega)r.GetInt32("status"), CodigoConfirmacao=r.GetString("codigo_confirmacao"),
            LatitudeAtual=r.GetNullableDecimal("latitude_atual"), LongitudeAtual=r.GetNullableDecimal("longitude_atual"),
            ColetadoEm=r.GetNullableDateTime("coletado_em"), EntregueEm=r.GetNullableDateTime("entregue_em")
        } : null;
    }

    private static Pedido MapPedido(NpgsqlDataReader r) => new()
    {
        PedidoId=r.GetGuid("pedido_id"), Numero=r.GetInt64("numero"), ClienteId=r.GetGuid("cliente_id"),
        RestauranteId=r.GetGuid("restaurante_id"), EnderecoId=r.GetNullableGuid("endereco_id"),
        TipoRecebimento=(TipoRecebimento)r.GetInt32("tipo_recebimento"), Status=(StatusPedido)r.GetInt32("status"),
        Subtotal=r.GetDecimal("subtotal"), TaxaEntrega=r.GetDecimal("taxa_entrega"), Desconto=r.GetDecimal("desconto"),
        Total=r.GetDecimal("total"), Observacao=r.GetSafeString("observacao"), MotivoCancelamento=r.GetSafeString("motivo_cancelamento"),
        CriadoEm=r.GetDateTime("criado_em"), AtualizadoEm=r.GetDateTime("atualizado_em")
    };
}
