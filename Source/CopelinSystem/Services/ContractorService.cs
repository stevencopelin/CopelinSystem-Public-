using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class ContractorService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ContractorService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get all active contractors
        /// </summary>
        public async Task<List<Contractor>> GetAllContractors(bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Contractors.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderBy(c => c.BusinessName)
                .ToListAsync();
        }

        /// <summary>
        /// Get contractor by ID
        /// </summary>
        public async Task<Contractor?> GetContractorById(int contractorId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Contractors.FindAsync(contractorId);
        }

        /// <summary>
        /// Add new contractor
        /// </summary>
        public async Task<Contractor> AddContractor(Contractor contractor)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            contractor.DateCreated = DateTime.Now;
            contractor.IsActive = true;
            context.Contractors.Add(contractor);
            await context.SaveChangesAsync();
            return contractor;
        }

        /// <summary>
        /// Update existing contractor
        /// </summary>
        public async Task<bool> UpdateContractor(Contractor contractor)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                context.Contractors.Update(contractor);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Soft delete contractor (set IsActive = false)
        /// </summary>
        public async Task<bool> DeleteContractor(int contractorId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var contractor = await context.Contractors.FindAsync(contractorId);
                if (contractor == null) return false;

                contractor.IsActive = false;
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
