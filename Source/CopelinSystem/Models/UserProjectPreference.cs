using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class UserProjectPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int SortOrder { get; set; } // 0-based index

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ProjectList? Project { get; set; }
    }

    public class UserProjectPreferenceConfiguration : IEntityTypeConfiguration<UserProjectPreference>
    {
        public void Configure(EntityTypeBuilder<UserProjectPreference> builder)
        {
            builder.ToTable("user_project_preference");

            builder.HasKey(upp => upp.Id);

            builder.Property(upp => upp.Id)
                .ValueGeneratedOnAdd();

            builder.Property(upp => upp.SortOrder)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(upp => upp.User)
                .WithMany()
                .HasForeignKey(upp => upp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(upp => upp.Project)
                .WithMany()
                .HasForeignKey(upp => upp.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: A user can only have one preference entry per project
            builder.HasIndex(upp => new { upp.UserId, upp.ProjectId }).IsUnique();
        }
    }
}
