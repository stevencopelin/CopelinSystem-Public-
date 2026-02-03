using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class TaskConfiguration
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        
        [Required]
        public string TaskName { get; set; } = string.Empty;
        
        public int Duration { get; set; }
        public bool IsValueBased { get; set; }
        public string? ValueThresholds { get; set; } // JSON string

        // Navigation property
        [ForeignKey("RegionId")]
        public virtual Region? Region { get; set; }
    }

    public class TaskConfigurationConfiguration : IEntityTypeConfiguration<TaskConfiguration>
    {
        public void Configure(EntityTypeBuilder<TaskConfiguration> builder)
        {
            builder.ToTable("task_configurations");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TaskName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(t => t.ValueThresholds)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(t => t.Region)
                .WithMany()
                .HasForeignKey(t => t.RegionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
