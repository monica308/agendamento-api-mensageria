namespace Agendamento.Application.DTOs;

public record CriarAgendamentoRequest(
    Guid ClienteId,
    Guid ProfissionalId,
    DateTime DataHoraInicio,
    DateTime DataHoraFim
);
