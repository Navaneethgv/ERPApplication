
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.NotificationId);
        builder.Property(e => e.NotificationId).ValueGeneratedOnAdd();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(1000);
        builder.Property(e => e.Type).HasMaxLength(50);
        builder.Property(e => e.OrderNumber).HasMaxLength(100);
        builder.Property(e => e.CustomerName).HasMaxLength(200);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.OrderStatus).HasMaxLength(50);
        builder.Property(e => e.TargetRole).HasMaxLength(50);
    }
}


