using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NombreCompleto).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Cargo).HasMaxLength(100);

        builder.HasIndex(e => e.NombreCompleto);
    }
}
