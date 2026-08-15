using Agendamento.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using EntidadeAgendamento = Agendamento.Domain.Entities.Agendamento;

namespace Agendamento.Infrastructure.Persistence;

// Esta classe é a implementação REAL da interface IAgendamentoRepository
// que a camada Application definiu. É aqui, e só aqui, que o Entity
// Framework é efetivamente usado neste fluxo.
public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly AgendamentoDbContext _context;

    public AgendamentoRepository(AgendamentoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteConflitoAsync(Guid profissionalId, DateTime inicio, DateTime fim, CancellationToken ct)
    {
        // AnyAsync gera um SQL do tipo "SELECT CASE WHEN EXISTS(...) ..."
        // muito mais eficiente do que trazer os agendamentos todos para
        // a memória e comparar em C#.
        return await _context.Agendamentos.AnyAsync(a =>
            a.ProfissionalId == profissionalId &&
            a.Status != Domain.Enums.StatusAgendamento.Cancelado &&
            a.DataHoraInicio < fim &&
            a.DataHoraFim > inicio,
            ct);
    }

    public async Task AdicionarAsync(EntidadeAgendamento agendamento, CancellationToken ct)
    {
        // Add() só marca o objeto como "novo" no rastreador de mudanças do EF.
        // Nenhum SQL é executado ainda nesse ponto.
        await _context.Agendamentos.AddAsync(agendamento, ct);
    }

    public async Task<EntidadeAgendamento?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken ct)
    {
        // É AQUI que o SQL de verdade (INSERT/UPDATE) é enviado ao banco.
        // Se a constraint única do índice for violada, o SQL Server recusa
        // e o Entity Framework relança isso como um DbUpdateException.
        await _context.SaveChangesAsync(ct);
    }
}
