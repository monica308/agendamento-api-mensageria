using Agendamento.Application.DTOs;
using Agendamento.Application.Interfaces;
using Agendamento.Domain.Entities;

namespace Agendamento.Application.UseCases;

// Exceção específica para quando o horário já está ocupado.
// A camada Api vai capturar esse tipo de exceção e traduzir para um HTTP 409 (Conflict).
public class HorarioIndisponivelException : Exception
{
    public HorarioIndisponivelException(string message) : base(message) { }
}

public class CriarAgendamentoUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IMessagePublisher _messagePublisher;

    // O .NET vai "injetar" essas duas dependências automaticamente em tempo
    // de execução (Injeção de Dependência), com base em como configurarmos
    // o Program.cs da Api. O UseCase só enxerga as INTERFACES, nunca o
    // Entity Framework ou o RabbitMQ diretamente.
    public CriarAgendamentoUseCase(
        IAgendamentoRepository agendamentoRepository,
        IMessagePublisher messagePublisher)
    {
        _agendamentoRepository = agendamentoRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<AgendamentoResponse> ExecutarAsync(CriarAgendamentoRequest request, CancellationToken ct)
    {
        // 1) Checagem "otimista" de conflito, consultando o banco.
        //    Isso resolve o caso comum (99% das vezes) e dá uma resposta
        //    rápida e amigável ("horário indisponível") ANTES de tentar salvar.
        var existeConflito = await _agendamentoRepository.ExisteConflitoAsync(
            request.ProfissionalId, request.DataHoraInicio, request.DataHoraFim, ct);

        if (existeConflito)
            throw new HorarioIndisponivelException("Este profissional já possui um agendamento nesse horário.");

        // 2) Cria a entidade de Domain - todas as validações de negócio
        //    (datas, etc.) acontecem dentro do construtor, como vimos.
        var novoAgendamento = new Agendamento.Domain.Entities.Agendamento(
            request.ClienteId, request.ProfissionalId, request.DataHoraInicio, request.DataHoraFim);

        // 3) Tenta salvar. Se, entre o passo 1 e este passo, OUTRA requisição
        //    concorrente conseguiu inserir um agendamento no mesmo horário,
        //    a constraint única do banco (configurada na Infrastructure) vai
        //    rejeitar este INSERT, e o SalvarAlteracoesAsync vai lançar uma
        //    exceção de violação de constraint. É essa a nossa "rede de
        //    segurança final" contra concorrência - ver o comentário no
        //    catch, na camada Api, para saber como isso é tratado.
        await _agendamentoRepository.AdicionarAsync(novoAgendamento, ct);
        await _agendamentoRepository.SalvarAlteracoesAsync(ct);

        // 4) Publica o evento na fila. Isso retorna quase instantaneamente -
        //    RabbitMQ apenas recebe a mensagem e a guarda; ele NÃO espera
        //    o e-mail ser enviado. É isso que mantém a resposta da API rápida.
        var mensagem = new NotificacaoAgendamentoMessage(
            novoAgendamento.Id,
            request.ClienteId,
            EmailCliente: "buscar-do-cliente@exemplo.com", // no passo da Infra, buscaremos isso de verdade
            NomeCliente: "Nome do cliente",
            novoAgendamento.DataHoraInicio);

        await _messagePublisher.PublicarAsync("fila-notificacoes-email", mensagem, ct);

        // 5) Retorna a resposta para o controller da Api transformar em HTTP 201.
        return new AgendamentoResponse(
            novoAgendamento.Id,
            novoAgendamento.ClienteId,
            novoAgendamento.ProfissionalId,
            novoAgendamento.DataHoraInicio,
            novoAgendamento.DataHoraFim,
            novoAgendamento.Status.ToString());
    }
}
