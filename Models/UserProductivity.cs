using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class UserProductivity
    {
        public int ProductivityId { get; set; }
        public int? ProductivityProjectId { get; set; }
        public int? ProductivityTaskId { get; set; }
        public string? ProductivityComment { get; set; }
        public string? ProductivitySubject { get; set; }
        public DateTime? ProductivityDate { get; set; }
        public TimeSpan? ProductivityStartTime { get; set; }
        public TimeSpan? ProductivityEndTime { get; set; }
        public int? ProductivityUserId { get; set; }
        public float? ProductivityTimeRendered { get; set; }
        public DateTime ProductivityDateCreated { get; set; }

        // Navigation properties for relationships
        public ProjectList? Project { get; set; }
        public TaskList? Task { get; set; }
    }

    public class UserProductivityConfiguration : IEntityTypeConfiguration<UserProductivity>
    {
        public void Configure(EntityTypeBuilder<UserProductivity> builder)
        {
            builder.ToTable("user_productivity");

            builder.HasKey(p => p.ProductivityId);

            builder.Property(p => p.ProductivityId)
                .HasColumnName("ProductivityId")
                .ValueGeneratedOnAdd();

            builder.Property(p => p.ProductivityProjectId)
                .HasColumnName("ProductivityProjectId");

            builder.Property(p => p.ProductivityTaskId)
                .HasColumnName("ProductivityTaskId");

            builder.Property(p => p.ProductivityComment)
                .HasColumnName("ProductivityComment")
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.ProductivitySubject)
                .HasColumnName("ProductivitySubject")
                .HasMaxLength(200);

            builder.Property(p => p.ProductivityDate)
                .HasColumnName("ProductivityDate")
                .HasColumnType("date");

            builder.Property(p => p.ProductivityStartTime)
                .HasColumnName("ProductivityStartTime")
                .HasColumnType("time");

            builder.Property(p => p.ProductivityEndTime)
                .HasColumnName("ProductivityEndTime")
                .HasColumnType("time");

            builder.Property(p => p.ProductivityUserId)
                .HasColumnName("ProductivityUserId");

            builder.Property(p => p.ProductivityTimeRendered)
                .HasColumnName("ProductivityTimeRendered")
                .HasColumnType("real");

            builder.Property(p => p.ProductivityDateCreated)
                .HasColumnName("ProductivityDateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            // Define foreign key relationships
            builder.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProductivityProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Task)
                .WithMany()
                .HasForeignKey(p => p.ProductivityTaskId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}