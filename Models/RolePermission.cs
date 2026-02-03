using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class RolePermission
    {
        public int RolePermissionId { get; set; }
        public byte RoleId { get; set; }              // Maps to UserRole enum
        public int PermissionId { get; set; }         // Foreign key to Permission
        public bool IsGranted { get; set; }           // Whether this role has this permission

        // Navigation property
        public Permission? Permission { get; set; }
    }

    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("role_permissions");

            builder.HasKey(rp => rp.RolePermissionId);

            builder.Property(rp => rp.RolePermissionId)
                .HasColumnName("RolePermissionId")
                .ValueGeneratedOnAdd();

            builder.Property(rp => rp.RoleId)
                .HasColumnName("RoleId")
                .IsRequired();

            builder.Property(rp => rp.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.Property(rp => rp.IsGranted)
                .HasColumnName("IsGranted")
                .HasDefaultValue(true);

            // Create composite unique index on (RoleId, PermissionId)
            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            // Configure relationship with Permission
            builder.HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
