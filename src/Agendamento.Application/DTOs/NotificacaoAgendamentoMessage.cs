namespace Agendamento.Application.DTOs;

// Este é o "envelope" da mensagem que viaja pela fila do RabbitMQ.
// Ele é serializado em JSON pela API (producer) e desserializado
// pelo Worker (consumer) do outro lado.
public record NotificacaoAgendamentoMessage(
    Guid AgendamentoId,
    Guid ClienteId,
    string EmailCliente,
    string NomeCliente,
    DateTime DataHoraInicio
);
