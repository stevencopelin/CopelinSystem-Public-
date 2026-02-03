using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class ProjectEmail
    {
        public int EmailId { get; set; }
        
        public int? ProjectId { get; set; }
        
        public string? ProjectWr { get; set; }
        
        [Required]
        public string EmailSubject { get; set; } = "";
        
        [Required]
        public string EmailFrom { get; set; } = "";
        
        public string? EmailTo { get; set; }
        
        public string? EmailBody { get; set; }
        
        public string? EmailBodyHtml { get; set; }
        
        [Required]
        public DateTime ReceivedDate { get; set; }
        
        public DateTime? ProcessedDate { get; set; }
        
        public bool IsMatched { get; set; } = false;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public ProjectList? Project { get; set; }
        
        public List<ProjectEmailAttachment>? Attachments { get; set; }
    }

    public class ProjectEmailConfiguration : IEntityTypeConfiguration<ProjectEmail>
    {
        public void Configure(EntityTypeBuilder<ProjectEmail> builder)
        {
            builder.ToTable("project_emails");

            builder.HasKey(e => e.EmailId);

            builder.Property(e => e.EmailId)
                .HasColumnName("EmailId")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ProjectId)
                .HasColumnName("ProjectId");

            builder.Property(e => e.ProjectWr)
                .HasColumnName("ProjectWr")
                .HasMaxLength(255);

            builder.Property(e => e.EmailSubject)
                .HasColumnName("EmailSubject")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.EmailFrom)
                .HasColumnName("EmailFrom")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.EmailTo)
                .HasColumnName("EmailTo")
                .HasMaxLength(255);

            builder.Property(e => e.EmailBody)
                .HasColumnName("EmailBody")
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.EmailBodyHtml)
                .HasColumnName("EmailBodyHtml")
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.ReceivedDate)
                .HasColumnName("ReceivedDate")
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(e => e.ProcessedDate)
                .HasColumnName("ProcessedDate")
                .HasColumnType("datetime");

            builder.Property(e => e.IsMatched)
                .HasColumnName("IsMatched")
                .HasDefaultValue(false);

            builder.Property(e => e.CreatedDate)
                .HasColumnName("CreatedDate")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // Foreign key relationship
            builder.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
