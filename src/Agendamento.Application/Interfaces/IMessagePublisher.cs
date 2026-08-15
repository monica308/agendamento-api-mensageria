namespace Agendamento.Application.Interfaces;

// Contrato genérico para "publicar uma mensagem em algum lugar".
// A Application só sabe que quer publicar um evento — ela não sabe (e não
// precisa saber) que por baixo dos panos isso é RabbitMQ. Se um dia vocês
// trocarem para AWS SQS, só a implementação na Infrastructure muda.
public interface IMessagePublisher
{
    Task PublicarAsync<T>(string fila, T mensagem, CancellationToken ct);
}
