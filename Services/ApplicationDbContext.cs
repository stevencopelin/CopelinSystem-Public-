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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ProjectListConfiguration());

            modelBuilder.ApplyConfiguration(new TaskListConfiguration());

            modelBuilder.ApplyConfiguration(new UserProductivityConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new ClientConfiguration());

            modelBuilder.ApplyConfiguration(new ClientContactConfiguration());

            modelBuilder.ApplyConfiguration(new ConsultantConfiguration());

            modelBuilder.ApplyConfiguration(new ContractorConfiguration());

            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());

            modelBuilder.ApplyConfiguration(new EmployeeRoleConfiguration());

            modelBuilder.ApplyConfiguration(new RegionConfiguration());

            modelBuilder.ApplyConfiguration(new PermissionConfiguration());

            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        }
    }
}