using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CopelinSystem.Models
{
    public class FileSystemItem
    {
        [Key]
        public int Id { get; set; }

        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual ProjectList? Project { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual FileSystemItem? Parent { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public bool IsFolder { get; set; }

        public string? PhysicalPath { get; set; } // Relative path

        public string? ContentType { get; set; }

        public long Size { get; set; }

        public string? CreatedBy { get; set; } // UserId or Name

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}
