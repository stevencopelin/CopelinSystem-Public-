using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class FileSystemItemConfiguration : IEntityTypeConfiguration<FileSystemItem>
    {
        public void Configure(EntityTypeBuilder<FileSystemItem> builder)
        {
            builder.ToTable("FileSystemItems");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("GETDATE()");
                
            builder.HasOne(d => d.Project)
                .WithMany()
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Parent)
                .WithMany()
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cycles or multiple cascade paths
        }
    }
}
