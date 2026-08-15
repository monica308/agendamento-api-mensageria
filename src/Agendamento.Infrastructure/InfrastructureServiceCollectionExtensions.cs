using Agendamento.Application.Interfaces;
using Agendamento.Infrastructure.Messaging;
using Agendamento.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agendamento.Infrastructure;

// Um "método de extensão" que organiza, num só lugar, todo o registro de
// serviços da Infrastructure. Tanto a Api quanto o Worker vão chamar este
// mesmo método no Program.cs deles - evitando duplicar essa configuração.
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Registra o DbContext, dizendo pra usar o provider de SQL Server
        // com a connection string vinda do appsettings.json / variáveis de ambiente.
        services.AddDbContext<AgendamentoDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // "Scoped" = uma instância nova por requisição HTTP (ou por job do Worker).
        // Isso é importante: o DbContext NÃO é seguro para ser compartilhado
        // entre requisições simultâneas, então cada uma ganha a sua.
        services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();

        // Lê a seção "RabbitMQ" do appsettings.json e "liga" ao RabbitMqSettings.
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));

        // "Singleton" = uma única instância para toda a vida da aplicação.
        // Faz sentido aqui porque o publisher não guarda estado por requisição.
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
