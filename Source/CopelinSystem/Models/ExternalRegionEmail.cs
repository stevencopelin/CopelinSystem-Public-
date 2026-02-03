using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class ExternalRegionEmail
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string Department { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;

        // Navigation property
        public Region? Region { get; set; }
    }

    public class ExternalRegionEmailConfiguration : IEntityTypeConfiguration<ExternalRegionEmail>
    {
        public void Configure(EntityTypeBuilder<ExternalRegionEmail> builder)
        {
            builder.ToTable("external_region_emails");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RegionId)
                .HasColumnName("RegionId")
                .IsRequired();

            builder.Property(e => e.Department)
                .HasColumnName("Department")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.EmailAddress)
                .HasColumnName("EmailAddress")
                .HasMaxLength(255)
                .IsRequired();

            // Configure relationship
            builder.HasOne(e => e.Region)
                .WithMany()
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique combination of Region and Department
            builder.HasIndex(e => new { e.RegionId, e.Department }).IsUnique();
        }
    }
}
