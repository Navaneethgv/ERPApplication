
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(e => e.TransactionId);
        builder.Property(e => e.TransactionId).ValueGeneratedOnAdd();
        builder.Property(e => e.TransactionNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.TransactionNumber).IsUnique();
        builder.Property(e => e.ReferenceType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.PaymentMethod).HasMaxLength(100);
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(1000);
    builder.Property(e => e.CreatedBy).HasMaxLength(100);
    }
}


