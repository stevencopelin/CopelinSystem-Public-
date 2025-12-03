using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class TaskList
    {
        public int TaskId { get; set; }
        public int? TaskProjectId { get; set; }
        public string? Task { get; set; }
        public string? Description { get; set; }
        public byte? Status { get; set; }
        public int? EstimatedDays { get; set; }
        public int? Progress { get; set; }
        public string? Dependencies { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateEnded { get; set; }

        // Navigation property for relationship with ProjectList
        public ProjectList? Project { get; set; }
    }

    public class TaskListConfiguration : IEntityTypeConfiguration<TaskList>
    {
        public void Configure(EntityTypeBuilder<TaskList> builder)
        {
            builder.ToTable("task_list");

            builder.HasKey(t => t.TaskId);

            builder.Property(t => t.TaskId)
                .HasColumnName("TaskId")
                .ValueGeneratedOnAdd();

            builder.Property(t => t.TaskProjectId)
                .HasColumnName("TaskProjectId");

            builder.Property(t => t.Task)
                .HasColumnName("Task")
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("nvarchar(max)");

            builder.Property(t => t.Status)
                .HasColumnName("Status");

            builder.Property(t => t.EstimatedDays)
                .HasColumnName("EstimatedDays");

            builder.Property(t => t.Progress)
                .HasColumnName("Progress");

            builder.Property(t => t.Dependencies)
                .HasColumnName("Dependencies")
                .HasMaxLength(255);

            builder.Property(t => t.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.DateEnded)
                .HasColumnName("DateEnded")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            // Define foreign key relationship
            builder.HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.TaskProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}