using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using RotaDoSabor.Controllers;
using RotaDoSabor.Repositories;
using RotaDoSabor.Services;
using RotaDoSabor.Server.Validations;
using System.Text;
using System.Text.Json.Serialization;

LoadEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A conexão com o banco de dados não foi configurada no arquivo .env.");

// Configuração da conexão com o banco de dados
builder.Services.AddScoped<System.Data.IDbConnection>(_ =>
    new Npgsql.NpgsqlConnection(connectionString));

// Registro dos repositórios
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<EnderecoRepository>();
builder.Services.AddScoped<RestauranteRepository>();
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<PedidoRepository>();
builder.Services.AddScoped<AvaliacaoRepository>();

// Registro dos serviços
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<RestauranteService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<AvaliacaoService>();

string jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("O emissor JWT não foi configurado no arquivo .env.");
string jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("A audiência JWT não foi configurada no arquivo .env.");
string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("A chave JWT não foi configurada no arquivo .env.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, payload) = exception switch
    {
        ValidationException ex => (StatusCodes.Status400BadRequest, (object)ex.Errors),
        NotFoundException ex => (StatusCodes.Status404NotFound, new { erro = ex.Message }),
        ConflictException ex => (StatusCodes.Status409Conflict, new { erro = ex.Message }),
        InvalidOperationException ex => (StatusCodes.Status400BadRequest, new { erro = ex.Message }),
        _ => (StatusCodes.Status500InternalServerError, new { erro = "Ocorreu um erro interno no servidor." })
    };
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(payload);
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { aplicacao = "RotaDoSabor.Server", status = "online" }))
    .WithTags("saúde").WithSummary("Verifica se a API está disponível");

app.MapAuthEndpoints();
app.MapClienteEndpoints();
app.MapEnderecoEndpoints();
app.MapRestauranteEndpoints();
app.MapProdutoEndpoints();
app.MapPedidoEndpoints();
app.MapAvaliacaoEndpoints();

app.Run();

static void LoadEnvironmentVariables()
{
    string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

    if (!File.Exists(envPath))
        return;

    foreach (string line in File.ReadAllLines(envPath))
    {
        string content = line.Trim();

        if (string.IsNullOrWhiteSpace(content) || content.StartsWith('#'))
            continue;

        int separatorIndex = content.IndexOf('=');

        if (separatorIndex <= 0)
            continue;

        string key = content[..separatorIndex].Trim();
        string value = content[(separatorIndex + 1)..].Trim().Trim('"', '\'');

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            Environment.SetEnvironmentVariable(key, value);
    }
}
