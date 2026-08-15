namespace Agendamento.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }

    // Construtor privado: usado pelo Entity Framework para "materializar"
    // (reconstruir) o objeto quando ele lê uma linha do banco.
    private Cliente() { }

    // Construtor público: é o único jeito de CRIAR um cliente novo no código.
    // Isso garante que um Cliente nunca exista em um estado inválido.
    public Cliente(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Email do cliente é inválido.");

        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
    }
}
