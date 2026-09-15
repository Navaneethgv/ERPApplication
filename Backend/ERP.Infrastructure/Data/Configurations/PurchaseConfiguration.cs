
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>, IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasKey(e => e.PurchaseId);
        builder.Property(e => e.PurchaseId).ValueGeneratedOnAdd();
        builder.Property(e => e.PurchaseNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.PurchaseNumber).IsUnique();
        builder.Property(e => e.SupplierName).HasMaxLength(200);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);

        builder.HasMany(e => e.Items)
               .WithOne()
               .HasForeignKey(i => i.PurchaseId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.HasKey(e => e.PurchaseItemId);
        builder.Property(e => e.PurchaseItemId).ValueGeneratedOnAdd();
        builder.Property(e => e.ProductName).HasMaxLength(200);
        builder.Property(e => e.SKU).HasMaxLength(100);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(e => e.TotalPrice);
    }
}


