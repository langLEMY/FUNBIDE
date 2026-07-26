using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class DonacionConfiguration : IEntityTypeConfiguration<Donacion>
{
    public void Configure(EntityTypeBuilder<Donacion> builder)
    {
        builder.ToTable("donaciones");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DonanteNombre).HasMaxLength(200).IsRequired();
        builder.Property(d => d.DonanteContacto).HasMaxLength(200);
        builder.Property(d => d.Monto).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(d => d.Concepto).HasMaxLength(300).IsRequired();
        builder.Property(d => d.UsuarioId).IsRequired();
        builder.Property(d => d.RegistradoEn).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("ck_donacion_monto_positivo", "\"Monto\" > 0"));

        builder.HasIndex(d => d.RegistradoEn);
    }
}
