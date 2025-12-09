using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class Consultant
    {
        public int ConsultantId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Services { get; set; }
        public string? SupplierNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

    public class ConsultantConfiguration : IEntityTypeConfiguration<Consultant>
    {
        public void Configure(EntityTypeBuilder<Consultant> builder)
        {
            builder.ToTable("consultants");

            builder.HasKey(c => c.ConsultantId);

            builder.Property(c => c.ConsultantId)
                .HasColumnName("ConsultantId")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.BusinessName)
                .HasColumnName("BusinessName")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.Contact)
                .HasColumnName("Contact")
                .HasMaxLength(255);

            builder.Property(c => c.Email)
                .HasColumnName("Email")
                .HasMaxLength(255);

            builder.Property(c => c.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(50);

            builder.Property(c => c.Address)
                .HasColumnName("Address")
                .HasMaxLength(500);

            builder.Property(c => c.Services)
                .HasColumnName("Services")
                .HasMaxLength(500);

            builder.Property(c => c.SupplierNumber)
                .HasColumnName("SupplierNumber")
                .HasMaxLength(100);

            builder.Property(c => c.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(c => c.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();
        }
    }
}
