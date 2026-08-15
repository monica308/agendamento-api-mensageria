using System.Text;
using System.Text.Json;
using Agendamento.Application.DTOs;
using Agendamento.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agendamento.Worker;

// BackgroundService é uma classe base do .NET feita exatamente para isso:
// um processo que roda continuamente, em paralelo, enquanto a aplicação
// estiver de pé. O método ExecuteAsync roda "para sempre" (até o processo
// ser encerrado), reagindo a eventos - nesse caso, mensagens chegando na fila.
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

        // Ao contrário da Api (que abre e fecha uma conexão a cada publicação),
        // o Worker abre UMA conexão e mantém ela viva o tempo todo, porque ele
        // precisa ficar continuamente "ouvindo" a fila.
        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: NomeFila,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // PrefetchCount = 1: diz ao RabbitMQ "não me entregue uma mensagem
        // nova enquanto eu não terminar (confirmar) a que estou processando
        // agora". Evita que o Worker fique sobrecarregado se um pico de
        // agendamentos acontecer de uma vez - ele processa uma de cada vez,
        // de forma controlada.
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

                // BasicAck = "confirmo que processei com sucesso, pode
                // remover essa mensagem da fila definitivamente".
                await channel.BasicAckAsync(evento.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem de notificação. Ela será reenviada para a fila.");

                // BasicNack = "não consegui processar". requeue: true manda
                // a mensagem de VOLTA para a fila, para ser tentada de novo
                // (por este Worker ou outra réplica dele) depois. Isso é o
                // que garante que uma falha temporária (ex: serviço de
                // e-mail fora do ar por 10 segundos) não perde a notificação.
                await channel.BasicNackAsync(evento.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queue: NomeFila, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        // Mantém o BackgroundService "vivo" até a aplicação ser encerrada.
        // O trabalho real acontece de forma orientada a evento, no callback acima.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessarNotificacaoAsync(NotificacaoAgendamentoMessage mensagem, CancellationToken ct)
    {
        // Aqui entraria a integração real de envio de e-mail (ex: AWS SES,
        // SendGrid, SMTP). Por ora, simulamos com um log - suficiente para
        // demonstrar o fluxo completo ponta a ponta no portfólio.
        _logger.LogInformation(
            "Enviando e-mail de confirmação para {Email} sobre o agendamento {AgendamentoId} às {DataHora}",
            mensagem.EmailCliente, mensagem.AgendamentoId, mensagem.DataHoraInicio);

        await Task.Delay(500, ct); // simula a latência de uma chamada real de envio de e-mail
    }
}
