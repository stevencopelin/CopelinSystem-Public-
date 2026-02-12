using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class ConsultantService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ConsultantService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get all active consultants
        /// </summary>
        public async Task<List<Consultant>> GetAllConsultants(bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Consultants.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderBy(c => c.BusinessName)
                .ToListAsync();
        }

        /// <summary>
        /// Get consultant by ID
        /// </summary>
        public async Task<Consultant?> GetConsultantById(int consultantId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Consultants.FindAsync(consultantId);
        }

        /// <summary>
        /// Add new consultant
        /// </summary>
        public async Task<Consultant> AddConsultant(Consultant consultant)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            consultant.DateCreated = DateTime.Now;
            consultant.IsActive = true;
            context.Consultants.Add(consultant);
            await context.SaveChangesAsync();
            return consultant;
        }

        /// <summary>
        /// Update existing consultant
        /// </summary>
        public async Task<bool> UpdateConsultant(Consultant consultant)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                context.Consultants.Update(consultant);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Soft delete consultant (set IsActive = false)
        /// </summary>
        public async Task<bool> DeleteConsultant(int consultantId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var consultant = await context.Consultants.FindAsync(consultantId);
                if (consultant == null) return false;

                consultant.IsActive = false;
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
