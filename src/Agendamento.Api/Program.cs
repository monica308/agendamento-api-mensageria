using Agendamento.Application.UseCases;
using Agendamento.Infrastructure;
using Agendamento.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CriarAgendamentoUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Aplica migrations pendentes ao subir. Em produção com múltiplas réplicas,
// mover para um step de deploy separado em vez de rodar em cada instância.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgendamentoDbContext>();
    db.Database.Migrate();
}

app.Run();
