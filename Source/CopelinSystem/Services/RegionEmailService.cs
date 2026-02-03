using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class RegionEmailService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly PermissionService _permissionService;

        public RegionEmailService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            PermissionService permissionService)
        {
            _contextFactory = contextFactory;
            _permissionService = permissionService;
        }

        public async Task<List<ExternalRegionEmail>> GetEmailsByRegion(int regionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ExternalRegionEmails
                .Where(e => e.RegionId == regionId)
                .ToListAsync();
        }

        public async Task<List<ExternalRegionEmail>> GetAllEmails()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ExternalRegionEmails
                .Include(e => e.Region)
                .OrderBy(e => e.Region!.RegionName)
                .ThenBy(e => e.Department)
                .ToListAsync();
        }

        public async Task<string?> GetEmailForDepartment(int regionId, string department)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var config = await context.ExternalRegionEmails
                .FirstOrDefaultAsync(e => e.RegionId == regionId && e.Department == department);
            
            return config?.EmailAddress;
        }

        public async Task<bool> SaveEmail(ExternalRegionEmail email, User currentUser)
        {
            // Verify permission
            if (!await _permissionService.UserHasPermission(currentUser, "ManageExternalRegion"))
            {
                throw new UnauthorizedAccessException("User does not have permission to manage regional emails.");
            }

            // Verify region access (unless Admin)
            if (currentUser.Role != UserRole.Admin)
            {
                // Must act within own region
                if (string.IsNullOrEmpty(currentUser.Region))
                {
                     throw new UnauthorizedAccessException("User does not have a region assigned.");
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                var region = await context.Regions.FirstOrDefaultAsync(r => r.RegionId == email.RegionId);
                
                if (region == null || region.RegionName != currentUser.Region)
                {
                    throw new UnauthorizedAccessException("User can only manage emails for their own region.");
                }
            }

            using var db = await _contextFactory.CreateDbContextAsync();
            
            if (email.Id == 0)
            {
                db.ExternalRegionEmails.Add(email);
            }
            else
            {
                db.ExternalRegionEmails.Update(email);
            }

            await db.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteEmail(int id, User currentUser)
        {
             // Verify permission
            if (!await _permissionService.UserHasPermission(currentUser, "ManageExternalRegion"))
            {
                throw new UnauthorizedAccessException("User does not have permission to manage regional emails.");
            }
            
            using var db = await _contextFactory.CreateDbContextAsync();
            var email = await db.ExternalRegionEmails.Include(e => e.Region).FirstOrDefaultAsync(e => e.Id == id);
            
            if (email == null) return false;

             // Verify region access (unless Admin)
            if (currentUser.Role != UserRole.Admin)
            {
                if (string.IsNullOrEmpty(currentUser.Region) || email.Region?.RegionName != currentUser.Region)
                {
                    throw new UnauthorizedAccessException("User can only manage emails for their own region.");
                }
            }

            db.ExternalRegionEmails.Remove(email);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
