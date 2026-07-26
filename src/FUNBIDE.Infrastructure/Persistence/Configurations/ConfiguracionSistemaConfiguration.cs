using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class ConfiguracionSistemaConfiguration : IEntityTypeConfiguration<ConfiguracionSistema>
{
    public void Configure(EntityTypeBuilder<ConfiguracionSistema> builder)
    {
        builder.ToTable("configuracion_sistema");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ModoMantenimientoActivo).IsRequired();
        builder.Property(c => c.ModoMantenimientoMensaje).HasMaxLength(500);
        builder.Property(c => c.ActualizadoEn).IsRequired();
        builder.Property(c => c.ActualizadoPorUsuarioId).IsRequired();
    }
}
