using System.Text;
using System.Text.Json;
using Agendamento.Application.DTOs;
using Agendamento.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agendamento.Worker;

public class NotificacaoConsumerService : BackgroundService
{
    private const string NomeFila = "fila-notificacoes-email";

    private readonly RabbitMqSettings _settings;
    private readonly ILogger<NotificacaoConsumerService> _logger;

    public NotificacaoConsumerService(IOptions<RabbitMqSettings> settings, ILogger<NotificacaoConsumerService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: NomeFila,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Processa uma mensagem por vez, evitando sobrecarga em picos.
        await channel.BasicQosAsync(0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, evento) =>
        {
            var corpo = evento.Body.ToArray();
            var json = Encoding.UTF8.GetString(corpo);

            try
            {
                var mensagem = JsonSerializer.Deserialize<NotificacaoAgendamentoMessage>(json);

                if (mensagem is not null)
                {
                    await ProcessarNotificacaoAsync(mensagem, stoppingToken);
                }

                await channel.BasicAckAsync(evento.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem de notificação. Ela será reenviada para a fila.");

                // TODO: dead-letter queue após N tentativas, em vez de requeue indefinido.
                await channel.BasicNackAsync(evento.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queue: NomeFila, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessarNotificacaoAsync(NotificacaoAgendamentoMessage mensagem, CancellationToken ct)
    {
        // TODO: integrar com provedor real de e-mail (SES, SendGrid, SMTP).
        _logger.LogInformation(
            "Enviando e-mail de confirmação para {Email} sobre o agendamento {AgendamentoId} às {DataHora}",
            mensagem.EmailCliente, mensagem.AgendamentoId, mensagem.DataHoraInicio);

        await Task.Delay(500, ct);
    }
}
