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

#pragma warning disable CA1416 // Validate platform compatibility
            var sid = windowsIdentity.User?.Value;
            var rawName = windowsIdentity.Name; // DOMAIN\username
#pragma warning restore CA1416 // Validate platform compatibility
            
            if (string.IsNullOrEmpty(sid))
                return null;

            // NORMALIZE: Fix double domain issues (e.g. DPWSERVICES\DPWSERVICES\User -> DPWSERVICES\User)
            var name = NormalizeIdentityName(rawName);

            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Try to find by SID matches
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.AdSid == sid);

            // 2. If not found by SID, try by normalized username
            if (user == null && !string.IsNullOrEmpty(name))
            {
                user = await context.Users
                    .FirstOrDefaultAsync(u => u.AdUsername == name);
            }

            // 3. Smart Fallback for Domain Mismatches (e.g. DPWSERVICES\User vs DPWSERVICES.DPW.QLD.GOV.AU\User)
            if (user == null && !string.IsNullOrEmpty(name) && name.Contains("\\"))
            {
                var parts = name.Split('\\');
                if (parts.Length == 2)
                {
                    var domain = parts[0];
                    var username = parts[1];

                    // 3a. Try Exact Username Match (ignoring domain)
                    // e.g. "DPWSERVICES\Steven.COPELIN" matches "Steven.COPELIN"
                    user = await context.Users
                        .FirstOrDefaultAsync(u => u.AdUsername == username);

                    if (user == null)
                    {
                        // 3b. Try Domain Fuzzy Match
                        // Find users with the same username part (checking fully qualified domains)
                        var candidateUsers = await context.Users
                            .Where(u => u.AdUsername != null && u.AdUsername.EndsWith("\\" + username))
                            .ToListAsync();

                        user = candidateUsers.FirstOrDefault(u => 
                        {
                            var dbParts = u.AdUsername?.Split('\\');
                            if (dbParts?.Length != 2) return false;
                            
                            var dbDomain = dbParts[0];
                            return dbDomain.StartsWith(domain, StringComparison.OrdinalIgnoreCase) || 
                                   domain.StartsWith(dbDomain, StringComparison.OrdinalIgnoreCase);
                        });
                    }
                }
            }

            if (user == null)
            {
                // Create new user using the NORMALIZED name
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
                    var usernamePart = parts[1];
                    
                    // Parse Token for First/Last Name
                    if (usernamePart.Contains("."))
                    {
                        var nameParts = usernamePart.Split('.');
                        user.Firstname = nameParts[0];
                        if (nameParts.Length > 1) user.Lastname = nameParts[1];
                    }
                    else if (usernamePart.Contains(" "))
                    {
                        var nameParts = usernamePart.Split(' ');
                        user.Firstname = nameParts[0];
                        if (nameParts.Length > 1) user.Lastname = nameParts[1];
                    }
                    else
                    {
                        user.Firstname = usernamePart;
                    }

                    // Generate Email
                    user.Email = $"{usernamePart.Replace(" ", ".").ToLower()}@hpw.qld.gov.au";
                }
                else
                {
                    // Fallback for email if no domain
                    user.Email = $"{name.Replace(" ", ".").ToLower()}@hpw.qld.gov.au";
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

                // If we matched via Smart Fallback/SID but the AdUsername in DB is different/outdated, 
                // we might consider updating it? 
                // For now, let's NOT change AdUsername to avoid breaking other things, but rely on SID for future.

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return user;
        }

        private string NormalizeIdentityName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            // Fix known double-domain issue
            // e.g. "DPWSERVICES\DPWSERVICES\Steven.COPELIN" -> "DPWSERVICES\Steven.COPELIN"
            if (name.Contains("\\"))
            {
                var parts = name.Split('\\');
                if (parts.Length >= 3)
                {
                    // Detect repeated domain
                    if (parts[0].Equals(parts[1], StringComparison.OrdinalIgnoreCase))
                    {
                        // Reconstruct without the first usage
                        // "Domain\Domain\User" -> "Domain\User"
                        return string.Join("\\", parts.Skip(1));
                    }
                }
            }

            return name;
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
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hpw.qld.gov.au");
            
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
                        Email = "admin@hpw.qld.gov.au",
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
            string email = $"{employee.FullName.Replace(" ", ".").ToLower()}@hpw.qld.gov.au";
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
        /// <summary>
        /// Helper to get User from ClaimsPrincipal
        /// </summary>
        /// <summary>
        /// Helper to get User from ClaimsPrincipal
        /// </summary>
        public async Task<User?> GetUserFromPrincipal(System.Security.Claims.ClaimsPrincipal principal)
        {
            if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
                return null;
            
            // Priority 1: Check for UserId claim (most reliable)
            var userIdClaim = principal.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = await GetUserById(userId);
                if (user != null) return user;
            }

            // Priority 2: Try WindowsIdentity
            if (principal.Identity is WindowsIdentity windowsIdentity)
            {
               return await GetOrCreateUserFromWindowsIdentity(windowsIdentity);
            }
            else 
            {
                // Fallback for non-Windows (e.g. Mac/Linux dev) using standard claims
                string? sid = principal.FindFirst(ClaimTypes.PrimarySid)?.Value 
                              ?? principal.FindFirst(ClaimTypes.Sid)?.Value;
                              
                string? name = principal.Identity.Name 
                               ?? principal.FindFirst(ClaimTypes.Name)?.Value;
                               
                if (string.IsNullOrEmpty(name)) return null;
                
                // If no SID, maybe generate one or use name
                // For dev purposes, we can try to use name as unique identifier if SID is missing
                
                return await GetOrCreateUserFromIdentityInfo(sid, name);
            }
        }

        private async Task<User?> GetOrCreateUserFromIdentityInfo(string? sid, string name)
        {
             if (string.IsNullOrEmpty(name)) return null;

             using var context = await _contextFactory.CreateDbContextAsync();
             User? user = null;

             // Try to find by SID matches
             if (!string.IsNullOrEmpty(sid))
             {
                 user = await context.Users.FirstOrDefaultAsync(u => u.AdSid == sid);
             }

             // Fallback to name match
             if (user == null)
             {
                 user = await context.Users.FirstOrDefaultAsync(u => u.AdUsername == name);
             }

             // Fallback: try removing spaces from name (e.g. "Kerbee Beck" -> "KerbeeBeck")
             if (user == null && name.Contains(" "))
             {
                 var nameNoSpace = name.Replace(" ", "");
                 user = await context.Users.FirstOrDefaultAsync(u => u.AdUsername == nameNoSpace);
             }

             // Create if not exists
             if (user == null)
             {
                 user = new User
                 {
                     AdSid = sid,
                     AdUsername = name,
                     DateCreated = DateTime.Now,
                     LastActive = DateTime.Now,
                     UserType = (byte)UserRole.ReadOnly // Default to ReadOnly
                 };

                 if (!string.IsNullOrEmpty(name))
                 {
                    // Generate Email from name if possible (First.Last@copelin.com)
                    user.Email = $"{name.Replace(" ", ".").ToLower()}@hpw.qld.gov.au";

                    if (name.Contains("\\"))
                    {
                        var parts = name.Split('\\');
                        user.AdDomain = parts[0];
                        user.Firstname = parts[1];
                        // Update email based on firstname if needed, but the simple replace above is usually good enough for "Domain\First Last"
                    }
                    else if (name.Contains(" "))
                    {
                        var parts = name.Split(' ');
                        user.Firstname = parts[0]; 
                        // If there's a last name
                        if (parts.Length > 1) user.Lastname = parts[1];
                    }
                    else
                    {
                        user.Firstname = name;
                    }
                 }
                 
                 // DEV/TEST HACK: Ensure Sean Reardon is Principal Estimator for testing
                 if (user.Firstname != null && user.Firstname.Equals("Sean", StringComparison.OrdinalIgnoreCase))
                 {
                     user.UserType = (byte)UserRole.PrincipalEstimator;
                 }
                 
                 context.Users.Add(user);
                 await context.SaveChangesAsync();
             }
             else
             {
                 // Update
                 user.LastActive = DateTime.Now;
                 if (string.IsNullOrEmpty(user.AdSid) && !string.IsNullOrEmpty(sid))
                 {
                     user.AdSid = sid;
                 }
                 

                 // DEV/TEST HACK: Ensure Sean Reardon is Principal Estimator for testing (even if existing)
                 // This ensures the role is corrected if it was previously created as ReadOnly
                 if ((user.Firstname != null && user.Firstname.Contains("Sean", StringComparison.OrdinalIgnoreCase)) || 
                     (user.AdUsername != null && user.AdUsername.Contains("Sean", StringComparison.OrdinalIgnoreCase)))
                 {
                     if (user.UserType == (byte)UserRole.ReadOnly)
                     {
                         user.UserType = (byte)UserRole.PrincipalEstimator;
                     }
                 }

                 context.Users.Update(user);
                 await context.SaveChangesAsync();
             }

             return user;
        }
    }
}