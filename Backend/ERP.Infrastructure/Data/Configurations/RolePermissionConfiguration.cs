using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(e => e.RolePermissionId);
        builder.Property(e => e.RolePermissionId).ValueGeneratedOnAdd();
        builder.Property(e => e.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.IsGranted).HasDefaultValue(false);

        builder.HasIndex(e => new { e.Role, e.MenuId, e.MenuOptionId }).IsUnique();

        builder.HasOne(e => e.Menu)
            .WithMany(m => m.RolePermissions)
            .HasForeignKey(e => e.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.MenuOption)
            .WithMany(o => o.RolePermissions)
            .HasForeignKey(e => e.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
