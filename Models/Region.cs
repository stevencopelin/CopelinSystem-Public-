using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class Region
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ToTable("regions");

            builder.HasKey(r => r.RegionId);

            builder.Property(r => r.RegionId)
                .HasColumnName("RegionId")
                .ValueGeneratedOnAdd();

            builder.Property(r => r.RegionName)
                .HasColumnName("RegionName")
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
