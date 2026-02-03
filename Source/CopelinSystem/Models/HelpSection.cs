using System.ComponentModel.DataAnnotations;

namespace CopelinSystem.Models
{
    public class HelpSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<HelpArticle> Articles { get; set; } = new();
    }
}
