using Agendamento.Domain.Enums;

namespace Agendamento.Domain.Entities;

public class Agendamento
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid ProfissionalId { get; private set; }
    public DateTime DataHoraInicio { get; private set; }
    public DateTime DataHoraFim { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Agendamento() { }

    public Agendamento(Guid clienteId, Guid profissionalId, DateTime dataHoraInicio, DateTime dataHoraFim)
    {
        if (dataHoraFim <= dataHoraInicio)
            throw new ArgumentException("O horário final deve ser depois do horário inicial.");

        if (dataHoraInicio < DateTime.UtcNow)
            throw new ArgumentException("Não é possível agendar em uma data/hora no passado.");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        ProfissionalId = profissionalId;
        DataHoraInicio = dataHoraInicio;
        DataHoraFim = dataHoraFim;
        Status = StatusAgendamento.Pendente;
        CriadoEm = DateTime.UtcNow;
    }

    // Sobreposição de intervalos: [inicio, fim) de dois agendamentos do mesmo profissional.
    public bool ConflitaCom(Agendamento outro)
    {
        if (outro.ProfissionalId != ProfissionalId)
            return false;

        return DataHoraInicio < outro.DataHoraFim && DataHoraFim > outro.DataHoraInicio;
    }

    public void Confirmar()
    {
        if (Status != StatusAgendamento.Pendente)
            throw new InvalidOperationException("Só é possível confirmar um agendamento pendente.");

        Status = StatusAgendamento.Confirmado;
    }

    public void Cancelar()
    {
        if (Status == StatusAgendamento.Cancelado)
            throw new InvalidOperationException("Este agendamento já está cancelado.");

        Status = StatusAgendamento.Cancelado;
    }
}
