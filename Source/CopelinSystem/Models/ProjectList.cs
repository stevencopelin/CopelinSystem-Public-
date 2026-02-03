using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopelinSystem.Models
{
    public class ProjectList
    {
        public int ProjectId { get; set; }
        
        [Required(ErrorMessage = "Project Name is required")]
        public string? ProjectName { get; set; }

        [Required(ErrorMessage = "Project Description is required")]
        public string? ProjectDescription { get; set; }
        public byte? ProjectStatus { get; set; }
        
        [Required(ErrorMessage = "Work Request (WR) is required")]
        public string? ProjectWr { get; set; }
        public string? ProjectComponentisation { get; set; }
        public string? ProjectFeeprop { get; set; }
        public string? ProjectWo { get; set; }
        public string? ProjectReqOrder { get; set; }
        public string? ProjectOneSchool { get; set; }
        public string? ProjectConsultant { get; set; }
        public string? ProjectFolderPath { get; set; }

        [Required(ErrorMessage = "Project Location is required")]
        public string? ProjectLocation { get; set; }
        public string? ProjectRegion { get; set; }
        public string? ProjectContact { get; set; }
        public string? ProjectContactEmail { get; set; }
        public string? ProjectSupervisor { get; set; }
        public string? ProjectSeniorEstimator { get; set; }
        public string? ProjectEllipesStatus { get; set; }
        public string? ProjectQuoteTracker { get; set; }
        public string? ProjectPurchaseOrder { get; set; }
        public string? ProjectIrisNo { get; set; }
        public string? ProjectEtenderNo { get; set; }
        public string? ProjectIndicative { get; set; }
        public string? ProjectActualPrice { get; set; }
        public string? ProjectWicNo { get; set; }
        [Required(ErrorMessage = "Works Type is required")]
        public string? ProjectWorkstype { get; set; }
        public string? ProjectProgramCode { get; set; }
        public string? ProjectClient { get; set; }
        public string? ProjectContractor { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public int? ProjectManagerID { get; set; }
        public string? ProjectUserIds { get; set; }
        public DateTime ProjectDateCreated { get; set; }
        public DateTime? ProjectWrRequested { get; set; }

        [Required(ErrorMessage = "Client Required by date is required")]
        public DateTime? ProjectClientRequired { get; set; }

        [Required(ErrorMessage = "Client Completion date is required")]
        public DateTime? ProjectClientCompletion { get; set; }
        public DateTime? ProjectTendered { get; set; }
        public DateTime? ProjectEstEval { get; set; }
        public DateTime? ProjectComplete { get; set; }
    }

    public class ProjectListConfiguration : IEntityTypeConfiguration<ProjectList>
    {
        public void Configure(EntityTypeBuilder<ProjectList> builder)
        {
            builder.ToTable("project_list");

            builder.HasKey(p => p.ProjectId);

            builder.Property(p => p.ProjectId)
                .HasColumnName("ProjectId")
                .ValueGeneratedOnAdd();

            builder.Property(p => p.ProjectName)
                .HasColumnName("ProjectName")
                .HasMaxLength(200);

            builder.Property(p => p.ProjectDescription)
                .HasColumnName("ProjectDescription")
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.ProjectStatus)
                .HasColumnName("ProjectStatus");

            builder.Property(p => p.ProjectWr)
                .HasColumnName("ProjectWr")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectComponentisation)
                .HasColumnName("ProjectComponentisation")
                .HasMaxLength(11);

            builder.Property(p => p.ProjectFeeprop)
                .HasColumnName("ProjectFeeprop")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectWo)
                .HasColumnName("ProjectWo")
                .HasMaxLength(15);

            builder.Property(p => p.ProjectReqOrder)
                .HasColumnName("ProjectReqOrder")
                .HasMaxLength(50);

            builder.Property(p => p.ProjectOneSchool)
                .HasColumnName("ProjectOneSchool")
                .HasMaxLength(15);

            builder.Property(p => p.ProjectConsultant)
                .HasColumnName("ProjectConsultant")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectFolderPath)
                .HasColumnName("ProjectFolderPath")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectLocation)
                .HasColumnName("ProjectLocation")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectRegion)
                .HasColumnName("ProjectRegion")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectContact)
                .HasColumnName("ProjectContact")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectContactEmail)
                .HasColumnName("ProjectContactEmail")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectSupervisor)
                .HasColumnName("ProjectSupervisor")
                .HasMaxLength(15);

            builder.Property(p => p.ProjectSeniorEstimator)
                .HasColumnName("ProjectSeniorEstimator")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectEllipesStatus)
                .HasColumnName("ProjectEllipesStatus")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectQuoteTracker)
                .HasColumnName("ProjectQuoteTracker")
                .HasMaxLength(256);

            builder.Property(p => p.ProjectPurchaseOrder)
                .HasColumnName("ProjectPurchaseOrder")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectIrisNo)
                .HasColumnName("ProjectIrisNo")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectEtenderNo)
                .HasColumnName("ProjectEtenderNo")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectIndicative)
                .HasColumnName("ProjectIndicative")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectActualPrice)
                .HasColumnName("ProjectActualPrice")
                .HasMaxLength(15);

            builder.Property(p => p.ProjectWicNo)
                .HasColumnName("ProjectWicNo")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectWorkstype)
                .HasColumnName("ProjectWorkstype")
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(p => p.ProjectProgramCode)
                .HasColumnName("ProjectProgramCode")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectClient)
                .HasColumnName("ProjectClient")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectContractor)
                .HasColumnName("ProjectContractor")
                .HasMaxLength(255);

            builder.Property(p => p.ProjectStartDate)
                .HasColumnName("ProjectStartDate")
                .HasColumnType("date");

            builder.Property(p => p.ProjectEndDate)
                .HasColumnName("ProjectEndDate")
                .HasColumnType("date");

            builder.Property(p => p.ProjectManagerID)
                .HasColumnName("ProjectManagerID");

            builder.Property(p => p.ProjectUserIds)
                .HasColumnName("ProjectUserIds")
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.ProjectDateCreated)
                .HasColumnName("ProjectDateCreated")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            builder.Property(p => p.ProjectWrRequested)
                .HasColumnName("ProjectWrRequested")
                .HasColumnType("date");

            builder.Property(p => p.ProjectClientRequired)
                .HasColumnName("ProjectClientRequired")
                .HasColumnType("date");

            builder.Property(p => p.ProjectClientCompletion)
                .HasColumnName("ProjectClientCompletion")
                .HasColumnType("date");

            builder.Property(p => p.ProjectTendered)
                .HasColumnName("ProjectTendered")
                .HasColumnType("date");

            builder.Property(p => p.ProjectEstEval)
                .HasColumnName("ProjectEstEval")
                .HasColumnType("date");

            builder.Property(p => p.ProjectComplete)
                .HasColumnName("ProjectComplete")
                .HasColumnType("date");
        }
    }
}

