using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("pacientes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Apellido).HasMaxLength(150).IsRequired();

        builder.Property(p => p.Documento)
            .HasConversion(vo => vo.Valor, valor => DocumentoIdentidad.Crear(valor))
            .HasColumnName("documento_identidad")
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(p => p.Documento).IsUnique();

        builder.Property(p => p.Telefono).HasMaxLength(30);
        builder.Property(p => p.FotoCedulaPath).HasMaxLength(300);

        builder.Property(p => p.Edad);
        builder.Property(p => p.Condicion).HasMaxLength(200);
        builder.Property(p => p.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.EstadoPaciente.Activo);
        builder.Property(p => p.UltimaVisita);

        builder.HasIndex(p => new { p.Nombre, p.Apellido });
    }
}
