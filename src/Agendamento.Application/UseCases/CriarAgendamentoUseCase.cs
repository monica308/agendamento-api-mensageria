using Agendamento.Application.DTOs;
using Agendamento.Application.Interfaces;
using Agendamento.Domain.Entities;

namespace Agendamento.Application.UseCases;

public class HorarioIndisponivelException : Exception
{
    public HorarioIndisponivelException(string message) : base(message) { }
}

public class CriarAgendamentoUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IMessagePublisher _messagePublisher;

    public CriarAgendamentoUseCase(
        IAgendamentoRepository agendamentoRepository,
        IMessagePublisher messagePublisher)
    {
        _agendamentoRepository = agendamentoRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<AgendamentoResponse> ExecutarAsync(CriarAgendamentoRequest request, CancellationToken ct)
    {
        var existeConflito = await _agendamentoRepository.ExisteConflitoAsync(
            request.ProfissionalId, request.DataHoraInicio, request.DataHoraFim, ct);

        if (existeConflito)
            throw new HorarioIndisponivelException("Este profissional já possui um agendamento nesse horário.");

        var novoAgendamento = new Agendamento.Domain.Entities.Agendamento(
            request.ClienteId, request.ProfissionalId, request.DataHoraInicio, request.DataHoraFim);

        // Se outra requisição concorrente inserir no mesmo horário entre a checagem
        // acima e este SaveChanges, o índice único do banco rejeita o insert
        // (tratado como DbUpdateException no controller).
        await _agendamentoRepository.AdicionarAsync(novoAgendamento, ct);
        await _agendamentoRepository.SalvarAlteracoesAsync(ct);

        var mensagem = new NotificacaoAgendamentoMessage(
            novoAgendamento.Id,
            request.ClienteId,
            EmailCliente: "buscar-do-cliente@exemplo.com", // TODO: buscar via IClienteRepository
            NomeCliente: "Nome do cliente",
            novoAgendamento.DataHoraInicio);

        await _messagePublisher.PublicarAsync("fila-notificacoes-email", mensagem, ct);

        return new AgendamentoResponse(
            novoAgendamento.Id,
            novoAgendamento.ClienteId,
            novoAgendamento.ProfissionalId,
            novoAgendamento.DataHoraInicio,
            novoAgendamento.DataHoraFim,
            novoAgendamento.Status.ToString());
    }
}
