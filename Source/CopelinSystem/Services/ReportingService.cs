using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class ReportingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ReportingService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // --- 1. Project Performance ---

        public async Task<Dictionary<string, int>> GetProjectStatusDistribution(string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            var data = await query
                .GroupBy(p => p.ProjectStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            foreach (var item in data)
            {
                string statusLabel = item.Status switch
                {
                    0 => "Created",
                    1 => "In Progress",
                    2 => "On Hold",
                    3 => "Review",
                    4 => "Cancelled",
                    5 => "Done",
                    _ => "Unknown"
                };
                result[statusLabel] = item.Count;
            }
            return result;
        }

        public async Task<(int onTime, int late, int total)> GetProjectOnTimeStats(string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // Get ALL 'Done' projects (Status 5) regardless of dates
            var allCompleted = await query
                .Where(p => p.ProjectStatus == 5)
                .Select(p => new { p.ProjectComplete, p.ProjectClientCompletion }) // Select only needed fields
                .ToListAsync();

            int total = allCompleted.Count;
            
            // Calculate OnTime/Late only for those with valid dates
            // If date is missing, we can't determine on-time status, so we exclude from the rate calculation denominator implicitly
            // or just count them as 'unknown'.
            // For this specific return signature (onTime, late, total), we'll do:
            
            int onTime = allCompleted.Count(p => p.ProjectComplete.HasValue && p.ProjectClientCompletion.HasValue && p.ProjectComplete <= p.ProjectClientCompletion);
            
            // Late is specifically those KNOWN to be late
            int late = allCompleted.Count(p => p.ProjectComplete.HasValue && p.ProjectClientCompletion.HasValue && p.ProjectComplete > p.ProjectClientCompletion);
            
            // Note: total might be > (onTime + late) if some projects don't have dates.
            // This ensures "Total Completed" KPI is accurate even if completion metadata is missing.

            return (onTime, late, total);
        }

        public async Task<List<ProjectList>> GetProjectsApproachingCompletion(int days, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Now.Date;
            var targetDate = today.AddDays(days);
            
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // Approaching: Status != Done (5) AND Status != Cancelled (4) 
            // AND ClientCompletion >= Today AND ClientCompletion <= TargetDate
            return await query
                .Where(p => p.ProjectStatus != 5 && p.ProjectStatus != 4 && 
                           p.ProjectClientCompletion.HasValue && 
                           p.ProjectClientCompletion >= today &&
                           p.ProjectClientCompletion <= targetDate)
                .OrderBy(p => p.ProjectClientCompletion)
                .ToListAsync();
        }

        public async Task<List<ProjectList>> GetProjectsApproachingTenderClose(int weeks, DateTime? filterStart, DateTime? filterEnd, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Now.Date;
            
            // Default logic
            var startDate = today;
            var targetDate = today.AddDays(weeks * 7);

            // Override logic if filter is present
            if (filterEnd.HasValue)
            {
                startDate = filterStart ?? today;
                targetDate = filterEnd.Value;
            }

            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // Approaching Tender: Status != Done (5) AND Status != Cancelled (4) 
            // AND ProjectTendered >= StartDate AND ProjectTendered <= TargetDate
            // Note: If filtering historically, we might want to include past tenders, so >= StartDate is important.
            
            return await query
                .Where(p => p.ProjectStatus != 5 && p.ProjectStatus != 4 &&
                            p.ProjectTendered.HasValue &&
                            p.ProjectTendered >= startDate &&
                            p.ProjectTendered <= targetDate)
                .OrderBy(p => p.ProjectTendered)
                .ToListAsync();
        }

        public async Task<List<ProjectList>> GetProjectsWROverdue(DateTime? filterStart, DateTime? filterEnd, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var limitDate = DateTime.Now.Date.AddDays(-21); // 3 weeks ago
            
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // WR Overdue: Status == Created (0)
            // Logic: 
            // - If filter applied: DateCreated within range
            // - Default: DateCreated < limitDate (Strictly overdue > 3 weeks)
            
            if (filterEnd.HasValue)
            {
                 var start = filterStart ?? DateTime.MinValue;
                 var end = filterEnd.Value;
                 return await query
                    .Where(p => p.ProjectStatus == 0 && p.ProjectDateCreated >= start && p.ProjectDateCreated <= end)
                    .OrderBy(p => p.ProjectDateCreated)
                    .ToListAsync();
            }
            else
            {
                return await query
                    .Where(p => p.ProjectStatus == 0 && p.ProjectDateCreated < limitDate)
                    .OrderBy(p => p.ProjectDateCreated)
                    .ToListAsync();
            }
        }

        public async Task<List<ProjectList>> GetOverdueProjects(string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Now.Date;
            
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // Overdue: Status != Done (5) AND Status != Cancelled (4) AND ClientCompletion < Today
            return await query
                .Where(p => p.ProjectStatus != 5 && p.ProjectStatus != 4 && 
                           p.ProjectClientCompletion.HasValue && 
                           p.ProjectClientCompletion < today)
                .OrderBy(p => p.ProjectClientCompletion)
                .ToListAsync();
        }

        public async Task<List<(string Name, string Location, DateTime? Start, DateTime? SchedEnd, DateTime? ActualEnd, int SchedDays, int ActualDays, int Variance)>> GetProjectDurationStats(DateTime? startDate, DateTime? endDate, int limit = 20, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // Get recently completed projects
            // Relaxed filter: Just check for Status = 5 (Done)
            // We will handle missing dates gracefully in the projection/loop
            var baseQuery = query.Where(p => p.ProjectStatus == 5);
            
            // Apply Date Filtering
            if (startDate.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.ProjectComplete >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Include the entire end day
                var realEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(p => p.ProjectComplete <= realEndDate);
            }

            var projects = await baseQuery
                .OrderByDescending(p => p.ProjectComplete) // Note: this might put nulls at top or bottom depending on DB
                .Take(limit)
                .ToListAsync();

            var result = new List<(string, string, DateTime?, DateTime?, DateTime?, int, int, int)>();

            foreach (var p in projects)
            {
                // Determine Start Date (fallback to Created Date if Start Date missing)
                DateTime projectStart = p.ProjectStartDate ?? p.ProjectDateCreated;
                
                // Determine Actual End Date (fallback to Today or handle as incomplete for calc)
                DateTime actualEnd = p.ProjectComplete ?? DateTime.Now.Date;

                // Actual Days
                int actual = (int)(actualEnd - projectStart).TotalDays;
                if (actual < 1) actual = 1;

                // Scheduled Days
                int predicted = 0;
                if (p.ProjectClientCompletion.HasValue)
                {
                    predicted = (int)(p.ProjectClientCompletion.Value - projectStart).TotalDays;
                    if (predicted < 1) predicted = 1;
                }

                int variance = actual - predicted;

                result.Add((p.ProjectName ?? "Unknown", p.ProjectLocation ?? "", p.ProjectStartDate, p.ProjectClientCompletion, p.ProjectComplete, predicted, actual, variance));
            }

            return result;
        }

        // --- 2. Financials ---

        public async Task<Dictionary<string, decimal>> GetRevenueByRegion(int year, string? region = null)
        {
            // Simple Financial Year Logic (July-June)
            DateTime start = new DateTime(year, 7, 1);
            DateTime end = new DateTime(year + 1, 6, 30);

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            var projects = await query
                .Where(p => p.ProjectDateCreated >= start && p.ProjectDateCreated <= end && p.ProjectActualPrice != null)
                .ToListAsync();

            // Perform client-side grouping because decimal parsing of sting 'ProjectActualPrice' is tricky in SQL
            var result = new Dictionary<string, decimal>();
            
            foreach (var p in projects)
            {
                if (string.IsNullOrEmpty(p.ProjectRegion)) continue;
                
                // Clean and parse price
                // Fix CS8604: ProjectActualPrice is filtered to be non-null in query, but compiler doesn't know.
                string priceInput = p.ProjectActualPrice ?? "";
                string cleanPrice = new string(priceInput.Where(c => char.IsDigit(c) || c == '.').ToArray());
                if (decimal.TryParse(cleanPrice, out decimal value))
                {
                    if (result.ContainsKey(p.ProjectRegion))
                        result[p.ProjectRegion] += value;
                    else
                        result[p.ProjectRegion] = value;
                }
            }
            
            return result;
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyRevenueTrend(int year, string? region = null)
        {
            DateTime start = new DateTime(year, 7, 1);
            DateTime end = new DateTime(year + 1, 6, 30);

            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            var projects = await query
                .Where(p => p.ProjectDateCreated >= start && p.ProjectDateCreated <= end && p.ProjectActualPrice != null)
                .ToListAsync();

            var result = new Dictionary<string, decimal>();
            
            // Initialize months
            for (int i = 0; i < 12; i++)
            {
                var month = start.AddMonths(i);
                result[month.ToString("MMM yyyy")] = 0;
            }

            foreach (var p in projects)
            {
                if (p.ProjectDateCreated == default) continue;

                string monthKey = p.ProjectDateCreated.ToString("MMM yyyy");
                if (result.ContainsKey(monthKey))
                {
                     // Fix CS8604: ProjectActualPrice is filtered to be non-null in query, but compiler doesn't know.
                string priceInput = p.ProjectActualPrice ?? "";
                string cleanPrice = new string(priceInput.Where(c => char.IsDigit(c) || c == '.').ToArray());
                     if (decimal.TryParse(cleanPrice, out decimal value))
                     {
                         result[monthKey] += value;
                     }
                }
            }
            return result;
        }

        public async Task<List<(string ClientName, decimal TotalRevenue, int ProjectCount)>> GetTopClientsByRevenue(int year, int limit = 10, string? region = null)
        {
            DateTime start = new DateTime(year, 7, 1);
            DateTime end = new DateTime(year + 1, 6, 30);

            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.ProjectLists.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            var projects = await query
                .Where(p => p.ProjectDateCreated >= start && p.ProjectDateCreated <= end && p.ProjectActualPrice != null)
                .ToListAsync();
            
            var clientStats = new Dictionary<string, (decimal Revenue, int Count)>();

            foreach (var p in projects)
            {
                if (string.IsNullOrEmpty(p.ProjectClient)) continue;

                string client = p.ProjectClient;
                // Fix CS8604: ProjectActualPrice is filtered to be non-null in query, but compiler doesn't know.
                string priceInput = p.ProjectActualPrice ?? "";
                string cleanPrice = new string(priceInput.Where(c => char.IsDigit(c) || c == '.').ToArray());
                
                decimal value = 0;
                decimal.TryParse(cleanPrice, out value);

                if (!clientStats.ContainsKey(client))
                    clientStats[client] = (0, 0);

                var current = clientStats[client];
                clientStats[client] = (current.Revenue + value, current.Count + 1);
            }

            return clientStats
                .Select(x => (x.Key, x.Value.Revenue, x.Value.Count))
                .OrderByDescending(x => x.Revenue)
                .Take(limit)
                .ToList();
        }

        // --- 3. Productivity ---

        public async Task<List<(string UserName, float TotalHours)>> GetTopProductiveUsers(int limit = 5, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // We need to join with Users table manually or rely on EF navigation if setup
            // UserProductivity has ProductivityUserId (int)
            
            var query = context.UserProductivities.AsQueryable();

             if (!string.IsNullOrEmpty(region))
            {
                // Join to Project to filter by region
                // Assuming UserProductivity has ProjectId. If not, we might need a join
                // The provided model snippet for UserProductivity wasn't full but lines 291 in GetProjectHours uses ProductivityProjectId
                
                // Let's optimize by fetching project IDs in that region first
                var projectIds = await context.ProjectLists
                    .Where(p => p.ProjectRegion == region)
                    .Select(p => p.ProjectId)
                    .ToListAsync();

                query = query.Where(up => up.ProductivityProjectId.HasValue && projectIds.Contains(up.ProductivityProjectId.Value));
            }

            var grouped = await query
                .GroupBy(up => up.ProductivityUserId)
                .Select(g => new { UserId = g.Key, TotalHours = g.Sum(up => up.ProductivityTimeRendered) })
                .OrderByDescending(x => x.TotalHours)
                .Take(limit)
                .ToListAsync();

            var result = new List<(string, float)>();
            
            // Enhance with User Names
            var userIds = grouped.Select(q => q.UserId).ToList();
            var users = await context.Users.Where(u => userIds.Contains(u.UserId)).ToDictionaryAsync(u => u.UserId, u => u.DisplayName);

            foreach (var item in grouped)
            {
                if (item.UserId.HasValue && users.ContainsKey(item.UserId.Value))
                {
                    result.Add((users[item.UserId.Value], item.TotalHours ?? 0));
                }
            }
            
            return result;
        }

        public async Task<List<(string ProjectName, string ProjectLocation, float TotalHours)>> GetProjectHours(int limit = 10, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.UserProductivities.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                 var regionProjectIds = await context.ProjectLists
                    .Where(p => p.ProjectRegion == region)
                    .Select(p => p.ProjectId)
                    .ToListAsync();

                query = query.Where(up => up.ProductivityProjectId.HasValue && regionProjectIds.Contains(up.ProductivityProjectId.Value));
            }
            
            var grouped = await query
                .GroupBy(up => up.ProductivityProjectId)
                .Select(g => new { ProjectId = g.Key, TotalHours = g.Sum(up => up.ProductivityTimeRendered) })
                .OrderByDescending(x => x.TotalHours)
                .Take(limit)
                .ToListAsync();

            var result = new List<(string, string, float)>();
            
            var projectIds = grouped.Select(q => q.ProjectId).ToList();
            var projects = await context.ProjectLists
                .Where(p => projectIds.Contains(p.ProjectId))
                .Select(p => new { p.ProjectId, p.ProjectName, p.ProjectLocation })
                .ToListAsync();
            
            var projectLookup = projects.ToDictionary(p => p.ProjectId, p => (p.ProjectName, p.ProjectLocation));

            foreach (var item in grouped)
            {
                if (item.ProjectId.HasValue && projectLookup.ContainsKey(item.ProjectId.Value))
                {
                    var p = projectLookup[item.ProjectId.Value];
                    result.Add((p.ProjectName ?? "Unknown", p.ProjectLocation ?? "", item.TotalHours ?? 0));
                }
            }
            return result;
        }

        public async Task<List<(string UserName, int TasksCreated, int TasksDone)>> GetUserTaskStats()
        {
             using var context = await _contextFactory.CreateDbContextAsync();
             // This is tricky as Tasks don't always track separate "CreatedBy" vs "AssignedTo" cleanly in the model in all systems.
             // Assuming ProjectLists.ProjectUserIds or TaskLists fields?
             // Looking at TaskList definition (via inference or context if possible) - let's assume simple counts per project user for now?
             // Or better: Use UserProductivity count as proxy for activity?
             
             // Let's stick to Productive Hours leaderboard as primary, 
             // and maybe just distinct days active?
             
             // Simplification: We'll return just the top users by hours for now as implemented in GetTopProductiveUsers
             // And maybe add a "Last Active" date?
             
             return new List<(string, int, int)>(); // Placeholder if we need strict task counts
        }

        // --- 4. Operational ---

        public async Task<int> GetTotalActiveClients(string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.ProjectLists
                .Where(p => p.ProjectStatus == 1); // In Progress

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            return await query
                .Select(p => p.ProjectClient)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<(string ClientName, int ActiveProjectCount)>> GetActiveClientsWithProjectCounts(int limit = 10, string? region = null)
        {
             using var context = await _contextFactory.CreateDbContextAsync();
             
             var query = context.ProjectLists
                 .Where(p => p.ProjectStatus == 1 && !string.IsNullOrEmpty(p.ProjectClient)); // In Progress

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

             var grouped = await query
                 .GroupBy(p => p.ProjectClient)
                 .Select(g => new { Client = g.Key, Count = g.Count() })
                 .OrderByDescending(x => x.Count)
                 .Take(limit)
                 .ToListAsync();

             return grouped.Select(x => (x.Client ?? "Unknown", x.Count)).ToList();
        }

        public async Task<List<(string UserName, int ActiveProjectCount)>> GetUserWorkload(int limit = 10, string? region = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Get all active projects with assigned users
            var query = context.ProjectLists
                .Where(p => p.ProjectStatus == 1 && !string.IsNullOrEmpty(p.ProjectUserIds));

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            var activeProjects = await query 
                .Select(p => p.ProjectUserIds)
                .ToListAsync();

            // Client-side processing to split IDs and count
            var userCounts = new Dictionary<int, int>();
            
            foreach (var ids in activeProjects)
            {
                if (string.IsNullOrEmpty(ids)) continue;
                
                var splitIds = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var idStr in splitIds)
                {
                    if (int.TryParse(idStr, out int userId))
                    {
                         if (userCounts.ContainsKey(userId))
                             userCounts[userId]++;
                         else
                             userCounts[userId] = 1;
                    }
                }
            }

            // Get Top Users IDs
            var topUserIds = userCounts.OrderByDescending(x => x.Value).Take(limit).Select(x => x.Key).ToList();
            
            // Fetch User Names
            var users = await context.Users
                .Where(u => topUserIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.DisplayName);

            var result = new List<(string, int)>();
            foreach (var kvp in userCounts.OrderByDescending(x => x.Value).Take(limit))
            {
                if (users.ContainsKey(kvp.Key))
                {
                    result.Add((users[kvp.Key], kvp.Value));
                }
            }
            
            return result;
        }
    }
}
