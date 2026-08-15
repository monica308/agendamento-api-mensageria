namespace Agendamento.Domain.Entities;

public class Profissional
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Especialidade { get; private set; }

    private Profissional() { }

    public Profissional(string nome, string especialidade)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do profissional é obrigatório.");

        if (string.IsNullOrWhiteSpace(especialidade))
            throw new ArgumentException("Especialidade é obrigatória.");

        Id = Guid.NewGuid();
        Nome = nome;
        Especialidade = especialidade;
    }
}
