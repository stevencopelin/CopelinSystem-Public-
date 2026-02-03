using System;
using System.ComponentModel.DataAnnotations;

namespace CopelinSystem.Models
{
    public class SubmissionNotificationDismissal
    {
        [Key]
        public int Id { get; set; }
        public Guid TokenId { get; set; }
        public int UserId { get; set; }
        public DateTime DismissedAt { get; set; } = DateTime.Now;

        // Navigation properties (optional, but good practice)
        public virtual SubmissionToken? Token { get; set; }
        public virtual User? User { get; set; }
    }
}
