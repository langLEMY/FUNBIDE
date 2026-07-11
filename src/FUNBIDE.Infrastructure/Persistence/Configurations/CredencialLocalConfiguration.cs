using FUNBIDE.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class CredencialLocalConfiguration : IEntityTypeConfiguration<CredencialLocal>
{
    public void Configure(EntityTypeBuilder<CredencialLocal> builder)
    {
        builder.ToTable("credenciales_locales");
        builder.HasKey(c => c.UsuarioId);
        builder.Property(c => c.UsuarioId).ValueGeneratedNever();

        builder.Property(c => c.PasswordHash).HasMaxLength(500).IsRequired();
    }
}
