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

        // Renacer y Aps: aseguradoras nuevas con tarifario propio por procedimiento (ver
        // TarifarioProcedimientoConfiguration). El PorcentajeCobertura de acá es solo el
        // respaldo para cuando Cobros usa un procedimiento fuera del tarifario cargado —
        // se estimó del promedio Seguro/Total del tarifario de cada una, NO es una cifra
        // que el usuario haya confirmado explícitamente; conviene revisarla con la
        // fundación antes de depender de ella para un procedimiento real.
        builder.HasData(
            new
            {
                Id = SeguroMedicoIds.Renacer,
                Nombre = "Renacer",
                PorcentajeCobertura = 83m,
                Activo = true,
            },
            new
            {
                Id = SeguroMedicoIds.Aps,
                Nombre = "Aps",
                PorcentajeCobertura = 88m,
                Activo = true,
            });
    }
}

/// <summary>Ids determinísticos compartidos con <see cref="TarifarioProcedimientoConfiguration"/> para poder referenciar estas aseguradoras desde su seed sin una segunda consulta.</summary>
public static class SeguroMedicoIds
{
    public static readonly Guid Renacer = Guid.Parse("8f3c7d20-0003-4a3e-8a9a-000000000001");
    public static readonly Guid Aps = Guid.Parse("8f3c7d20-0003-4a3e-8a9a-000000000002");
}
