using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;  // e.g., "ViewProjects", "EditProjects"
        public string Category { get; set; } = string.Empty;        // e.g., "Projects", "Clients", "Users"
        public string DisplayName { get; set; } = string.Empty;     // Human-readable name
        public string? Description { get; set; }                    // What this permission allows
    }

    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(p => p.PermissionId);

            builder.Property(p => p.PermissionId)
                .HasColumnName("PermissionId")
                .ValueGeneratedOnAdd();

            builder.Property(p => p.PermissionName)
                .HasColumnName("PermissionName")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Category)
                .HasColumnName("Category")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.DisplayName)
                .HasColumnName("DisplayName")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("Description")
                .HasMaxLength(500);

            // Create unique index on PermissionName
            builder.HasIndex(p => p.PermissionName)
                .IsUnique();
        }
    }
}
