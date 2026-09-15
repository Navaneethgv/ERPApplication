
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.CustomerId);
        builder.Property(e => e.CustomerId).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).HasMaxLength(150).IsRequired();
        builder.Property(e => e.ContactPerson).HasMaxLength(100);
        builder.Property(e => e.Email).HasMaxLength(150);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Company).HasMaxLength(150);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.CreditLimit).HasPrecision(18, 2);
        builder.Property(e => e.CurrentBalance).HasPrecision(18, 2);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
    }
}


