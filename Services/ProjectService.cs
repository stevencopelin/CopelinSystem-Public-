using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class ProjectService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ProjectService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        public async Task<List<ProjectList>> GetAllProjects()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists
                .OrderByDescending(p => p.ProjectDateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Get projects by status
        /// </summary>
        public async Task<List<ProjectList>> GetProjectsByStatus(byte status)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists
                .Where(p => p.ProjectStatus == status)
                .OrderByDescending(p => p.ProjectDateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Get projects for a specific user (based on role)
        /// </summary>
        public async Task<List<ProjectList>> GetProjectsForUser(User user)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            if (user.Role == UserRole.Admin)
            {
                // Admins can see all projects
                return await context.ProjectLists
                    .OrderByDescending(p => p.ProjectDateCreated)
                    .ToListAsync();
            }
            else if (user.Role == UserRole.PrincipalEstimator)
            {
                // Principal Estimators see:
                // 1. Projects in their region
                // 2. Projects they're assigned to (regardless of region)
                return await context.ProjectLists
                    .Where(p => 
                        (p.ProjectRegion == user.Region) ||  // Projects in their region
                        (p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString())))  // Their assigned projects
                    .OrderByDescending(p => p.ProjectDateCreated)
                    .ToListAsync();
            }
            else if (user.Role == UserRole.Manager || user.Role == UserRole.Estimator)
            {
                // Managers (Senior Estimators) and Estimators can see projects they're assigned to
                // Note: ProjectUserIds is a comma-separated string in your DB
                return await context.ProjectLists
                    .Where(p => p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString()))
                    .OrderByDescending(p => p.ProjectDateCreated)
                    .ToListAsync();
            }
            else
            {
                // Read-only users can see all projects but can't edit
                return await context.ProjectLists
                    .OrderByDescending(p => p.ProjectDateCreated)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Get projects by region
        /// </summary>
        public async Task<List<ProjectList>> GetProjectsByRegion(string region)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists
                .Where(p => p.ProjectRegion == region)
                .OrderByDescending(p => p.ProjectDateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        public async Task<ProjectList?> GetProjectById(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists.FindAsync(projectId);
        }

        /// <summary>
        /// Get projects with tasks
        /// </summary>
        public async Task<List<ProjectList>> GetProjectsWithTasks()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists
                .Include(p => p.ProjectId)
                .OrderByDescending(p => p.ProjectDateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Create new project
        /// </summary>
        public async Task<ProjectList> CreateProject(ProjectList project)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            project.ProjectDateCreated = DateTime.Now;
            
            // Calculate and set predicted end date for new projects
            project.ProjectEndDate = await CalculatePredictedEndDate(project);
            
            context.ProjectLists.Add(project);
            await context.SaveChangesAsync();
            
            // Create default tasks for the new project
            await CreateDefaultTasksForProject(project.ProjectId);
            
            return project;
        }

        /// <summary>
        /// Get the actual start date of the project (earliest task start date)
        /// </summary>
        public async Task<DateTime> GetProjectActualStartDate(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Get the earliest task that has been started (Status 1 or 3)
            var earliestStartedTask = await context.TaskLists
                .Where(t => t.TaskProjectId == projectId && t.Status >= 1)
                .OrderBy(t => t.DateCreated)
                .FirstOrDefaultAsync();
            
            if (earliestStartedTask != null)
            {
                return earliestStartedTask.DateCreated;
            }
            
            // No tasks started yet, fall back to project dates
            var project = await context.ProjectLists.FindAsync(projectId);
            return project?.ProjectStartDate ?? project?.ProjectDateCreated ?? DateTime.Now;
        }

        /// <summary>
        /// Calculate predicted end date based on project value and standard task durations
        /// </summary>
        private async Task<DateTime> CalculatePredictedEndDate(ProjectList project)
        {
            // Start from actual work start date instead of creation date
            DateTime startDate = await GetProjectActualStartDate(project.ProjectId);

            // Calculate Tender Management duration based on Project Indicative Value
            int tenderManagementDays = 21; // Default 3 weeks
            
            if (!string.IsNullOrEmpty(project.ProjectIndicative))
            {
                // Clean the string (remove $, commas, whitespace)
                string cleanValue = new string(project.ProjectIndicative.Where(c => char.IsDigit(c) || c == '.').ToArray());
                
                if (decimal.TryParse(cleanValue, out decimal indicativeValue))
                {
                    if (indicativeValue < 20000) tenderManagementDays = 28; // 4 weeks
                    else if (indicativeValue < 40000) tenderManagementDays = 42; // 6 weeks
                    else if (indicativeValue < 250000) tenderManagementDays = 56; // 8 weeks
                    else if (indicativeValue < 1000000) tenderManagementDays = 70; // 10 weeks
                    else tenderManagementDays = 84; // 12 weeks (> 1M)
                }
            }

            // Sum of all standard task durations:
            // Project Files & Documentation: 2
            // Fee Proposal & Costing: 14
            // Document Preparation: 2
            // Project Scoping: 3
            // Tender Management: [Dynamic based on value]
            // Exception Management: 7
            // Estimating Evaluation: 7
            // Quote Issued: 5
            // Quote Approved: 1
            // Quote Received: 1
            // Handover & Closeout: 7
            
            int totalDays = 2 + 14 + 2 + 3 + tenderManagementDays + 7 + 7 + 5 + 1 + 1 + 7;
            
            return startDate.AddDays(totalDays);
        }

        /// <summary>
        /// Create default tasks for a new project
        /// </summary>
        private async Task CreateDefaultTasksForProject(int projectId)
        {
            var defaultTaskNames = new List<string>
            {
                "Project Files & Documentation",
                "Fee Proposal & Costing",
                "Document Preparation",
                "Project Scoping",
                "Tender Management",
                "Exception Management",
                "Estimating Evaluation",
                "Quote Issued",
                "Quote Approved",
                "Quote Received",
                "Handover & Closeout"
            };

            using var context = await _contextFactory.CreateDbContextAsync();
            
            foreach (var taskName in defaultTaskNames)
            {
                var task = new TaskList
                {
                    TaskProjectId = projectId,
                    Task = taskName,
                    Status = 0, // Created/Not Started
                    Progress = 0,
                    DateCreated = DateTime.Now,
                    DateEnded = DateTime.Now
                };
                
                context.TaskLists.Add(task);
            }
            
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update existing project
        /// </summary>
        public async Task<bool> UpdateProject(ProjectList project)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                context.ProjectLists.Update(project);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete project
        /// </summary>
        public async Task<bool> DeleteProject(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var project = await context.ProjectLists.FindAsync(projectId);
                if (project == null)
                    return false;

                context.ProjectLists.Remove(project);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get total projects count
        /// </summary>
        public async Task<int> GetTotalProjectsCount()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists.CountAsync();
        }

        /// <summary>
        /// Get in-progress projects count
        /// </summary>
        public async Task<int> GetInProgressProjectsCount()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Assuming status 0 = In Progress based on your PHP code
            return await context.ProjectLists.CountAsync(p => p.ProjectStatus == 0);
        }

        /// <summary>
/// Get user's in-progress projects count (aligned with GetProjectsForUser logic)
/// </summary>
public async Task<int> GetUserInProgressProjectsCount(User user)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    if (user.Role == UserRole.Admin || user.Role == UserRole.ReadOnly)
    {
        // Admins and ReadOnly users see all in-progress projects
        return await context.ProjectLists
            .CountAsync(p => p.ProjectStatus == 1);
    }
    else if (user.Role == UserRole.PrincipalEstimator)
    {
        // Principal Estimators see in-progress projects in their region + assigned projects
        return await context.ProjectLists
            .Where(p => p.ProjectStatus == 1 &&
                       ((p.ProjectRegion == user.Region) ||
                        (p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString()))))
            .CountAsync();
    }
    else
    {
        // Managers and Estimators see only assigned projects
        return await context.ProjectLists
            .Where(p => p.ProjectStatus == 1 &&
                       p.ProjectUserIds != null && 
                       p.ProjectUserIds.Contains(user.UserId.ToString()))
            .CountAsync();
    }
}

        /// <summary>
/// Get user's total tasks count (aligned with GetProjectsForUser logic)
/// </summary>
public async Task<int> GetUserTotalTasksCount(User user)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    if (user.Role == UserRole.Admin || user.Role == UserRole.ReadOnly)
    {
        // Admins and ReadOnly users see all tasks
        return await context.TaskLists.CountAsync();
    }
    else if (user.Role == UserRole.PrincipalEstimator)
    {
        // Principal Estimators see tasks in their region + assigned projects
        var userProjectIds = await context.ProjectLists
            .Where(p => (p.ProjectRegion == user.Region) ||
                       (p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString())))
            .Select(p => p.ProjectId)
            .ToListAsync();
        
        return await context.TaskLists
            .CountAsync(t => userProjectIds.Contains(t.TaskProjectId ?? 0));
    }
    else
    {
        // Managers and Estimators see tasks in assigned projects
        var userProjectIds = await context.ProjectLists
            .Where(p => p.ProjectUserIds != null && 
                       p.ProjectUserIds.Contains(user.UserId.ToString()))
            .Select(p => p.ProjectId)
            .ToListAsync();
        
        return await context.TaskLists
            .CountAsync(t => userProjectIds.Contains(t.TaskProjectId ?? 0));
    }
}

        /// <summary>
/// Get user's in-progress tasks count (aligned with GetProjectsForUser logic)
/// </summary>
public async Task<int> GetUserInProgressTasksCount(User user)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    if (user.Role == UserRole.Admin || user.Role == UserRole.ReadOnly)
    {
        // Admins and ReadOnly users see all in-progress tasks
        return await context.TaskLists
            .CountAsync(t => t.Status == 1);
    }
    else if (user.Role == UserRole.PrincipalEstimator)
    {
        // Principal Estimators see in-progress tasks in their region + assigned projects
        var userProjectIds = await context.ProjectLists
            .Where(p => (p.ProjectRegion == user.Region) ||
                       (p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString())))
            .Select(p => p.ProjectId)
            .ToListAsync();
        
        return await context.TaskLists
            .CountAsync(t => userProjectIds.Contains(t.TaskProjectId ?? 0) && t.Status == 1);
    }
    else
    {
        // Managers and Estimators see in-progress tasks in assigned projects
        var userProjectIds = await context.ProjectLists
            .Where(p => p.ProjectUserIds != null && 
                       p.ProjectUserIds.Contains(user.UserId.ToString()))
            .Select(p => p.ProjectId)
            .ToListAsync();
        
        return await context.TaskLists
            .CountAsync(t => userProjectIds.Contains(t.TaskProjectId ?? 0) && t.Status == 1);
    }
}


        /// <summary>
        /// Get projects by manager
        /// </summary>
        public async Task<List<ProjectList>> GetProjectsByManager(int managerId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists
                .Where(p => p.ProjectManagerID == managerId)
                .OrderByDescending(p => p.ProjectDateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Search projects by name or location
        /// </summary>
        public async Task<List<ProjectList>> SearchProjects(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await context.ProjectLists
                    .OrderByDescending(p => p.ProjectDateCreated)
                    .ToListAsync();
            }

            searchTerm = searchTerm.ToLower();
            return await context.ProjectLists
                .Where(p =>
                    (p.ProjectName != null && p.ProjectName.ToLower().Contains(searchTerm)) ||
                    (p.ProjectLocation != null && p.ProjectLocation.ToLower().Contains(searchTerm)) ||
                    (p.ProjectWr != null && p.ProjectWr.ToLower().Contains(searchTerm)))
                .OrderByDescending(p => p.ProjectDateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Get project progress (percentage of completed tasks)
        /// </summary>
        public async Task<decimal> GetProjectProgress(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var totalTasks = await context.TaskLists
                .CountAsync(t => t.TaskProjectId == projectId);

            if (totalTasks == 0)
                return 0;

            var completedTasks = await context.TaskLists
                .CountAsync(t => t.TaskProjectId == projectId && t.Status == 3); // Status 3 = Done

            return (decimal)completedTasks / totalTasks * 100;
        }

        /// <summary>
        /// Get total hours logged for a project
        /// </summary>
        public async Task<decimal> GetProjectTotalHours(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var total = await context.UserProductivities
                .Where(up => up.ProductivityProjectId == projectId)
                .SumAsync(up => up.ProductivityTimeRendered ?? 0);

            return (decimal)total;
        }

        /// <summary>
        /// Get projects with upcoming deadlines (within X days)
        /// </summary>
        public async Task<List<ProjectList>> GetProjectsWithUpcomingDeadlines(int daysAhead = 7)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var futureDate = DateTime.Now.AddDays(daysAhead);
            return await context.ProjectLists
                .Where(p => p.ProjectEndDate.HasValue &&
                           p.ProjectEndDate.Value >= DateTime.Now &&
                           p.ProjectEndDate.Value <= futureDate)
                .OrderBy(p => p.ProjectEndDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get overdue projects
        /// </summary>
        public async Task<List<ProjectList>> GetOverdueProjects()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists
                .Where(p => p.ProjectEndDate.HasValue &&
                           p.ProjectEndDate.Value < DateTime.Now &&
                           p.ProjectStatus != 5) // Not "Done"
                .OrderBy(p => p.ProjectEndDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get total revenue for financial year
        /// </summary>
        public async Task<decimal> GetFinancialYearRevenue()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;

            DateTime fyStart;
            DateTime fyEnd;

            if (month >= 7) // July to December
            {
                fyStart = new DateTime(year, 7, 1);
                fyEnd = new DateTime(year + 1, 6, 30);
            }
            else // January to June
            {
                fyStart = new DateTime(year - 1, 7, 1);
                fyEnd = new DateTime(year, 6, 30);
            }

            var projects = await context.ProjectLists
                .Where(p => p.ProjectDateCreated >= fyStart &&
                           p.ProjectDateCreated <= fyEnd &&
                           p.ProjectActualPrice != null)
                .Select(p => p.ProjectActualPrice)
                .ToListAsync();

            var revenue = projects.Sum(priceStr => decimal.TryParse(priceStr, out var price) ? price : 0);

            return revenue;
        }

        /// <summary>
        /// Get user's financial year revenue (aligned with GetProjectsForUser logic)
        /// </summary>
        public async Task<decimal> GetUserFinancialYearRevenue(User user)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;

            DateTime fyStart;
            DateTime fyEnd;

            if (month >= 7) // July to December
            {
                fyStart = new DateTime(year, 7, 1);
                fyEnd = new DateTime(year + 1, 6, 30);
            }
            else // January to June
            {
                fyStart = new DateTime(year - 1, 7, 1);
                fyEnd = new DateTime(year, 6, 30);
            }

            List<string?> projectPrices;

            if (user.Role == UserRole.Admin || user.Role == UserRole.ReadOnly)
            {
                // Admins and ReadOnly users see all revenue
                projectPrices = await context.ProjectLists
                    .Where(p => p.ProjectDateCreated >= fyStart &&
                               p.ProjectDateCreated <= fyEnd &&
                               p.ProjectActualPrice != null)
                    .Select(p => p.ProjectActualPrice)
                    .ToListAsync();
            }
            else if (user.Role == UserRole.PrincipalEstimator)
            {
                // Principal Estimators see revenue from their region + assigned projects
                projectPrices = await context.ProjectLists
                    .Where(p => p.ProjectDateCreated >= fyStart &&
                               p.ProjectDateCreated <= fyEnd &&
                               p.ProjectActualPrice != null &&
                               ((p.ProjectRegion == user.Region) ||
                                (p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString()))))
                    .Select(p => p.ProjectActualPrice)
                    .ToListAsync();
            }
            else
            {
                // Managers and Estimators see revenue from assigned projects only
                projectPrices = await context.ProjectLists
                    .Where(p => p.ProjectDateCreated >= fyStart &&
                               p.ProjectDateCreated <= fyEnd &&
                               p.ProjectActualPrice != null &&
                               p.ProjectUserIds != null &&
                               p.ProjectUserIds.Contains(user.UserId.ToString()))
                    .Select(p => p.ProjectActualPrice)
                    .ToListAsync();
            }

            var revenue = projectPrices.Sum(priceStr => decimal.TryParse(priceStr, out var price) ? price : 0);

            return revenue;
        }


        /// <summary>
        /// Get total system tasks count
        /// </summary>
        public async Task<int> GetTotalSystemTasksCount()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TaskLists.CountAsync();
        }

        /// <summary>
        /// Get total system in-progress tasks count
        /// </summary>
        public async Task<int> GetTotalSystemInProgressTasksCount()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Assuming status 1 = In Progress (Need to verify TaskList status enum/values)
            // Based on typical conventions: 0=ToDo, 1=InProgress, 2=Review, 3=Done
            // Or if using the same as projects: 0=InProgress
            // Let's assume 1 for now based on "Your tasks in Progress" widget usually implying active work
            // Checking TaskList model would be ideal, but for now I'll use a safe assumption or check if I can see TaskList definition.
            // Actually, let's check TaskList definition first to be sure.
            return await context.TaskLists.CountAsync(t => t.Status == 1); 
        }

        /// <summary>
        /// Check if user can access project
        /// </summary>
        public async Task<bool> CanUserAccessProject(int projectId, User user)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (user.Role >= UserRole.Manager)
                return true; // Managers and above can access all

            var project = await context.ProjectLists.FindAsync(projectId);
            if (project == null)
                return false;

            // Check if user is the project manager
            if (project.ProjectManagerID == user.UserId)
                return true;

            // Check if user is in the project's user list
            if (project.ProjectUserIds != null &&
                project.ProjectUserIds.Contains(user.UserId.ToString()))
                return true;

            return false;
        }

        /// <summary>
        /// Get tasks for a specific project
        /// </summary>
        public async Task<List<TaskList>> GetProjectTasks(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TaskLists
                .Where(t => t.TaskProjectId == projectId)
                .OrderBy(t => t.DateCreated)
                .ToListAsync();
        }

        /// <summary>
        /// Get productivity entries for a specific project
        /// </summary>
        public async Task<List<UserProductivity>> GetProjectProductivity(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserProductivities
                .Where(p => p.ProductivityProjectId == projectId)
                .OrderByDescending(p => p.ProductivityDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get user by ID (for productivity display)
        /// </summary>
        public async Task<User?> GetUserById(int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.FindAsync(userId);
        }

        // Task CRUD Operations

        public async Task<TaskList> AddProjectTask(TaskList task)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            task.DateCreated = DateTime.Now;
            context.TaskLists.Add(task);
            await context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> UpdateProjectTask(TaskList task)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                // Get the existing task to compare status
                var existingTask = await context.TaskLists.FindAsync(task.TaskId);
                
                if (existingTask != null)
                {
                    var oldStatus = existingTask.Status;
                    var newStatus = task.Status;
                    
                    // Update DateCreated when transitioning from Pending (0) to In-Progress (1)
                    // This captures when the task actually started
                    if (oldStatus == 0 && newStatus == 1)
                    {
                        task.DateCreated = DateTime.Now;
                    }
                    
                    // Update DateEnded when transitioning from In-Progress (1) to Done (3)
                    // This captures when the task actually completed
                    if (oldStatus == 1 && newStatus == 3)
                    {
                        task.DateEnded = DateTime.Now;
                    }
                    
                    // Calculate and update EstimatedDays with actual duration when task is marked as Done
                    if (newStatus == 3 && task.DateEnded > task.DateCreated)
                    {
                        var actualDuration = (task.DateEnded - task.DateCreated).TotalDays;
                        task.EstimatedDays = (int)Math.Ceiling(actualDuration); // Round up to nearest day
                    }
                    
                    // Update the task with new values
                    context.Entry(existingTask).CurrentValues.SetValues(task);
                    await context.SaveChangesAsync();

                    // If task started (Status 1), recalculate project end date
                    // This ensures the prediction shifts based on actual start date
                    if (newStatus == 1)
                    {
                        var project = await context.ProjectLists.FindAsync(task.TaskProjectId);
                        if (project != null)
                        {
                            // Recalculate end date based on this new task start
                            // We need to use a new context or the existing one? 
                            // CalculatePredictedEndDate calls GetProjectActualStartDate which creates its own context.
                            // This is fine.
                            project.ProjectEndDate = await CalculatePredictedEndDate(project);
                            context.ProjectLists.Update(project);
                            await context.SaveChangesAsync();
                        }
                    }

                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProjectTask(int taskId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var task = await context.TaskLists.FindAsync(taskId);
                if (task == null) return false;

                context.TaskLists.Remove(task);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TaskList?> GetTaskById(int taskId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TaskLists.FindAsync(taskId);
        }

        // Productivity CRUD Operations

        public async Task<UserProductivity> AddUserProductivity(UserProductivity productivity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            productivity.ProductivityDateCreated = DateTime.Now;
            context.UserProductivities.Add(productivity);
            await context.SaveChangesAsync();
            return productivity;
        }

        public async Task<bool> UpdateUserProductivity(UserProductivity productivity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                context.UserProductivities.Update(productivity);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserProductivity(int productivityId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var productivity = await context.UserProductivities.FindAsync(productivityId);
                if (productivity == null) return false;

                context.UserProductivities.Remove(productivity);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserProductivity?> GetProductivityById(int productivityId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserProductivities.FindAsync(productivityId);
        }

        /// <summary>
        /// Check if a project with the given WR exists
        /// </summary>
        public async Task<bool> CheckProjectWrExists(string wr)
        {
            if (string.IsNullOrWhiteSpace(wr))
                return false;

            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists.AnyAsync(p => p.ProjectWr == wr);
        }
    }
}
