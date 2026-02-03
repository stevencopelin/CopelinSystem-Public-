using System.Text.Json;
using CopelinSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CopelinSystem.Services
{
    public class TaskConfigurationService
    {
        private readonly ApplicationDbContext _context;

        public TaskConfigurationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskConfiguration>> GetConfigsByRegion(int regionId)
        {
            return await _context.TaskConfigurations
                .Where(t => t.RegionId == regionId)
                .ToListAsync();
        }

        public async Task<TaskConfiguration?> GetConfigById(int id)
        {
            return await _context.TaskConfigurations.FindAsync(id);
        }

        public async Task CreateConfig(TaskConfiguration config)
        {
            _context.TaskConfigurations.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConfig(TaskConfiguration config)
        {
            // Check if entity is already being tracked
            var existingEntity = _context.TaskConfigurations.Local.FirstOrDefault(e => e.Id == config.Id);
            if (existingEntity != null)
            {
                // Detach the existing entity
                _context.Entry(existingEntity).State = EntityState.Detached;
            }
            
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteConfig(int id)
        {
            var config = await _context.TaskConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.TaskConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTaskDuration(int regionId, string taskName, decimal? projectValue)
        {
            // Try to find specific config for this region
            var config = await _context.TaskConfigurations
                .FirstOrDefaultAsync(t => t.RegionId == regionId && t.TaskName == taskName);

            if (config == null)
            {
                // Fallback to default durations if not configured
                return GetDefaultDuration(taskName, projectValue);
            }

            if (config.IsValueBased && !string.IsNullOrEmpty(config.ValueThresholds) && projectValue.HasValue)
            {
                try
                {
                    var thresholds = JsonSerializer.Deserialize<List<ValueThreshold>>(config.ValueThresholds);
                    if (thresholds != null)
                    {
                        // Sort by limit descending to find the matching range
                        // Logic: find the first threshold where value >= limit? 
                        // Or find the range it falls into.
                        
                        // Based on user logic:
                        // < 20k -> 28
                        // 20k - 40k -> 42
                        // ...
                        
                        // Let's assume thresholds are defined as "Upper Limit" or "Lower Limit"?
                        // The user's logic was:
                        // < 20000
                        // >= 20000 && < 40000
                        
                        // Let's standardize on "Lower Limit".
                        // 0 -> 28
                        // 20000 -> 42
                        // 40000 -> 56
                        // ...
                        
                        // We find the largest Lower Limit that is <= projectValue
                        var match = thresholds
                            .Where(t => t.Limit <= projectValue.Value)
                            .OrderByDescending(t => t.Limit)
                            .FirstOrDefault();
                            
                        if (match != null)
                        {
                            return match.Days;
                        }
                    }
                }
                catch
                {
                    // Fallback on error
                }
            }

            return config.Duration;
        }

        private int GetDefaultDuration(string taskName, decimal? projectValue)
        {
            // Hardcoded defaults as fallback
            switch (taskName)
            {
                case "Project Files & Documentation": return 2;
                case "Fee Proposal & Costing": return 14;
                case "Document Preparation": return 2;
                case "Project Scoping": return 3;
                case "Tender Management":
                    if (projectValue.HasValue)
                    {
                        var val = projectValue.Value;
                        if (val < 20000) return 28;
                        if (val < 40000) return 42;
                        if (val < 250000) return 56;
                        if (val < 1000000) return 70;
                        return 84;
                    }
                    return 21; // Default
                case "Exception Management": return 7;
                case "Estimating Evaluation": return 7;
                case "Quote Issued": return 5;
                case "Quote Approved": return 1;
                case "Quote Received": return 1;
                case "Handover & Closeout": return 7;
                default: return 1;
            }
        }
        
        public class ValueThreshold
        {
            public decimal Limit { get; set; } // Lower limit
            public int Days { get; set; }
        }
    }
}
