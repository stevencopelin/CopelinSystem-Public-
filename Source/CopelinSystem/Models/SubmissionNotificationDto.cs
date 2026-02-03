using System;

namespace CopelinSystem.Models
{
    public class SubmissionNotificationDto
    {
        public Guid TokenId { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectWr { get; set; }
        public string? Department { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
