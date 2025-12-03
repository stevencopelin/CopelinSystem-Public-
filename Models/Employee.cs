using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? RegionId { get; set; }
        public bool Active { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Navigation properties
        public Region? Region { get; set; }
        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    }

    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("employees");

            builder.HasKey(e => e.EmployeeId);

            builder.Property(e => e.EmployeeId)
                .HasColumnName("EmployeeId")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.FullName)
                .HasColumnName("FullName")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.RegionId)
                .HasColumnName("RegionId");

            builder.Property(e => e.Active)
                .HasColumnName("Active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(e => e.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // Define foreign key relationship with Region
            builder.HasOne(e => e.Region)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
