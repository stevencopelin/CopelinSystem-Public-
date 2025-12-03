using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public int? RegionId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Navigation properties
        public Region? Region { get; set; }
        public ICollection<ClientContact> ClientContacts { get; set; } = new List<ClientContact>();
    }

    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("clients");

            builder.HasKey(c => c.ClientId);

            builder.Property(c => c.ClientId)
                .HasColumnName("ClientId")
                .ValueGeneratedOnAdd();

            builder.Property(c => c.ClientCode)
                .HasColumnName("ClientCode")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.ClientName)
                .HasColumnName("ClientName")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.RegionId)
                .HasColumnName("RegionId");

            builder.Property(c => c.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(c => c.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // Define foreign key relationship with Region
            builder.HasOne(c => c.Region)
                .WithMany()
                .HasForeignKey(c => c.RegionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
