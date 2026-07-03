using FUNBIDE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("movimientos_inventario");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.InventarioItemId).IsRequired();
        builder.Property(m => m.UsuarioId).IsRequired();
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Cantidad).IsRequired();
        builder.Property(m => m.StockResultante).IsRequired();
        builder.Property(m => m.Referencia).HasMaxLength(200);
        builder.Property(m => m.RegistradoEn).IsRequired();

        builder.HasOne<InventarioItem>()
            .WithMany()
            .HasForeignKey(m => m.InventarioItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.InventarioItemId);
    }
}
