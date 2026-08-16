using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class CobroConfiguration : IEntityTypeConfiguration<Cobro>
{
    public void Configure(EntityTypeBuilder<Cobro> builder)
    {
        builder.ToTable("cobros");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PacienteId).IsRequired();
        builder.Property(c => c.CitaId);
        builder.Property(c => c.TurnoCajaId).IsRequired();
        builder.Property(c => c.UsuarioId).IsRequired();
        builder.Property(c => c.Concepto).HasMaxLength(300).IsRequired();
        builder.Property(c => c.MontoTotal).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(c => c.SeguroMedicoId);
        builder.Property(c => c.PorcentajeCobertura).HasColumnType("decimal(5,2)");
        builder.Property(c => c.MontoCobertura).HasColumnType("decimal(12,2)");
        builder.Property(c => c.TarifarioProcedimientoId);
        builder.Property(c => c.CodigoAutorizacion).HasMaxLength(100);
        builder.Property(c => c.MontoPagado).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(c => c.RegistradoEn).IsRequired();

        // Desglose de cómo se pagó (ver PagoRecibido): owned collection en su propia
        // tabla, sin identidad propia — vive y muere con el Cobro dueño. Clave natural
        // (CobroId, Metodo): un cobro nunca trae dos líneas del mismo método.
        builder.OwnsMany(c => c.Pagos, pago =>
        {
            pago.ToTable("cobros_pagos");
            pago.WithOwner().HasForeignKey("CobroId");
            pago.Property<Guid>("CobroId");
            pago.HasKey("CobroId", nameof(PagoRecibido.Metodo));
            pago.Property(p => p.Metodo).HasConversion<string>().HasMaxLength(20).IsRequired();
            pago.Property(p => p.Monto).HasColumnType("decimal(12,2)").IsRequired();
            pago.ToTable(t => t.HasCheckConstraint("ck_pago_recibido_monto_positivo", "\"Monto\" > 0"));
        });

        builder.Ignore(c => c.MontoACargoPaciente);
        builder.Ignore(c => c.MontoPendiente);

        builder.ToTable(t => t.HasCheckConstraint("ck_cobro_monto_total_positivo", "\"MontoTotal\" > 0"));
        builder.ToTable(t => t.HasCheckConstraint("ck_cobro_monto_pagado_no_negativo", "\"MontoPagado\" >= 0"));

        builder.HasIndex(c => c.PacienteId);
        builder.HasIndex(c => c.TurnoCajaId);
        builder.HasIndex(c => c.CitaId);
        builder.HasIndex(c => c.RegistradoEn);
    }
}
