using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectList> ProjectLists { get; set; }

        public DbSet<TaskList> TaskLists { get; set; }

        public DbSet<UserProductivity> UserProductivities { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<ClientContact> ClientContacts { get; set; }

        public DbSet<Consultant> Consultants { get; set; }

        public DbSet<Contractor> Contractors { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeRole> EmployeeRoles { get; set; }

        public DbSet<Region> Regions { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<TaskConfiguration> TaskConfigurations { get; set; }

        public DbSet<SubmissionToken> SubmissionTokens { get; set; }
        public DbSet<SubmissionNotificationDismissal> SubmissionNotificationDismissals { get; set; }

        public DbSet<ProjectEmail> ProjectEmails { get; set; }

        public DbSet<ProjectEmailAttachment> ProjectEmailAttachments { get; set; }

        public DbSet<UserProjectPreference> UserProjectPreferences { get; set; }

        public DbSet<HelpSection> HelpSections { get; set; }
        
        public DbSet<HelpArticle> HelpArticles { get; set; }

        public DbSet<AppBranding> AppBranding { get; set; }

        public virtual DbSet<ExternalRegionEmail> ExternalRegionEmails { get; set; }

        public DbSet<FileSystemItem> FileSystemItems { get; set; }

        // Checklist / ISO Forms
        public DbSet<ChecklistTemplate> ChecklistTemplates { get; set; }
        public DbSet<ChecklistSection> ChecklistSections { get; set; }
        public DbSet<ChecklistQuestion> ChecklistQuestions { get; set; }
        public DbSet<ChecklistQuestionOption> ChecklistQuestionOptions { get; set; }
        public DbSet<ProjectChecklist> ProjectChecklists { get; set; }
        public DbSet<ChecklistResponse> ChecklistResponses { get; set; }

        // Ellipse Status Codes
        public DbSet<StatusCode> StatusCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensures identity configuration is applied

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RegionConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectListConfiguration());
            modelBuilder.ApplyConfiguration(new UserProjectPreferenceConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            modelBuilder.ApplyConfiguration(new ConsultantConfiguration());
            modelBuilder.ApplyConfiguration(new ContractorConfiguration());
            modelBuilder.ApplyConfiguration(new TaskConfigurationConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalRegionEmailConfiguration());
            modelBuilder.ApplyConfiguration(new TaskListConfiguration());
            modelBuilder.ApplyConfiguration(new UserProductivityConfiguration());
            modelBuilder.ApplyConfiguration(new ClientContactConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectEmailConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectEmailAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new FileSystemItemConfiguration());

            modelBuilder.Entity<AppBranding>().HasData(
                new AppBranding
                {
                    Id = 1,
                    FooterHtml = @"<footer class=""main-footer"">
    <strong> {{Year}} <a href=""#"">Estimating Module | Copelin System</a> - </strong>
    Qld Governement - QBuild.
    <div class=""float-right d-none d-sm-inline-block"">
        <b>Version</b> {{Version}}
    </div>
</footer>",
                    IsLocked = true
                }
            );
        }
    }
}