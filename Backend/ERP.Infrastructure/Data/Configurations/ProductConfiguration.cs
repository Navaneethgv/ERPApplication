
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.ProductId);
        builder.Property(e => e.ProductId).ValueGeneratedOnAdd();
        builder.Property(e => e.SKU).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.SKU).IsUnique();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.CostPrice).HasPrecision(18, 2);
        builder.Property(e => e.UnitOfMeasure).HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
    }
}


