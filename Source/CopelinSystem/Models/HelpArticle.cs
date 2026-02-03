using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CopelinSystem.Models
{
    public class HelpArticle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(50)]
        public string MediaType { get; set; } = "None"; // None, Image, Video

        [MaxLength(500)]
        public string? MediaUrl { get; set; }

        public int HelpSectionId { get; set; }

        [ForeignKey("HelpSectionId")]
        public HelpSection? HelpSection { get; set; }

        public int Order { get; set; }
    }
}
