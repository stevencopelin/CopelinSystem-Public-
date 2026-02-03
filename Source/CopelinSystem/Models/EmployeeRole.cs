using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public enum RoleType
    {
        Supervisor = 1,
        SeniorEstimator = 2,
        PrincipalEstimator = 3,
        PlanningManager = 4,      // Same permissions as Principal Estimator
        Estimator = 5,            // Can edit projects, add tasks and productivity
        ReadOnly = 6,             // Can only view data
        Administrator = 7         // Full system access
    }

    public class EmployeeRole
    {
        public int EmployeeRoleId { get; set; }
        public int EmployeeId { get; set; }
        public RoleType RoleType { get; set; }

        // Navigation property
        public Employee? Employee { get; set; }
    }

    public class EmployeeRoleConfiguration : IEntityTypeConfiguration<EmployeeRole>
    {
        public void Configure(EntityTypeBuilder<EmployeeRole> builder)
        {
            builder.ToTable("employee_roles");

            builder.HasKey(er => er.EmployeeRoleId);

            builder.Property(er => er.EmployeeRoleId)
                .HasColumnName("EmployeeRoleId")
                .ValueGeneratedOnAdd();

            builder.Property(er => er.EmployeeId)
                .HasColumnName("EmployeeId")
                .IsRequired();

            builder.Property(er => er.RoleType)
                .HasColumnName("RoleType")
                .HasConversion<int>()
                .IsRequired();

            // Define foreign key relationship with Employee
            builder.HasOne(er => er.Employee)
                .WithMany(e => e.EmployeeRoles)
                .HasForeignKey(er => er.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create unique index to prevent duplicate role assignments
            builder.HasIndex(er => new { er.EmployeeId, er.RoleType })
                .IsUnique();
        }
    }
}
