using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class MenuOptionConfiguration : IEntityTypeConfiguration<MenuOption>
{
    public void Configure(EntityTypeBuilder<MenuOption> builder)
    {
        builder.HasKey(e => e.MenuOptionId);
        builder.Property(e => e.MenuOptionId).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(200);
        builder.Property(e => e.SortOrder).HasDefaultValue(0);

        builder.HasIndex(e => new { e.MenuId, e.Code }).IsUnique();

        builder.HasOne(e => e.Menu)
            .WithMany(m => m.MenuOptions)
            .HasForeignKey(e => e.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
