using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class SesionActivaConfiguration : IEntityTypeConfiguration<SesionActiva>
{
    public void Configure(EntityTypeBuilder<SesionActiva> builder)
    {
        builder.ToTable("sesiones_activas");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UsuarioId).IsRequired();
        builder.Property(s => s.SessionId).HasMaxLength(100).IsRequired();
        builder.Property(s => s.UltimoVistoEn).IsRequired();

        // Un dispositivo (SessionId) manda latidos repetidos para el mismo usuario: se
        // actualiza la fila existente en vez de acumular una por latido.
        builder.HasIndex(s => new { s.UsuarioId, s.SessionId }).IsUnique();
    }
}
