using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class ResumenDiarioConfiguration : IEntityTypeConfiguration<ResumenDiario>
{
    public void Configure(EntityTypeBuilder<ResumenDiario> builder)
    {
        builder.ToTable("resumenes_diarios");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Fecha).IsRequired();
        builder.HasIndex(r => r.Fecha).IsUnique();

        builder.Property(r => r.PacientesAtendidos).IsRequired();
        builder.Property(r => r.DineroMovido).HasColumnType("decimal(12,2)").IsRequired();

        // Usa xmin como token de concurrencia optimista adicional, igual que
        // InventarioItemConfiguration: la garantía primaria es el bloqueo de fila
        // explícito (SELECT ... FOR UPDATE) en IResumenDiarioRepository.ObtenerOCrearConBloqueoAsync.
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
