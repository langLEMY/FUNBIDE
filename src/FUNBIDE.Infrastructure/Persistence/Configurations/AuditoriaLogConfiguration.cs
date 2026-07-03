using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class AuditoriaLogConfiguration : IEntityTypeConfiguration<AuditoriaLog>
{
    public void Configure(EntityTypeBuilder<AuditoriaLog> builder)
    {
        builder.ToTable("auditoria_logs");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.UsuarioId);
        builder.Property(l => l.Accion).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Recurso).HasMaxLength(300).IsRequired();
        builder.Property(l => l.CodigoRespuestaHttp);
        builder.Property(l => l.RegistradoEn).IsRequired();

        builder.Property(l => l.Detalle)
            .HasConversion(vo => vo.Valor, valor => DocumentoJson.Crear(valor))
            .HasColumnName("detalle")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(l => l.Detalle).HasMethod("gin");
        builder.HasIndex(l => l.RegistradoEn);
        builder.HasIndex(l => l.Recurso);
    }
}
