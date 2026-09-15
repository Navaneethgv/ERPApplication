
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.SupplierId);
        builder.Property(e => e.SupplierId).ValueGeneratedOnAdd();
        builder.Property(e => e.SupplierCode).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.SupplierCode).IsUnique();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ContactPerson).HasMaxLength(100);
        builder.Property(e => e.Email).HasMaxLength(150);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.PaymentTerms).HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
    }
}


