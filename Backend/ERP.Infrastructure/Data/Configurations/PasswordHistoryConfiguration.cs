using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.HasKey(e => e.PasswordHistoryId);
        builder.Property(e => e.PasswordHistoryId).ValueGeneratedOnAdd();
        builder.Property(e => e.PasswordHash).IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(u => u.PasswordHistories)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
