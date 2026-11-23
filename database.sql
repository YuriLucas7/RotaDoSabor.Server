-- docker run --name rota-do-sabor-postgres --env-file .env -p 5432:5432 -d postgres:17

CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE SCHEMA IF NOT EXISTS rota_sabor;

CREATE TABLE IF NOT EXISTS rota_sabor.cliente (
    cliente_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nome varchar(150) NOT NULL,
    email varchar(180) NOT NULL,
    telefone varchar(20) NOT NULL,
    senha_hash text NOT NULL,
    ativo boolean NOT NULL DEFAULT true,
    criado_em timestamp without time zone NOT NULL DEFAULT now(),
    CONSTRAINT uq_cliente_email UNIQUE (email),
    CONSTRAINT uq_cliente_telefone UNIQUE (telefone)
);

CREATE TABLE IF NOT EXISTS rota_sabor.endereco (
    endereco_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id uuid NOT NULL REFERENCES rota_sabor.cliente(cliente_id) ON DELETE CASCADE,
    apelido varchar(60) NOT NULL,
    cep varchar(12) NOT NULL,
    logradouro varchar(180) NOT NULL,
    numero varchar(30) NOT NULL,
    complemento varchar(100),
    bairro varchar(100) NOT NULL,
    cidade varchar(100) NOT NULL,
    estado char(2) NOT NULL,
    latitude numeric(10,7),
    longitude numeric(10,7),
    principal boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS rota_sabor.restaurante (
    restaurante_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nome varchar(150) NOT NULL,
    descricao text,
    cnpj varchar(18) NOT NULL UNIQUE,
    telefone varchar(20) NOT NULL,
    email varchar(180) NOT NULL,
    imagem_url text,
    categoria varchar(80) NOT NULL,
    pedido_minimo numeric(12,2) NOT NULL DEFAULT 0,
    taxa_entrega numeric(12,2) NOT NULL DEFAULT 0,
    tempo_entrega_minimo integer NOT NULL DEFAULT 20,
    tempo_entrega_maximo integer NOT NULL DEFAULT 45,
    status integer NOT NULL DEFAULT 1,
    aberto boolean NOT NULL DEFAULT false,
    possui_oferta boolean NOT NULL DEFAULT false,
    criado_em timestamp without time zone NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS rota_sabor.produto (
    produto_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    restaurante_id uuid NOT NULL REFERENCES rota_sabor.restaurante(restaurante_id) ON DELETE CASCADE,
    nome varchar(150) NOT NULL,
    descricao text,
    categoria varchar(80) NOT NULL,
    imagem_url text,
    preco numeric(12,2) NOT NULL CHECK (preco > 0),
    preco_promocional numeric(12,2),
    disponivel boolean NOT NULL DEFAULT true,
    ordem integer NOT NULL DEFAULT 0
);

CREATE SEQUENCE IF NOT EXISTS rota_sabor.pedido_numero_seq START 1000;

CREATE TABLE IF NOT EXISTS rota_sabor.pedido (
    pedido_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    numero bigint NOT NULL DEFAULT nextval('rota_sabor.pedido_numero_seq'),
    cliente_id uuid NOT NULL REFERENCES rota_sabor.cliente(cliente_id),
    restaurante_id uuid NOT NULL REFERENCES rota_sabor.restaurante(restaurante_id),
    endereco_id uuid REFERENCES rota_sabor.endereco(endereco_id),
    tipo_recebimento integer NOT NULL,
    status integer NOT NULL,
    subtotal numeric(12,2) NOT NULL,
    taxa_entrega numeric(12,2) NOT NULL DEFAULT 0,
    desconto numeric(12,2) NOT NULL DEFAULT 0,
    total numeric(12,2) NOT NULL,
    observacao text,
    motivo_cancelamento text,
    criado_em timestamp without time zone NOT NULL DEFAULT now(),
    atualizado_em timestamp without time zone NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS rota_sabor.pedido_item (
    pedido_item_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    pedido_id uuid NOT NULL REFERENCES rota_sabor.pedido(pedido_id) ON DELETE CASCADE,
    produto_id uuid NOT NULL REFERENCES rota_sabor.produto(produto_id),
    nome_produto varchar(150) NOT NULL,
    quantidade integer NOT NULL CHECK (quantidade > 0),
    preco_unitario numeric(12,2) NOT NULL,
    total numeric(12,2) NOT NULL,
    observacao text
);

CREATE TABLE IF NOT EXISTS rota_sabor.pagamento (
    pagamento_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    pedido_id uuid NOT NULL UNIQUE REFERENCES rota_sabor.pedido(pedido_id) ON DELETE CASCADE,
    forma integer NOT NULL,
    status integer NOT NULL,
    valor numeric(12,2) NOT NULL,
    transacao_externa_id varchar(180),
    criado_em timestamp without time zone NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS rota_sabor.entrega (
    entrega_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    pedido_id uuid NOT NULL UNIQUE REFERENCES rota_sabor.pedido(pedido_id) ON DELETE CASCADE,
    entregador_id uuid,
    status integer NOT NULL,
    codigo_confirmacao varchar(10) NOT NULL,
    latitude_atual numeric(10,7),
    longitude_atual numeric(10,7),
    coletado_em timestamp without time zone,
    entregue_em timestamp without time zone
);

CREATE TABLE IF NOT EXISTS rota_sabor.avaliacao (
    avaliacao_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    pedido_id uuid NOT NULL UNIQUE REFERENCES rota_sabor.pedido(pedido_id),
    cliente_id uuid NOT NULL REFERENCES rota_sabor.cliente(cliente_id),
    restaurante_id uuid NOT NULL REFERENCES rota_sabor.restaurante(restaurante_id),
    nota_restaurante integer NOT NULL CHECK (nota_restaurante BETWEEN 1 AND 5),
    nota_entrega integer NOT NULL CHECK (nota_entrega BETWEEN 1 AND 5),
    nota_comida integer NOT NULL CHECK (nota_comida BETWEEN 1 AND 5),
    comentario text,
    criado_em timestamp without time zone NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_produto_restaurante ON rota_sabor.produto(restaurante_id, disponivel);
CREATE INDEX IF NOT EXISTS ix_pedido_cliente ON rota_sabor.pedido(cliente_id, criado_em DESC);
CREATE INDEX IF NOT EXISTS ix_pedido_restaurante_status ON rota_sabor.pedido(restaurante_id, status);
