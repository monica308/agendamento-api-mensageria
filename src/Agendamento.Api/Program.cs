using Agendamento.Application.UseCases;
using Agendamento.Infrastructure;
using Agendamento.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Registro de serviços (Injeção de Dependência) ---

// Controllers do estilo MVC/API tradicional (em vez de Minimal APIs) -
// mantém uma organização mais familiar se você já usou [ApiController] antes.
builder.Services.AddControllers();

// Swagger/OpenAPI: gera automaticamente uma documentação interativa da
// API, acessível em /swagger quando rodando em ambiente de Development.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Chama o método que criamos na Infrastructure, que registra o DbContext,
// o repositório e o publisher do RabbitMQ - um único ponto de configuração.
builder.Services.AddInfrastructure(builder.Configuration);

// Registra o caso de uso da Application. "Scoped" porque ele depende do
// repositório, que também é Scoped (dependências Singleton não podem
// depender de coisas Scoped - regra do próprio .NET, para evitar bugs sutis).
builder.Services.AddScoped<CriarAgendamentoUseCase>();

var app = builder.Build();

// --- Pipeline HTTP (a ordem destes "middlewares" importa) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// --- Aplica as migrations pendentes automaticamente ao iniciar ---
// Extremamente útil para o cenário "docker compose up": o container da Api
// sobe, aplica o schema mais recente no banco automaticamente, sem você
// precisar rodar comando nenhum manualmente depois.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgendamentoDbContext>();
    db.Database.Migrate();
}

app.Run();
