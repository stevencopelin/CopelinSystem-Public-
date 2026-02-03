using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class AuthenticationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AuthenticationService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get or create a user from Windows Identity
        /// </summary>
        public async Task<User?> GetOrCreateUserFromWindowsIdentity(IIdentity identity)
        {
            if (identity == null || !identity.IsAuthenticated)
                return null;

            var windowsIdentity = identity as WindowsIdentity;
            if (windowsIdentity == null)
                return null;

            var sid = windowsIdentity.User?.Value;
            var name = windowsIdentity.Name; // DOMAIN\username
            
            if (string.IsNullOrEmpty(sid))
                return null;

            using var context = await _contextFactory.CreateDbContextAsync();

            // Try to find by SID first
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.AdSid == sid);

            // If not found by SID, try by username (in case SID changed or migration)
            if (user == null && !string.IsNullOrEmpty(name))
            {
                user = await context.Users
                    .FirstOrDefaultAsync(u => u.AdUsername == name);
            }

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    AdSid = sid,
                    AdUsername = name,
                    DateCreated = DateTime.Now,
                    LastActive = DateTime.Now,
                    UserType = (byte)UserRole.ReadOnly // Default role
                };

                // Parse domain and username
                if (!string.IsNullOrEmpty(name) && name.Contains("\\"))
                {
                    var parts = name.Split('\\');
                    user.AdDomain = parts[0];
                    // We might want to set a default display name from the username part
                    // But usually we'd want to query AD for real name. 
                    // For now, let's just set Firstname/Lastname to parts of the username if possible or placeholders
                    user.Firstname = parts[1]; 
                }
                else
                {
                    user.Firstname = name;
                }

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            else
            {
                // Update existing user
                user.LastActive = DateTime.Now;
                
                // Update SID if it was missing (matched by username)
                if (string.IsNullOrEmpty(user.AdSid))
                {
                    user.AdSid = sid;
                }

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return user;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<User?> GetUserById(int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.FindAsync(userId);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public async Task<List<User>> GetAllUsers()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .OrderBy(u => u.Firstname)
                .ThenBy(u => u.Lastname)
                .ToListAsync();
        }

        /// <summary>
        /// Check if user can edit data (Estimator or higher)
        /// </summary>
        public bool CanEditData(User user)
        {
            if (user == null) return false;
            return user.Role >= UserRole.Estimator;
        }

        /// <summary>
        /// Check if user is Manager or higher
        /// </summary>
        public bool IsManagerOrHigher(User user)
        {
            if (user == null) return false;
            return user.Role >= UserRole.Manager;
        }

        /// <summary>
        /// Check if user is Admin
        /// </summary>
        public bool IsAdmin(User user)
        {
            if (user == null) return false;
            return user.Role == UserRole.Admin;
        }

        /// <summary>
        /// Ensure an Admin user exists (for Dev environment)
        /// </summary>
        public async Task<User> EnsureAdminUserExists()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@copelin.com");
            
            if (admin == null)
            {
                // Check if ANY admin exists
                admin = await context.Users.FirstOrDefaultAsync(u => u.UserType == (byte)UserRole.Admin);
                
                if (admin == null)
                {
                    admin = new User
                    {
                        Firstname = "System",
                        Lastname = "Admin",
                        Email = "admin@copelin.com",
                        UserType = (byte)UserRole.Admin,
                        Region = "All",
                        AdUsername = "admin",
                        DateCreated = DateTime.Now,
                        LastActive = DateTime.Now
                    };
                    context.Users.Add(admin);
                    await context.SaveChangesAsync();
                }
            }
            return admin;
        }
        /// <summary>
        /// Ensure a User exists for an Employee (for Dev environment)
        /// </summary>
        public async Task<User> EnsureUserForEmployee(Employee employee, UserRole role)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Try to match by email
            string email = $"{employee.FullName.Replace(" ", ".").ToLower()}@copelin.com";
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Firstname = employee.FullName.Split(' ').FirstOrDefault() ?? employee.FullName,
                    Lastname = employee.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    Email = email,
                    UserType = (byte)role,
                    Region = employee.Region?.RegionName,
                    AdUsername = employee.FullName.Replace(" ", ""),
                    DateCreated = DateTime.Now,
                    LastActive = DateTime.Now
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            else
            {
                // Update existing user's role and last active time
                user.UserType = (byte)role;
                user.LastActive = DateTime.Now;
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return user;
        }
    }
}