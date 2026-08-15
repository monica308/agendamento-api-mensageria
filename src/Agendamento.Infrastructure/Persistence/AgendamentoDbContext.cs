using Microsoft.EntityFrameworkCore;
using EntidadeAgendamento = Agendamento.Domain.Entities.Agendamento;
using Agendamento.Domain.Entities;

namespace Agendamento.Infrastructure.Persistence;

// O DbContext representa a "sessão" com o banco de dados. Cada DbSet<T>
// abaixo vira uma tabela. É através dele que o Entity Framework rastreia
// o que foi adicionado/alterado, para gerar o SQL certo no SaveChanges.
public class AgendamentoDbContext : DbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<EntidadeAgendamento> Agendamentos => Set<EntidadeAgendamento>();

    public AgendamentoDbContext(DbContextOptions<AgendamentoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Em vez de "sujar" as entidades do Domain com atributos do EF
        // (ex: [Required], [MaxLength]), usamos "Fluent API": configuramos
        // o mapeamento aqui, de fora, mantendo o Domain limpo de detalhes
        // de banco de dados.

        modelBuilder.Entity<Cliente>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Profissional>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Especialidade).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<EntidadeAgendamento>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Status).HasConversion<string>(); // salva o enum como texto legível no banco

            // *** A LINHA MAIS IMPORTANTE DO PROJETO PARA O TEMA CONCORRÊNCIA ***
            // Cria um índice ÚNICO composto por ProfissionalId + DataHoraInicio.
            // Isso significa: o próprio SQL Server vai IMPEDIR fisicamente que
            // duas linhas existam com o mesmo profissional no mesmo horário de
            // início - mesmo que duas requisições cheguem no mesmo milissegundo
            // e passem pela checagem em C# ao mesmo tempo. É a garantia final,
            // porque bancos de dados relacionais processam escritas na mesma
            // linha/índice de forma serializada internamente.
            builder.HasIndex(a => new { a.ProfissionalId, a.DataHoraInicio })
                   .IsUnique();
        });
    }
}
