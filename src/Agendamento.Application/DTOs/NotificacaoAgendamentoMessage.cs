namespace Agendamento.Application.DTOs;

public record NotificacaoAgendamentoMessage(
    Guid AgendamentoId,
    Guid ClienteId,
    string EmailCliente,
    string NomeCliente,
    DateTime DataHoraInicio
);
