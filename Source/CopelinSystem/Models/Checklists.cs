using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CopelinSystem.Models
{
    // ---------------- Templates ----------------

    public class ChecklistTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = "General"; // e.g., WHS, Environmental

        public bool IsActive { get; set; } = true;
        
        [MaxLength(20)]
        public string Version { get; set; } = "1.0";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation
        public List<ChecklistSection> Sections { get; set; } = new();
    }

    public class ChecklistSection
    {
        [Key]
        public int SectionId { get; set; }

        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public ChecklistTemplate? Template { get; set; }

        [Required]
        [MaxLength(200)]
        public string SectionName { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        // Navigation
        public List<ChecklistQuestion> Questions { get; set; } = new();
    }

    public class ChecklistQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public ChecklistSection? Section { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public string QuestionType { get; set; } = "Text"; // Text, Radio, Checkbox, Dropdown, Date, TextArea, YesNo

        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? HelpText { get; set; }
        
        // Navigation
        public List<ChecklistQuestionOption> Options { get; set; } = new();
    }

    public class ChecklistQuestionOption
    {
        [Key]
        public int OptionId { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public ChecklistQuestion? Question { get; set; }

        [Required]
        public string OptionText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }


    // ---------------- Instances ----------------

    public class ProjectChecklist
    {
        [Key]
        public int ChecklistInstanceId { get; set; }

        public int ProjectId { get; set; }
        // Assuming loose coupling or navigation to ProjectList if needed
        // public ProjectList? Project { get; set; }

        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public ChecklistTemplate? Template { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "InProgress"; // NotStarted, InProgress, Completed, Reviewed

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? CompletedBy { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Navigation
        public List<ChecklistResponse> Responses { get; set; } = new();
    }

    public class ChecklistResponse
    {
        [Key]
        public int ResponseId { get; set; }

        public int ChecklistInstanceId { get; set; }
        [ForeignKey("ChecklistInstanceId")]
        public ProjectChecklist? ChecklistInstance { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public ChecklistQuestion? Question { get; set; }

        public string? ResponseValue { get; set; } // JSON or simple string
        public string? Notes { get; set; }

        public DateTime ResponseDate { get; set; } = DateTime.Now;
        public string RespondedBy { get; set; } = string.Empty;
    }
}
