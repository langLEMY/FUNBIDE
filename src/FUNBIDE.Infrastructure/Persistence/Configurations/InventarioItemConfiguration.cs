using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
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
        builder.Property(i => i.Categoria)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(i => i.StockMinimo).IsRequired().HasDefaultValue(0);
        // Referencia el nombre real de columna ("StockActual", PascalCase por convención
        // por defecto de EF — sin HasColumnName no se vuelve snake_case): un check
        // constraint sin comillas se pliega a minúsculas en Postgres y no encontraría la
        // columna real.
        builder.ToTable(t => t.HasCheckConstraint("ck_inventario_stock_no_negativo", "\"StockActual\" >= 0"));

        // Usa la columna de sistema xmin de PostgreSQL como token de concurrencia
        // optimista adicional; la garantía ACID primaria proviene del bloqueo de fila
        // (SELECT ... FOR UPDATE) aplicado en IInventarioRepository.ObtenerConBloqueoAsync.
        builder.Property<uint>("xmin").IsRowVersion();

        // Datos de ejemplo para que el inventario no aparezca vacío la primera vez que
        // se aplica esta migración: no hay todavía una pantalla de "crear ítem" (queda
        // para el perfil de Farmacia), así que se siembran algunos artículos reales.
        builder.HasData(
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000001"),
                Codigo = "MED-001",
                Nombre = "Acetaminofén 500mg",
                StockActual = 320,
                Categoria = CategoriaInventario.Medicamento,
                StockMinimo = 100,
            },
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000002"),
                Codigo = "MED-002",
                Nombre = "Ibuprofeno 400mg",
                StockActual = 40,
                Categoria = CategoriaInventario.Medicamento,
                StockMinimo = 80,
            },
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000003"),
                Codigo = "MED-003",
                Nombre = "Amoxicilina 500mg",
                StockActual = 150,
                Categoria = CategoriaInventario.Medicamento,
                StockMinimo = 60,
            },
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000004"),
                Codigo = "MED-004",
                Nombre = "Loratadina 10mg",
                StockActual = 25,
                Categoria = CategoriaInventario.Medicamento,
                StockMinimo = 30,
            },
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000005"),
                Codigo = "INS-001",
                Nombre = "Guantes de nitrilo (caja)",
                StockActual = 60,
                Categoria = CategoriaInventario.Insumo,
                StockMinimo = 20,
            },
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000006"),
                Codigo = "INS-002",
                Nombre = "Jeringas 5ml",
                StockActual = 15,
                Categoria = CategoriaInventario.Insumo,
                StockMinimo = 40,
            },
            new
            {
                Id = Guid.Parse("2f1a1a2a-0001-4a3e-8a9a-000000000007"),
                Codigo = "INS-003",
                Nombre = "Gasas estériles (paquete)",
                StockActual = 90,
                Categoria = CategoriaInventario.Insumo,
                StockMinimo = 30,
            });
    }
}
