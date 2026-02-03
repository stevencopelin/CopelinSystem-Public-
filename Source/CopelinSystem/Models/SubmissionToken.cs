using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CopelinSystem.Models
{
    public class SubmissionToken
    {
        [Key]
        public Guid Token { get; set; }

        public int ProjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Department { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        public bool IsNotificationDismissed { get; set; } = false;
    }
}
