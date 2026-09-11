using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class EntradaHistorialClinicoConfiguration : IEntityTypeConfiguration<EntradaHistorialClinico>
{
    public void Configure(EntityTypeBuilder<EntradaHistorialClinico> builder)
    {
        builder.ToTable("historial_clinico");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PacienteId).IsRequired();
        builder.Property(e => e.DoctorId).IsRequired();
        builder.Property(e => e.CitaId);
        builder.Property(e => e.RegistradoEn).IsRequired();

        // Default explícito para que las filas existentes (todas notas libres antes de
        // que existiera este campo) queden clasificadas como NotaClinica sin migración de datos.
        builder.Property(e => e.Tipo)
            .HasConversion<string>()
            .HasColumnName("tipo")
            .HasMaxLength(40)
            .HasDefaultValue(TipoEntradaHistorial.NotaClinica)
            .IsRequired();

        builder.Property(e => e.Contenido)
            .HasConversion(vo => vo.Valor, valor => DocumentoJson.Crear(valor))
            .HasColumnName("contenido")
            .HasColumnType("jsonb")
            .IsRequired();

        // Índice GIN sobre la columna JSONB: acelera búsquedas por claves/valores
        // dentro del historial clínico (operadores @>, ?, ?&, ?|).
        builder.HasIndex(e => e.Contenido).HasMethod("gin");

        builder.HasIndex(e => e.PacienteId);
        builder.HasIndex(e => e.RegistradoEn);
    }
}
