using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class PermisoUsuarioConfiguration : IEntityTypeConfiguration<PermisoUsuario>
{
    public void Configure(EntityTypeBuilder<PermisoUsuario> builder)
    {
        builder.ToTable("permisos_usuario");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UsuarioId).IsRequired();
        builder.Property(p => p.Modulo).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(p => p.Concedido).IsRequired();
        builder.Property(p => p.ActualizadoEn).IsRequired();
        builder.Property(p => p.ActualizadoPorUsuarioId).IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.UsuarioId, p.Modulo }).IsUnique();
    }
}
