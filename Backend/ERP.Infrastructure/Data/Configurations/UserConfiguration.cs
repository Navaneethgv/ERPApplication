using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.UserId).ValueGeneratedOnAdd();
        builder.Property(e => e.Username).HasMaxLength(150).IsRequired();
        builder.HasIndex(e => e.Username).IsUnique();
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.Role).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.FullName).HasMaxLength(150);

        builder.Property(e => e.PasswordChangedAt).IsRequired();
        builder.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
        builder.Property(e => e.LockoutEnd);
    }
}
