using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class ProjectEmailAttachment
    {
        public int AttachmentId { get; set; }
        
        [Required]
        public int EmailId { get; set; }
        
        [Required]
        public string FileName { get; set; } = "";
        
        [Required]
        public string FilePath { get; set; } = "";
        
        public long FileSize { get; set; }
        
        public string? ContentType { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public ProjectEmail? Email { get; set; }
    }

    public class ProjectEmailAttachmentConfiguration : IEntityTypeConfiguration<ProjectEmailAttachment>
    {
        public void Configure(EntityTypeBuilder<ProjectEmailAttachment> builder)
        {
            builder.ToTable("project_email_attachments");

            builder.HasKey(a => a.AttachmentId);

            builder.Property(a => a.AttachmentId)
                .HasColumnName("AttachmentId")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.EmailId)
                .HasColumnName("EmailId")
                .IsRequired();

            builder.Property(a => a.FileName)
                .HasColumnName("FileName")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.FilePath)
                .HasColumnName("FilePath")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.FileSize)
                .HasColumnName("FileSize");

            builder.Property(a => a.ContentType)
                .HasColumnName("ContentType")
                .HasMaxLength(100);

            builder.Property(a => a.CreatedDate)
                .HasColumnName("CreatedDate")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // Foreign key relationship with cascade delete
            builder.HasOne(a => a.Email)
                .WithMany()
                .HasForeignKey(a => a.EmailId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
