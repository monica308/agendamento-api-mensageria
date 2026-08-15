using Agendamento.Application.DTOs;
using Agendamento.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Agendamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // vira "api/agendamentos"
public class AgendamentosController : ControllerBase
{
    private readonly CriarAgendamentoUseCase _criarAgendamentoUseCase;

    // O controller recebe o USE CASE já pronto via injeção de dependência.
    // Repare que o controller é "burro" de propósito: ele não tem lógica
    // de negócio nenhuma, só traduz HTTP <-> chamada de caso de uso.
    public AgendamentosController(CriarAgendamentoUseCase criarAgendamentoUseCase)
    {
        _criarAgendamentoUseCase = criarAgendamentoUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AgendamentoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarAgendamentoRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await _criarAgendamentoUseCase.ExecutarAsync(request, ct);

            // 201 Created + o cabeçalho "Location" apontando pra onde consultar
            // esse recurso depois - é o padrão REST correto para um POST que cria algo.
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (HorarioIndisponivelException ex)
        {
            // Conflito de horário detectado na checagem otimista (passo 1 do use case).
            return Conflict(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Erros de validação vindos das entidades do Domain (ex: data no passado).
            return BadRequest(new { erro = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // *** Este é o catch que trata a corrida de concorrência de verdade ***
            // Se duas requisições passaram pela checagem otimista ao mesmo tempo
            // (ambas viram "livre"), só uma consegue de fato inserir no banco -
            // a outra cai exatamente aqui, porque violou o índice único.
            return Conflict(new { erro = "Este horário acabou de ser reservado por outra pessoa. Tente outro horário." });
        }
    }
}
