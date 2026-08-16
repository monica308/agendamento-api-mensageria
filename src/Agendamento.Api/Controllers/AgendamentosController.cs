using Agendamento.Application.DTOs;
using Agendamento.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Agendamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly CriarAgendamentoUseCase _criarAgendamentoUseCase;

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
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (HorarioIndisponivelException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Índice único violado: duas requisições concorrentes disputaram o mesmo horário.
            return Conflict(new { erro = "Este horário acabou de ser reservado por outra pessoa. Tente outro horário." });
        }
    }
}
