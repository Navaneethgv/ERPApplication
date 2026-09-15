using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.HasKey(e => e.MenuId);
        builder.Property(e => e.MenuId).ValueGeneratedOnAdd();
        builder.Property(e => e.Title).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Route).HasMaxLength(100);
        builder.Property(e => e.Icon).HasMaxLength(100);
        builder.Property(e => e.SortOrder).HasDefaultValue(0);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasOne(e => e.ParentMenu)
            .WithMany(m => m.SubMenus)
            .HasForeignKey(e => e.ParentMenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
