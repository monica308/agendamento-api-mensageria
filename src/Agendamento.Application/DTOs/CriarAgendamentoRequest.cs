namespace Agendamento.Application.DTOs;

// Representa exatamente o JSON que o cliente da API vai enviar no corpo do POST.
// É proposital ser uma classe separada da entidade "Agendamento" do Domain:
// o cliente da API não deveria conseguir, por exemplo, definir o Status
// diretamente ao criar (isso é uma regra interna do sistema).
public record CriarAgendamentoRequest(
    Guid ClienteId,
    Guid ProfissionalId,
    DateTime DataHoraInicio,
    DateTime DataHoraFim
);
