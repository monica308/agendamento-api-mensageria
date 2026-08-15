using Agendamento.Infrastructure.Messaging;
using Agendamento.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Reaproveita a mesma classe de configurações do RabbitMQ usada na Infrastructure,
// lendo a seção "RabbitMQ" do appsettings.json (ou variáveis de ambiente).
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Registra nosso BackgroundService. O .NET vai automaticamente chamar
// StartAsync (que dispara o ExecuteAsync) quando a aplicação subir.
builder.Services.AddHostedService<NotificacaoConsumerService>();

var host = builder.Build();
host.Run();
