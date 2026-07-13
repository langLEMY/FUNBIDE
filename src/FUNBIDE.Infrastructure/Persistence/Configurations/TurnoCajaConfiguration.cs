using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class TurnoCajaConfiguration : IEntityTypeConfiguration<TurnoCaja>
{
    public void Configure(EntityTypeBuilder<TurnoCaja> builder)
    {
        builder.ToTable("turnos_caja");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UsuarioAperturaId).IsRequired();
        builder.Property(t => t.MontoInicial).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(t => t.AbiertoEn).IsRequired();
        builder.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.UsuarioCierreId);
        builder.Property(t => t.MontoFinalContado).HasColumnType("decimal(12,2)");
        builder.Property(t => t.MontoEsperado).HasColumnType("decimal(12,2)");
        builder.Property(t => t.Diferencia).HasColumnType("decimal(12,2)");
        builder.Property(t => t.Notas).HasMaxLength(2000);
        builder.Property(t => t.CerradoEn);

        builder.ToTable(t => t.HasCheckConstraint("ck_turno_caja_monto_inicial_no_negativo", "\"MontoInicial\" >= 0"));

        // Solo puede haber un turno Abierto a la vez: lo garantiza AbrirTurnoCajaUseCase
        // a nivel de aplicación, y este índice único lo hace irrompible también a nivel
        // de base de datos (dos aperturas concurrentes no pueden colar dos turnos abiertos).
        builder.HasIndex(t => t.Estado).HasFilter("\"Estado\" = 'Abierto'").IsUnique();

        // Token de concurrencia optimista, igual que ResumenDiario/InventarioItem: sin esto,
        // dos cierres de caja concurrentes (dos pestañas, un reintento de red) se pisan en
        // silencio en vez de que el segundo falle con un conflicto detectable.
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
