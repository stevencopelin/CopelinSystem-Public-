using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class PermissionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY_PREFIX = "Permissions_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public PermissionService(IDbContextFactory<ApplicationDbContext> contextFactory, IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }

        /// <summary>
        /// Check if a role has a specific permission
        /// </summary>
        public async Task<bool> HasPermission(UserRole role, string permissionName)
        {
            // Safety net: Admin always has all permissions
            if (role == UserRole.Admin)
            {
                return true;
            }

            var rolePermissions = await GetRolePermissions(role);
            return rolePermissions.Any(p => p.PermissionName == permissionName && p.IsGranted);
        }

        /// <summary>
        /// Get all available permissions in the system
        /// </summary>
        public async Task<List<Permission>> GetAllPermissions()
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}All";
            
            if (_cache.TryGetValue(cacheKey, out List<Permission>? cachedPermissions) && cachedPermissions != null)
            {
                return cachedPermissions;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            var permissions = await context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayName)
                .ToListAsync();

            _cache.Set(cacheKey, permissions, CacheDuration);
            return permissions;
        }

        /// <summary>
        /// Get permissions for a specific role with granted status
        /// </summary>
        public async Task<List<PermissionWithStatus>> GetRolePermissions(UserRole role)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}Role_{(byte)role}";
            
            if (_cache.TryGetValue(cacheKey, out List<PermissionWithStatus>? cachedPermissions) && cachedPermissions != null)
            {
                return cachedPermissions;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var allPermissions = await context.Permissions.ToListAsync();
            var rolePermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == (byte)role)
                .Include(rp => rp.Permission)
                .ToListAsync();

            var result = allPermissions.Select(p =>
            {
                var rolePermission = rolePermissions.FirstOrDefault(rp => rp.PermissionId == p.PermissionId);
                return new PermissionWithStatus
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Category = p.Category,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    IsGranted = role == UserRole.Admin || (rolePermission?.IsGranted ?? false)
                };
            }).ToList();

            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }

        /// <summary>
        /// Get full permission matrix for all roles
        /// </summary>
        public async Task<Dictionary<UserRole, List<PermissionWithStatus>>> GetPermissionMatrix()
        {
            var matrix = new Dictionary<UserRole, List<PermissionWithStatus>>();
            
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                matrix[role] = await GetRolePermissions(role);
            }

            return matrix;
        }

        /// <summary>
        /// Update a role's permission
        /// </summary>
        public async Task<bool> UpdateRolePermission(UserRole role, int permissionId, bool isGranted)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var rolePermission = await context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == (byte)role && rp.PermissionId == permissionId);

            if (rolePermission != null)
            {
                // Update existing
                rolePermission.IsGranted = isGranted;
                context.RolePermissions.Update(rolePermission);
            }
            else
            {
                // Create new
                rolePermission = new RolePermission
                {
                    RoleId = (byte)role,
                    PermissionId = permissionId,
                    IsGranted = isGranted
                };
                context.RolePermissions.Add(rolePermission);
            }

            await context.SaveChangesAsync();
            
            // Clear cache for this role
            _cache.Remove($"{CACHE_KEY_PREFIX}Role_{(byte)role}");
            
            return true;
        }

        /// <summary>
        /// Get permissions grouped by category
        /// </summary>
        public async Task<Dictionary<string, List<Permission>>> GetPermissionsByCategory()
        {
            var permissions = await GetAllPermissions();
            return permissions.GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Clear all permission caches
        /// </summary>
        public void ClearCache()
        {
            _cache.Remove($"{CACHE_KEY_PREFIX}All");
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                _cache.Remove($"{CACHE_KEY_PREFIX}Role_{(byte)role}");
            }
        }

        /// <summary>
        /// Check if current user has permission (convenience method)
        /// </summary>
        public async Task<bool> UserHasPermission(User? user, string permissionName)
        {
            if (user == null) return false;
            return await HasPermission(user.Role, permissionName);
        }

        /// <summary>
        /// Get all granted permission names for a user
        /// </summary>
        public async Task<List<string>> GetUserPermissionNames(User? user)
        {
            if (user == null) return new List<string>();
            var permissions = await GetRolePermissions(user.Role);
            return permissions.Where(p => p.IsGranted).Select(p => p.PermissionName).ToList();
        }

        /// <summary>
        /// Check if user has any of the specified permissions
        /// </summary>
        public async Task<bool> UserHasAnyPermission(User? user, params string[] permissionNames)
        {
            if (user == null || permissionNames == null || permissionNames.Length == 0) return false;
            
            foreach (var permissionName in permissionNames)
            {
                if (await HasPermission(user.Role, permissionName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if user has all of the specified permissions
        /// </summary>
        public async Task<bool> UserHasAllPermissions(User? user, params string[] permissionNames)
        {
            if (user == null || permissionNames == null || permissionNames.Length == 0) return false;
            
            foreach (var permissionName in permissionNames)
            {
                if (!await HasPermission(user.Role, permissionName))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Helper class for permission with granted status
    /// </summary>
    public class PermissionWithStatus
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsGranted { get; set; }
    }
}
