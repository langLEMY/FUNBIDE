using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class MovimientoFinancieroConfiguration : IEntityTypeConfiguration<MovimientoFinanciero>
{
    public void Configure(EntityTypeBuilder<MovimientoFinanciero> builder)
    {
        builder.ToTable("movimientos_financieros");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Monto).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(m => m.Concepto).HasMaxLength(300).IsRequired();
        builder.Property(m => m.UsuarioId).IsRequired();
        builder.Property(m => m.CitaId);
        builder.Property(m => m.TurnoCajaId);
        builder.Property(m => m.RegistradoEn).IsRequired();

        // Columna real "Monto" (PascalCase, sin HasColumnName): debe ir entre comillas
        // en el check constraint o Postgres la pliega a minúsculas y no la encuentra.
        builder.ToTable(t => t.HasCheckConstraint("ck_movimiento_financiero_monto_positivo", "\"Monto\" > 0"));

        builder.HasIndex(m => m.RegistradoEn);

        // ObtenerPorTurnoAsync filtra por esta columna en cada carga del resumen de Caja
        // (ObtenerResumenCajaUseCase) — sin índice, full scan de toda la tabla histórica.
        builder.HasIndex(m => m.TurnoCajaId);
    }
}
