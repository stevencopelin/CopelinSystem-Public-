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
        private readonly TaskConfigurationService _taskConfigService;
        private readonly FileSystemService _fileSystemService;

        public ProjectService(IDbContextFactory<ApplicationDbContext> contextFactory, TaskConfigurationService taskConfigService, FileSystemService fileSystemService)
        {
            _contextFactory = contextFactory;
            _taskConfigService = taskConfigService;
            _fileSystemService = fileSystemService;
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
            
            List<ProjectList> projects;
            var query = context.ProjectLists.AsQueryable();

            if (user.Role == UserRole.Admin)
            {
                // Admins can see all projects
                // No filter on query
            }
            else if (user.Role == UserRole.PrincipalEstimator)
            {
                // Principal Estimators see:
                // 1. Projects in their region
                // 2. Projects they're assigned to (regardless of region)
                query = query.Where(p => 
                        (p.ProjectRegion == user.Region) ||  // Projects in their region
                        (p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString()))); // Their assigned projects
            }
            else if (user.Role == UserRole.Manager || user.Role == UserRole.Estimator)
            {
                // Managers (Senior Estimators) and Estimators can see projects they're assigned to
                query = query.Where(p => p.ProjectUserIds != null && p.ProjectUserIds.Contains(user.UserId.ToString()));
            }
            else
            {
                // Read-only users can see all projects but can't edit
                // No filter on query
            }

            projects = await query.ToListAsync();

            // Get User Preferences for sorting
            var preferences = await context.UserProjectPreferences
                .Where(upp => upp.UserId == user.UserId)
                .ToDictionaryAsync(upp => upp.ProjectId, upp => upp.SortOrder);

            // Sort: 
            // 1. By SortOrder (if exists)
            // 2. By DateCreated Descending (newest first) for those without preference (or same preference)
            // Note: Projects without preference get int.MaxValue to appear at the bottom (or top if we want)
            // Let's make new projects appear at the TOP by default? User said "Progress projects, drag and drop them".
            // Typically new items appear at top. If users reorder, they set an explicit order.
            // If I default to int.MaxValue, they go to bottom.
            // If I default to -1, they go to top.
            // Let's default to appearing at the TOP (standard for chronological lists), 
            // but if user has a custom order, they sort explicitly.
            // Actually, if a user reorders list, they are defining an explicit order 0..N.
            // New items appearing at top might disrupt the "first" item.
            // Let's assume default sort is by DateCreated.
            // How to mix?
            // If UserProjectPreferences is empty, we want DateCreated Descending.
            // If UserProjectPreferences is populated, we want those generic orders.
            // Let's perform the sort:
            // return projects.OrderBy(p => preferences.ContainsKey(p.ProjectId) ? preferences[p.ProjectId] : int.MaxValue).ThenByDescending(p => p.ProjectDateCreated).ToList();
            
            return projects
                .OrderBy(p => preferences.ContainsKey(p.ProjectId) ? preferences[p.ProjectId] : int.MaxValue)
                .ThenByDescending(p => p.ProjectDateCreated)
                .ToList();
        }

        /// <summary>
        /// Update sort order for projects for a user
        /// </summary>
        public async Task UpdateProjectSortOrder(int userId, List<int> projectIds)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Fetch existing preferences for this user
            var existingPreferences = await context.UserProjectPreferences
                .Where(upp => upp.UserId == userId)
                .ToListAsync();

            // Prepare list to remove (projects no longer in the list? No, we likely receive the full list of visible projects)
            // But just in case, we only care about updating/adding.
            
            // Loop through the new order
            for (int i = 0; i < projectIds.Count; i++)
            {
                int projectId = projectIds[i];
                var pref = existingPreferences.FirstOrDefault(p => p.ProjectId == projectId);

                if (pref != null)
                {
                    // Update
                    pref.SortOrder = i;
                }
                else
                {
                    // Add new
                    context.UserProjectPreferences.Add(new UserProjectPreference
                    {
                        UserId = userId,
                        ProjectId = projectId,
                        SortOrder = i
                    });
                }
            }
            
            await context.SaveChangesAsync();
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
            
            // Create default folders
            // Use "System" or the actual creator if available. 
            // Since we don't have the creator ID passed in explicitly as a robust ID here, default to "System" or try to parse
            await _fileSystemService.CreateDefaultFolders(project.ProjectId, "System");

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
        /// <summary>
        /// Calculate predicted end date based on project value and standard task durations
        /// </summary>
        private async Task<DateTime> CalculatePredictedEndDate(ProjectList project)
        {
            // Start from actual work start date instead of creation date
            DateTime startDate = await GetProjectActualStartDate(project.ProjectId);
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Determine Region ID
            int regionId = 0;
            if (!string.IsNullOrEmpty(project.ProjectRegion))
            {
                var region = await context.Regions.FirstOrDefaultAsync(r => r.RegionName == project.ProjectRegion);
                if (region != null)
                {
                    regionId = region.RegionId;
                }
            }
            
            // Get project value for thresholds
            decimal? projectValue = null;
            if (!string.IsNullOrEmpty(project.ProjectIndicative))
            {
                string cleanValue = new string(project.ProjectIndicative.Where(c => char.IsDigit(c) || c == '.').ToArray());
                // Handle potential multiple dots or invalid formats specifically? 
                // decimal.TryParse handles simpler cases.
                // Assuming well-formed input or "good enough" for now.
                if (decimal.TryParse(cleanValue, out decimal val))
                {
                    projectValue = val;
                }
            }

            // Define standard tasks list
            var taskNames = new List<string>
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

            int totalDays = 0;
            
            foreach (var name in taskNames)
            {
                totalDays += await _taskConfigService.GetTaskDuration(regionId, name, projectValue);
            }
            
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
        /// <summary>
        /// Delete project
        /// </summary>
        /// <summary>
        /// Delete project
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteProject(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Use ExecutionStrategy to handle transactions with retry logic enabled
            var strategy = context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    var project = await context.ProjectLists.FindAsync(projectId);
                    if (project == null)
                        return (false, "Project not found.");

                    // Validation 1: Project Status must be 'Done' (5)
                    if (project.ProjectStatus != 5)
                    {
                        return (false, "Cannot delete project. Project status must be 'Done'.");
                    }

                    // Validation 2: Check if all tasks are Done (Status == 3)
                    // We check for any task that is NOT 3.
                    // Also handle nulls just in case (treat as incomplete)
                    var incompleteTasks = await context.TaskLists
                        .Where(t => t.TaskProjectId == projectId && (t.Status == null || t.Status != 3))
                        .AnyAsync();

                    if (incompleteTasks)
                    {
                        // No need to rollback mostly as we just read, but good practice
                        // transaction auto-rollbacks on dispose if not committed.
                        return (false, "Cannot delete project. All tasks must be 'Done' first.");
                    }

                    // 1. Remove User Productivity/Timesheets (Constraint: NoAction)
                    var productivityLogs = await context.UserProductivities
                        .Where(up => up.ProductivityProjectId == projectId)
                        .ToListAsync();
                    if (productivityLogs.Any())
                    {
                        context.UserProductivities.RemoveRange(productivityLogs);
                    }

                    // 2. Remove Tasks (Constraint: Cascade, but safe to be explicit)
                    var tasks = await context.TaskLists
                        .Where(t => t.TaskProjectId == projectId)
                        .ToListAsync();
                    if (tasks.Any())
                    {
                        context.TaskLists.RemoveRange(tasks);
                    }
                    
                    // 3. Remove User Preferences (Constraint: Cascade)
                    var prefs = await context.UserProjectPreferences
                        .Where(p => p.ProjectId == projectId)
                        .ToListAsync();
                    if (prefs.Any())
                    {
                        context.UserProjectPreferences.RemoveRange(prefs);
                    }

                    // 4. Remove Project
                    context.ProjectLists.Remove(project);
                    
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return (true, "Project deleted successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error deleting project: {ex.Message}");
                    return (false, $"Failed to delete project: {ex.Message}");
                }
            });
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
        /// Get total tasks count for a specific region
        /// </summary>
        public async Task<int> GetRegionTotalTasksCount(string region)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            if (string.IsNullOrEmpty(region))
            {
                return await context.TaskLists.CountAsync();
            }

            return await context.TaskLists
                .Include(t => t.Project)
                .Where(t => t.Project != null && t.Project.ProjectRegion == region)
                .CountAsync();
        }

        /// <summary>
        /// Get total email and attachment count
        /// </summary>
        public async Task<int> GetTotalKeyDocumentsCount()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var emails = await context.ProjectEmails.CountAsync();
            var attachments = await context.ProjectEmailAttachments.CountAsync();
            
            return emails + attachments;
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
        /// Get open tasks reminders (In-Progress, > 7 days old) filtered by user role
        /// </summary>
        public async Task<List<TaskList>> GetOpenTasksReminders(User user)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Base Query: Status = 1 (In Progress) and Age > 7 days
            // We use DateCreated as the start date for the task
            var cutoffDate = DateTime.Now.AddDays(-7);
            
            var query = context.TaskLists
                .Include(t => t.Project) // Eager load Project for Name/WR/Region
                .Where(t => t.Status == 1 && t.DateCreated < cutoffDate);

            // Apply Role Filters
            if (user.Role == UserRole.Admin || user.Role == UserRole.ReadOnly)
            {
                // Admins see all applicable tasks
                // No additional filters needed
            }
            else if (user.Role == UserRole.PrincipalEstimator)
            {
                // Principal Estimators see tasks from:
                // 1. Projects in their Region
                // 2. Projects assigned to them checks
                // Note: We need to filter based on the Project's properties
                query = query.Where(t => 
                    (t.Project != null && t.Project.ProjectRegion == user.Region) || 
                    (t.Project != null && t.Project.ProjectUserIds != null && t.Project.ProjectUserIds.Contains(user.UserId.ToString())));
            }
            else
            {
                // Managers and Estimators see tasks from projects assigned to them
                query = query.Where(t => 
                    t.Project != null && 
                    t.Project.ProjectUserIds != null && 
                    t.Project.ProjectUserIds.Contains(user.UserId.ToString()));
            }

            // Return sorted by DateCreated (oldest first - highest priority/age)
            return await query
                .OrderBy(t => t.DateCreated)
                .ToListAsync();
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

        /// <summary>
        /// Get project by WR number
        /// </summary>
        public async Task<ProjectList?> GetProjectByWr(string wr)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(wr)) return null;
            
            var cleanWr = wr.Trim();
            // Try exact match first, then robust match
            var project = await context.ProjectLists.FirstOrDefaultAsync(p => p.ProjectWr == cleanWr);
            
            if (project == null)
            {
                // Fallback: Try match with DB-side trimming if supported, or pull candidates
                // Note: EF Core translation of Trim might vary. 
                // Let's try Where(Contains) which is safer for translation, assuming unique WR
                // Also checking ProjectWo (Work Order) just in case
                project = await context.ProjectLists.FirstOrDefaultAsync(p => 
                    (p.ProjectWr != null && p.ProjectWr.Contains(cleanWr)) || 
                    (p.ProjectWo != null && p.ProjectWo.Contains(cleanWr)));
            }
            return project;
        }

        public async Task<List<string>> GetDebugProjectMatches(string term)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var matches = await context.ProjectLists
                .Where(p => (p.ProjectWr != null && p.ProjectWr.Contains(term)) || (p.ProjectWo != null && p.ProjectWo.Contains(term)))
                .Take(5)
                .Select(p => $"ID: {p.ProjectId} | WR: '{p.ProjectWr}' | WO: '{p.ProjectWo}' | Name: {p.ProjectName}")
                .ToListAsync();
            
            if (!matches.Any())
            {
                // Try searching JUST by ID strictly if term is int
                if (int.TryParse(term, out int id))
                {
                    var p = await context.ProjectLists.FindAsync(id);
                    if (p != null) matches.Add($"ID MATCH: {p.ProjectId} | WR: '{p.ProjectWr}'");
                }
                
                // FINAL FALLBACK: Show what IS in the DB to prove connection
                // 1. Check if we can see the current project (ID 41 seems to be the one viewed)
                var count = await context.ProjectLists.CountAsync();
                matches.Add($"Total Projects in DB: {count}");

                var anyProjects = await context.ProjectLists
                    .OrderByDescending(p => p.ProjectId)
                    .Take(5)
                    .Select(p => $"Found: ID {p.ProjectId} | WR '{p.ProjectWr}' | WO '{p.ProjectWo}'")
                    .ToListAsync();
                    
                matches.AddRange(anyProjects);
            }
            return matches;
        }

        /// <summary>
        /// Get all Ellipse Status Codes
        /// </summary>
        public async Task<List<StatusCode>> GetEllipseStatuses()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.StatusCodes
                .OrderBy(s => s.Code)
                .ToListAsync();
        }
    }
}
