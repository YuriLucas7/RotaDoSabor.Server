namespace RotaDoSabor.Models;

public enum PerfilUsuario
{
    Cliente = 1,
    Restaurante = 2,
    Entregador = 3,
    Administrador = 4
}

public enum StatusRestaurante
{
    Pendente = 1,
    Ativo = 2,
    Bloqueado = 3
}

public enum StatusPedido
{
    AguardandoPagamento = 1,
    PagamentoAprovado = 2,
    AguardandoRestaurante = 3,
    EmPreparo = 4,
    Pronto = 5,
    EmColeta = 6,
    EmEntrega = 7,
    Entregue = 8,
    Cancelado = 9
}

public enum FormaPagamento
{
    Pix = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Dinheiro = 4
}

public enum StatusPagamento
{
    Pendente = 1,
    Aprovado = 2,
    Recusado = 3,
    Estornado = 4
}

public enum TipoRecebimento
{
    Entrega = 1,
    Retirada = 2
}

public enum StatusEntrega
{
    AguardandoEntregador = 1,
    EntregadorAtribuido = 2,
    Coletado = 3,
    EmRota = 4,
    Entregue = 5,
    Cancelada = 6
}
