# RotaDoSabor.Server

API REST do aplicativo Rota do Sabor, construída com ASP.NET Core 10, Minimal APIs, PostgreSQL e Npgsql.

## Estrutura

- `Controllers`: mapeamento dos endpoints HTTP.
- `Models`: entidades, enums e contratos da API.
- `Repositories`: acesso ao PostgreSQL com Npgsql.
- `Services`: regras de negócio e validações.
- `Validations`: exceções e regras de validação.
- `database.sql`: criação do schema e das tabelas.

## Configuração

1. Copie o arquivo `.env.example` para `.env`.
2. Preencha no `.env` as credenciais do PostgreSQL e a chave JWT.
3. Crie o contêiner com o comando indicado no início do `database.sql`.
4. Execute o arquivo `database.sql` no banco `rota_do_sabor`.
5. Execute `dotnet restore` e `dotnet run`.

No ambiente de desenvolvimento, o Swagger estará disponível em `/swagger`.
