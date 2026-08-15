using System.Text;
using System.Text.Json;
using Agendamento.Application.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Agendamento.Infrastructure.Messaging;

// Implementação concreta de IMessagePublisher usando RabbitMQ.
// Esta é a ÚNICA classe do projeto que "sabe" que estamos usando RabbitMQ
// especificamente - se um dia trocarmos para AWS SQS, só esta classe muda
// (criaríamos uma SqsMessagePublisher : IMessagePublisher no lugar dela).
public class RabbitMqPublisher : IMessagePublisher
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task PublicarAsync<T>(string fila, T mensagem, CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        // Abre uma conexão e um "canal" (channel) com o RabbitMQ.
        // 'using' garante que a conexão é fechada corretamente ao final,
        // mesmo se algo der errado (evita vazamento de conexões).
        using var connection = await factory.CreateConnectionAsync(ct);
        using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        // "Declara" a fila: cria a fila se ela ainda não existir.
        // durable: true => a fila sobrevive a um restart do RabbitMQ
        // (as mensagens não declaradas como persistentes ainda podem se
        // perder num crash, mas a FILA em si continua existindo).
        await channel.QueueDeclareAsync(
            queue: fila,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        // Serializa o objeto C# para JSON e depois para bytes,
        // porque o RabbitMQ transporta apenas bytes "crus".
        var json = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true // a mensagem em si sobrevive a um restart do broker
        };

        // Publica a mensagem. exchange "" (vazio) = exchange padrão do
        // RabbitMQ, que roteia diretamente para a fila cujo nome bate
        // com o routingKey - a forma mais simples de publicar, ótima
        // para o escopo deste projeto.
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: fila,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);
    }
}
