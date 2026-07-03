using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class InventarioItemConfiguration : IEntityTypeConfiguration<InventarioItem>
{
    public void Configure(EntityTypeBuilder<InventarioItem> builder)
    {
        builder.ToTable("inventario_items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Codigo).HasMaxLength(50).IsRequired();
        builder.HasIndex(i => i.Codigo).IsUnique();

        builder.Property(i => i.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(i => i.StockActual).IsRequired();
        // Referencia el nombre real de columna ("StockActual", PascalCase por convención
        // por defecto de EF — sin HasColumnName no se vuelve snake_case): un check
        // constraint sin comillas se pliega a minúsculas en Postgres y no encontraría la
        // columna real.
        builder.ToTable(t => t.HasCheckConstraint("ck_inventario_stock_no_negativo", "\"StockActual\" >= 0"));

        // Usa la columna de sistema xmin de PostgreSQL como token de concurrencia
        // optimista adicional; la garantía ACID primaria proviene del bloqueo de fila
        // (SELECT ... FOR UPDATE) aplicado en IInventarioRepository.ObtenerConBloqueoAsync.
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
