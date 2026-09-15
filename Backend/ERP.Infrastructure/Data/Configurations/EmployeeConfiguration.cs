
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.EmployeeId);
        builder.Property(e => e.EmployeeId).ValueGeneratedOnAdd();
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100);
        builder.Ignore(e => e.FullName);
        builder.Property(e => e.Email).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Department).HasMaxLength(100);
        builder.Property(e => e.Designation).HasMaxLength(100);
        builder.Property(e => e.Salary).HasPrecision(18, 2);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
    }
}


