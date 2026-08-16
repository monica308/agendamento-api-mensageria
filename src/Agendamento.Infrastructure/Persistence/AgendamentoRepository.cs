using Agendamento.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using EntidadeAgendamento = Agendamento.Domain.Entities.Agendamento;

namespace Agendamento.Infrastructure.Persistence;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly AgendamentoDbContext _context;

    public AgendamentoRepository(AgendamentoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteConflitoAsync(Guid profissionalId, DateTime inicio, DateTime fim, CancellationToken ct)
    {
        return await _context.Agendamentos.AnyAsync(a =>
            a.ProfissionalId == profissionalId &&
            a.Status != Domain.Enums.StatusAgendamento.Cancelado &&
            a.DataHoraInicio < fim &&
            a.DataHoraFim > inicio,
            ct);
    }

    public async Task AdicionarAsync(EntidadeAgendamento agendamento, CancellationToken ct)
    {
        await _context.Agendamentos.AddAsync(agendamento, ct);
    }

    public async Task<EntidadeAgendamento?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
