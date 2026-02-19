using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    // Enum for user roles
    public enum UserRole : byte
    {
        ReadOnly = 1,        // Can only view data
        Estimator = 2,       // Can edit projects, add tasks and add productivity
        Manager = 3,         // Can delete projects, manage teams plus estimator permissions
        PrincipalEstimator = 4,  // Can manage users in own region, all manager rights
        Admin = 5            // Full system access
    }

    public class User
    {
        public int UserId { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public string? Region { get; set; }  // User's region for regional management

        // Active Directory fields
        public string? AdUsername { get; set; }  // e.g., "jsmith" or "DOMAIN\jsmith"
        public string? AdDomain { get; set; }    // e.g., "COPELIN"
        public string? AdSid { get; set; }       // Security Identifier from AD

        public byte UserType { get; set; }  // Maps to UserRole enum
        public string? PasswordHash { get; set; } // Password hash for username/password authentication
        public string? Avatar { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastActive { get; set; }

        // Computed properties
        public string DisplayName => $"{Firstname} {Lastname}".Trim();
        public UserRole Role => (UserRole)UserType;
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.UserId);

            builder.Property(u => u.UserId)
                .HasColumnName("UserId")
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Firstname)
                .HasColumnName("Firstname")
                .HasMaxLength(200);

            builder.Property(u => u.Lastname)
                .HasColumnName("Lastname")
                .HasMaxLength(200);

            builder.Property(u => u.Email)
                .HasColumnName("Email")
                .HasMaxLength(200);

            builder.Property(u => u.Region)
                .HasColumnName("Region")
                .HasMaxLength(100);

            // Active Directory fields
            builder.Property(u => u.AdUsername)
                .HasColumnName("AdUsername")
                .HasMaxLength(200);

            builder.Property(u => u.AdDomain)
                .HasColumnName("AdDomain")
                .HasMaxLength(100);

            builder.Property(u => u.AdSid)
                .HasColumnName("AdSid")
                .HasMaxLength(200);

            builder.Property(u => u.UserType)
                .HasColumnName("UserType")
                .HasDefaultValue(1)  // Default to ReadOnly
                .HasComment("1=ReadOnly, 2=Estimator, 3=Manager, 4=PrincipalEstimator, 5=Admin");

            builder.Property(u => u.PasswordHash)
                .HasColumnName("PasswordHash");

            builder.Property(u => u.Avatar)
                .HasColumnName("Avatar")
                .HasColumnType("nvarchar(max)");

            builder.Property(u => u.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.LastActive)
                .HasColumnName("LastActive")
                .HasColumnType("datetime");

            // Create unique index on AdUsername for quick lookups
            builder.HasIndex(u => u.AdUsername)
                .IsUnique()
                .HasFilter("[AdUsername] IS NOT NULL");

            // Create index on AdSid for AD authentication
            builder.HasIndex(u => u.AdSid)
                .HasFilter("[AdSid] IS NOT NULL");

            // Create index on Region for regional queries
            builder.HasIndex(u => u.Region);
        }
    }
}