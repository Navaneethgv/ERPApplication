
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(e => e.InventoryId);
        builder.Property(e => e.InventoryId).ValueGeneratedOnAdd();
        builder.Property(e => e.ProductName).HasMaxLength(200);
        builder.Property(e => e.SKU).HasMaxLength(100);
        builder.Property(e => e.Location).HasMaxLength(100);
        builder.Ignore(e => e.AvailableQuantity);
    }
}


