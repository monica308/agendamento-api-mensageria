using Microsoft.EntityFrameworkCore;
using EntidadeAgendamento = Agendamento.Domain.Entities.Agendamento;
using Agendamento.Domain.Entities;

namespace Agendamento.Infrastructure.Persistence;

public class AgendamentoDbContext : DbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<EntidadeAgendamento> Agendamentos => Set<EntidadeAgendamento>();

    public AgendamentoDbContext(DbContextOptions<AgendamentoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            builder.Property(a => a.Status).HasConversion<string>();

            // Constraint única para evitar overbooking em corrida de concorrência
            // (checagem em C# cobre o caso comum; isto é a garantia final no banco).
            // Cobre apenas o mesmo instante de início exato - sobreposição parcial
            // (10h-11h vs 10h30-11h30) fica só na checagem otimista por enquanto.
            builder.HasIndex(a => new { a.ProfissionalId, a.DataHoraInicio })
                   .IsUnique();
        });
    }
}
