namespace Agendamento.Application.DTOs;

public record AgendamentoResponse(
    Guid Id,
    Guid ClienteId,
    Guid ProfissionalId,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    string Status
);
