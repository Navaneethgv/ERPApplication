using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class PasswordPolicyConfiguration : IEntityTypeConfiguration<PasswordPolicy>
{
    public void Configure(EntityTypeBuilder<PasswordPolicy> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.MinLength).HasDefaultValue(8);
        builder.Property(e => e.RequireUppercase).HasDefaultValue(true);
        builder.Property(e => e.RequireLowercase).HasDefaultValue(true);
        builder.Property(e => e.RequireDigit).HasDefaultValue(true);
        builder.Property(e => e.RequireSpecialChar).HasDefaultValue(true);
        builder.Property(e => e.ExpiryDays).HasDefaultValue(90);
        builder.Property(e => e.HistoryCount).HasDefaultValue(5);
        builder.Property(e => e.MaxFailedAttempts).HasDefaultValue(5);
        builder.Property(e => e.LockoutDurationMinutes).HasDefaultValue(15);
        
    }
}
