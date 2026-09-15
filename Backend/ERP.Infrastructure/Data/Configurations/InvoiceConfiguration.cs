
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(e => e.InvoiceId);
        builder.Property(e => e.InvoiceId).ValueGeneratedOnAdd();
        builder.Property(e => e.InvoiceNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.InvoiceNumber).IsUnique();
        builder.Property(e => e.SaleOrderNumber).HasMaxLength(100);
        builder.Property(e => e.CustomerName).HasMaxLength(200);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.PaidAmount).HasPrecision(18, 2);
        builder.Ignore(e => e.BalanceAmount);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(1000);
    }
}


