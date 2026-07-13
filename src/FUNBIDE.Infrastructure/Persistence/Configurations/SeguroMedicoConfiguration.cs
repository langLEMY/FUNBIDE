using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class SeguroMedicoConfiguration : IEntityTypeConfiguration<SeguroMedico>
{
    public void Configure(EntityTypeBuilder<SeguroMedico> builder)
    {
        builder.ToTable("seguros_medicos");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(s => s.PorcentajeCobertura).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(s => s.Activo).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_seguro_medico_porcentaje_cobertura_rango", "\"PorcentajeCobertura\" > 0 AND \"PorcentajeCobertura\" <= 100"));

        builder.HasIndex(s => s.Nombre).IsUnique();
    }
}
