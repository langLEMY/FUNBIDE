using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class TarifarioProcedimientoConfiguration : IEntityTypeConfiguration<TarifarioProcedimiento>
{
    public void Configure(EntityTypeBuilder<TarifarioProcedimiento> builder)
    {
        builder.ToTable("tarifario_procedimientos");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SeguroMedicoId).IsRequired();
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Procedimiento).HasMaxLength(300).IsRequired();
        builder.Property(t => t.MontoSeguro).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(t => t.MontoPaciente).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(t => t.MontoTotal).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(t => t.Activo).IsRequired();

        // Reconciliación del import (ver ImportarTarifarioUseCase): si ya existe una fila
        // con el mismo seguro+plan+procedimiento, se actualizan los montos en vez de
        // duplicarla — necesario para poder reimportar una lista de precios actualizada.
        builder.HasIndex(t => new { t.SeguroMedicoId, t.Plan, t.Procedimiento }).IsUnique();
    }
}
