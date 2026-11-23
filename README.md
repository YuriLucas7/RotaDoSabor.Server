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

1. Crie um banco PostgreSQL chamado `rota_do_sabor`.
2. Execute o arquivo `database.sql`.
3. Altere `ConnectionStrings:DefaultConnection` no `appsettings.json`.
4. Substitua `Jwt:Key` por uma chave segura com pelo menos 32 caracteres.
5. Execute `dotnet restore` e `dotnet run`.

No ambiente de desenvolvimento, o Swagger estará disponível em `/swagger`.
