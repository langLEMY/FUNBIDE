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
        // Filtrado (no un unique index normal): EliminarUsuarioPermanentementeUseCase
        // conserva la fila (desactivada) para no romper referencias de citas/historial/
        // movimientos/auditoría, así que el correo/nombre de usuario de una cuenta borrada
        // permanentemente debe quedar libre para reusarse — igual que ya asumen
        // UsuarioRepository.ObtenerPorCorreoAsync/ObtenerPorNombreUsuarioAsync (filtran
        // "!EliminadoPermanentemente"). Sin este filtro, un unique index normal seguía
        // bloqueando ese correo/usuario para siempre aunque la app ya lo diera por libre.
        builder.HasIndex(u => u.Correo).IsUnique().HasFilter("NOT \"EliminadoPermanentemente\"");

        builder.Property(u => u.NombreUsuario).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.NombreUsuario).IsUnique().HasFilter("NOT \"EliminadoPermanentemente\"");

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

        builder.Property(u => u.Exequatur).HasMaxLength(50);
    }
}
