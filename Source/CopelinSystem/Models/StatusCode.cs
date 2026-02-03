using System.ComponentModel.DataAnnotations;

namespace CopelinSystem.Models
{
    public class StatusCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Combines Code and Description (e.g., "AC - Access Issues")
        /// </summary>
        public string FullStatus => $"{Code} - {Description}";
    }
}
