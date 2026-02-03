using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class SubmissionTokenService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public SubmissionTokenService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Guid> GenerateToken(int projectId, string dept)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var token = new SubmissionToken
            {
                Token = Guid.NewGuid(),
                ProjectId = projectId,
                Department = dept,
                DateCreated = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(7), // Token valid for 7 days
                IsUsed = false
            };

            context.SubmissionTokens.Add(token);
            await context.SaveChangesAsync();

            return token.Token;
        }

        public async Task<SubmissionToken?> ValidateToken(Guid token)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var tokenEntity = await context.SubmissionTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (tokenEntity == null) return null;
            if (tokenEntity.IsUsed) return null;
            if (tokenEntity.ExpiresAt < DateTime.Now) return null;

            return tokenEntity;
        }

        public async Task MarkTokenUsed(Guid token)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var tokenEntity = await context.SubmissionTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (tokenEntity != null)
            {
                tokenEntity.IsUsed = true;
                tokenEntity.UsedAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<SubmissionNotificationDto>> GetRecentSubmissions(int userId, int days = 3)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var cutoffDAte = DateTime.Now.AddDays(-days);
            string userIdStr = userId.ToString();

            // 1. Get IDs of tokens dismissed by this user
            var dismissedTokenIds = await context.SubmissionNotificationDismissals
                .Where(d => d.UserId == userId)
                .Select(d => d.TokenId)
                .ToListAsync();

            // 2. Query Submissions
            var submissions = await context.SubmissionTokens
                .Where(t => t.IsUsed && t.UsedAt >= cutoffDAte && !dismissedTokenIds.Contains(t.Token))
                .OrderByDescending(t => t.UsedAt)
                .Join(context.ProjectLists,
                    token => token.ProjectId,
                    project => project.ProjectId,
                    (token, project) => new { Token = token, Project = project })
                .ToListAsync();

            // 3. Filter in memory for complex project-user logic (ProjectUserIds parsing)
            // Or improve the LINQ query if possible. string.Contains is supported in EF Core usually.
            
            var filteredSubmissions = submissions.Where(x => 
                x.Project.ProjectManagerID == userId ||
                (x.Project.ProjectUserIds != null && x.Project.ProjectUserIds.Contains(userIdStr))
            ).Select(x => new SubmissionNotificationDto
            {
                TokenId = x.Token.Token,
                ProjectId = x.Project.ProjectId,
                ProjectName = x.Project.ProjectName,
                ProjectWr = x.Project.ProjectWr,
                Department = x.Token.Department,
                UsedAt = x.Token.UsedAt
            }).ToList();

            return filteredSubmissions;
        }

        public async Task<bool> DismissNotification(Guid token, int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Check if already dismissed
            var existing = await context.SubmissionNotificationDismissals
                .AnyAsync(d => d.TokenId == token && d.UserId == userId);
            
            if (existing) return true;

            var dismissal = new SubmissionNotificationDismissal
            {
                TokenId = token,
                UserId = userId,
                DismissedAt = DateTime.Now
            };

            context.SubmissionNotificationDismissals.Add(dismissal);
            
            try 
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
