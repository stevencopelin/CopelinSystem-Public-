using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class ClientService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ClientService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get all active clients
        /// </summary>
        public async Task<List<Client>> GetAllClients(bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Clients
                .Include(c => c.Region)
                .Include(c => c.ClientContacts)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderBy(c => c.ClientCode)
                .ToListAsync();
        }

        /// <summary>
        /// Get active clients by region (for cascading lookup)
        /// </summary>
        public async Task<List<Client>> GetClientsByRegion(int regionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Clients
                .Include(c => c.Region)
                .Include(c => c.ClientContacts)
                .Where(c => c.IsActive && c.RegionId == regionId)
                .OrderBy(c => c.ClientName)
                .ToListAsync();
        }

        /// <summary>
        /// Get client by ID
        /// </summary>
        public async Task<Client?> GetClientById(int clientId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Clients
                .Include(c => c.Region)
                .Include(c => c.ClientContacts)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        /// <summary>
        /// Add new client
        /// </summary>
        public async Task<Client> AddClient(Client client)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            client.DateCreated = DateTime.Now;
            client.IsActive = true;
            context.Clients.Add(client);
            await context.SaveChangesAsync();
            return client;
        }

        /// <summary>
        /// Update existing client
        /// </summary>
        public async Task<bool> UpdateClient(Client client)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                context.Clients.Update(client);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Soft delete client (set IsActive = false)
        /// </summary>
        public async Task<bool> DeleteClient(int clientId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var client = await context.Clients.FindAsync(clientId);
                if (client == null) return false;

                client.IsActive = false;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== CLIENT CONTACT METHODS ====================

        /// <summary>
        /// Get all active contacts for a client (for cascading lookup)
        /// </summary>
        public async Task<List<ClientContact>> GetClientContactsByClient(int clientId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ClientContacts
                .Where(cc => cc.ClientId == clientId && cc.IsActive)
                .OrderByDescending(cc => cc.IsPrimary)
                .ThenBy(cc => cc.ContactName)
                .ToListAsync();
        }

        /// <summary>
        /// Get primary contact for a client
        /// </summary>
        public async Task<ClientContact?> GetPrimaryContact(int clientId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ClientContacts
                .Where(cc => cc.ClientId == clientId && cc.IsActive && cc.IsPrimary)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get client contact by ID
        /// </summary>
        public async Task<ClientContact?> GetClientContactById(int clientContactId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ClientContacts
                .Include(cc => cc.Client)
                .FirstOrDefaultAsync(cc => cc.ClientContactId == clientContactId);
        }

        /// <summary>
        /// Add new client contact
        /// </summary>
        public async Task<ClientContact> AddClientContact(ClientContact contact)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            contact.DateCreated = DateTime.Now;
            contact.IsActive = true;

            // If this is set as primary, unset other primary contacts for this client
            if (contact.IsPrimary)
            {
                var existingPrimary = await context.ClientContacts
                    .Where(cc => cc.ClientId == contact.ClientId && cc.IsPrimary)
                    .ToListAsync();

                foreach (var existing in existingPrimary)
                {
                    existing.IsPrimary = false;
                }
            }

            context.ClientContacts.Add(contact);
            await context.SaveChangesAsync();
            return contact;
        }

        /// <summary>
        /// Update existing client contact
        /// </summary>
        public async Task<bool> UpdateClientContact(ClientContact contact)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                // If this is set as primary, unset other primary contacts for this client
                if (contact.IsPrimary)
                {
                    var existingPrimary = await context.ClientContacts
                        .Where(cc => cc.ClientId == contact.ClientId && cc.IsPrimary && cc.ClientContactId != contact.ClientContactId)
                        .ToListAsync();

                    foreach (var existing in existingPrimary)
                    {
                        existing.IsPrimary = false;
                    }
                }

                context.ClientContacts.Update(contact);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Soft delete client contact (set IsActive = false)
        /// </summary>
        public async Task<bool> DeleteClientContact(int clientContactId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var contact = await context.ClientContacts.FindAsync(clientContactId);
                if (contact == null) return false;

                contact.IsActive = false;
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
