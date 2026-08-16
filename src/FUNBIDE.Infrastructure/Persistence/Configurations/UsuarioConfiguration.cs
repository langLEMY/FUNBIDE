using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.SupabaseUserId).IsRequired();
        builder.HasIndex(u => u.SupabaseUserId).IsUnique();

        builder.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Correo).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Correo).IsUnique();

        builder.Property(u => u.NombreUsuario).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.NombreUsuario).IsUnique();

        builder.Property(u => u.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.Activo).IsRequired();
        builder.Property(u => u.EliminadoPermanentemente).IsRequired().HasDefaultValue(false);

        builder.Property(u => u.FotoPerfilUrl).HasMaxLength(500);

        builder.Property(u => u.Especialidad)
            .HasConversion<string>()
            .HasMaxLength(40);
    }
}
