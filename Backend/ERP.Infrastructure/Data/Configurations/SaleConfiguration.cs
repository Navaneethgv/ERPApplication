
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>, IEntityTypeConfiguration<SalesItem>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(e => e.SaleId);
        builder.Property(e => e.SaleId).ValueGeneratedOnAdd();
        builder.Property(e => e.SaleOrderNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.SaleOrderNumber).IsUnique();
        builder.Property(e => e.CustomerName).HasMaxLength(200);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);

        builder.HasMany(e => e.Items)
               .WithOne()
               .HasForeignKey(i => i.SaleId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<SalesItem> builder)
    {
        builder.HasKey(e => e.SalesItemId);
        builder.Property(e => e.SalesItemId).ValueGeneratedOnAdd();
        builder.Property(e => e.ProductName).HasMaxLength(200);
        builder.Property(e => e.SKU).HasMaxLength(100);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(e => e.TotalPrice);
    }
}


