using EntidadeAgendamento = Agendamento.Domain.Entities.Agendamento;

namespace Agendamento.Application.Interfaces;

public interface IAgendamentoRepository
{
    Task<bool> ExisteConflitoAsync(Guid profissionalId, DateTime inicio, DateTime fim, CancellationToken ct);
    Task AdicionarAsync(EntidadeAgendamento agendamento, CancellationToken ct);
    Task<EntidadeAgendamento?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}