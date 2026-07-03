using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("citas");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PacienteId).IsRequired();
        builder.Property(c => c.DoctorId).IsRequired();
        builder.Property(c => c.Motivo).HasMaxLength(500).IsRequired();
        builder.Property(c => c.NotasCierre).HasMaxLength(2000);

        builder.Property(c => c.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(c => c.Intervalo, intervalo =>
        {
            intervalo.Property(i => i.Inicio).HasColumnName("inicio");
            intervalo.Property(i => i.Fin).HasColumnName("fin");
        });
        builder.Navigation(c => c.Intervalo).IsRequired(false);

        builder.HasIndex(c => new { c.DoctorId, c.Estado });
    }
}
