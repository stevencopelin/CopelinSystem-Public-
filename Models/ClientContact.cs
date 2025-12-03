using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class ClientContact
    {
        public int ClientContactId { get; set; }
        public int ClientId { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Navigation property
        public Client? Client { get; set; }
    }

    public class ClientContactConfiguration : IEntityTypeConfiguration<ClientContact>
    {
        public void Configure(EntityTypeBuilder<ClientContact> builder)
        {
            builder.ToTable("client_contacts");

            builder.HasKey(cc => cc.ClientContactId);

            builder.Property(cc => cc.ClientContactId)
                .HasColumnName("ClientContactId")
                .ValueGeneratedOnAdd();

            builder.Property(cc => cc.ClientId)
                .HasColumnName("ClientId")
                .IsRequired();

            builder.Property(cc => cc.ContactName)
                .HasColumnName("ContactName")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(cc => cc.ContactEmail)
                .HasColumnName("ContactEmail")
                .HasMaxLength(255);

            builder.Property(cc => cc.ContactPhone)
                .HasColumnName("ContactPhone")
                .HasMaxLength(50);

            builder.Property(cc => cc.IsPrimary)
                .HasColumnName("IsPrimary")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(cc => cc.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(cc => cc.DateCreated)
                .HasColumnName("DateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // Define foreign key relationship with Client
            builder.HasOne(cc => cc.Client)
                .WithMany(c => c.ClientContacts)
                .HasForeignKey(cc => cc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
