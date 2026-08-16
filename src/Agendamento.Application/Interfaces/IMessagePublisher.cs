namespace Agendamento.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublicarAsync<T>(string fila, T mensagem, CancellationToken ct);
}
