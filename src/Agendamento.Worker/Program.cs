using Agendamento.Infrastructure.Messaging;
using Agendamento.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<NotificacaoConsumerService>();

var host = builder.Build();
host.Run();
