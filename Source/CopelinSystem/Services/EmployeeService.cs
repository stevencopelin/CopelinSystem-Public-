using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class EmployeeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly AuthenticationService _authService;

        public EmployeeService(IDbContextFactory<ApplicationDbContext> contextFactory, AuthenticationService authService)
        {
            _contextFactory = contextFactory;
            _authService = authService;
        }

        /// <summary>
        /// Get all active employees
        /// </summary>
        public async Task<List<Employee>> GetAllEmployees()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Employees
                .Include(e => e.Region)
                .Include(e => e.EmployeeRoles)
                .Where(e => e.Active)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Get active employees by role type
        /// </summary>
        public async Task<List<Employee>> GetEmployeesByRole(RoleType roleType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Employees
                .Include(e => e.Region)
                .Include(e => e.EmployeeRoles)
                .Where(e => e.Active && e.EmployeeRoles.Any(r => r.RoleType == roleType))
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Get active supervisors
        /// </summary>
        public async Task<List<Employee>> GetSupervisors()
        {
            return await GetEmployeesByRole(RoleType.Supervisor);
        }

        /// <summary>
        /// Get active senior estimators (includes both Senior and Principal)
        /// </summary>
        public async Task<List<Employee>> GetSeniorEstimators()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Employees
                .Include(e => e.Region)
                .Include(e => e.EmployeeRoles)
                .Where(e => e.Active && e.EmployeeRoles.Any(r => 
                    r.RoleType == RoleType.SeniorEstimator || 
                    r.RoleType == RoleType.PrincipalEstimator))
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        public async Task<Employee?> GetEmployeeById(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Employees
                .Include(e => e.Region)
                .Include(e => e.EmployeeRoles)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        /// <summary>
        /// Add new employee
        /// </summary>
        public async Task<Employee> AddEmployee(Employee employee)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            employee.Active = true;
            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            return employee;
        }

        /// <summary>
        /// Update existing employee
        /// </summary>
        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var existingEmployee = await context.Employees.FindAsync(employee.EmployeeId);
                if (existingEmployee == null) return false;

                existingEmployee.FullName = employee.FullName;
                existingEmployee.RegionId = employee.RegionId;
                existingEmployee.Active = employee.Active;
                
                await context.SaveChangesAsync();
                
                // Sync to User table to ensure Region/Roles match
                await SyncUserRole(employee.EmployeeId);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Soft delete employee (set Active = false)
        /// </summary>
        public async Task<bool> DeleteEmployee(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null) return false;

                employee.Active = false;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Add role to employee
        /// </summary>
        public async Task<bool> AddEmployeeRole(int employeeId, RoleType roleType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                // Check if role already exists
                var existingRole = await context.EmployeeRoles
                    .FirstOrDefaultAsync(er => er.EmployeeId == employeeId && er.RoleType == roleType);
                
                if (existingRole != null) return true; // Already exists

                var employeeRole = new EmployeeRole
                {
                    EmployeeId = employeeId,
                    RoleType = roleType
                };

                context.EmployeeRoles.Add(employeeRole);
                await context.SaveChangesAsync();
                
                // Sync to User table
                await SyncUserRole(employeeId);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove role from employee
        /// </summary>
        public async Task<bool> RemoveEmployeeRole(int employeeId, RoleType roleType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var employeeRole = await context.EmployeeRoles
                    .FirstOrDefaultAsync(er => er.EmployeeId == employeeId && er.RoleType == roleType);
                
                if (employeeRole == null) return false;

                context.EmployeeRoles.Remove(employeeRole);
                await context.SaveChangesAsync();
                
                // Sync to User table
                await SyncUserRole(employeeId);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Synchronize Employee Roles to User Roles
        /// </summary>
        public async Task SyncUserRole(int employeeId)
        {
            // 1. Get Employee with Roles
            var employee = await GetEmployeeById(employeeId);
            if (employee == null) return;
            
            // 2. Determine highest UserRole
            UserRole targetRole = UserRole.ReadOnly; // Default
            
            var roles = employee.EmployeeRoles.Select(r => r.RoleType).ToList();
            
            if (roles.Contains(RoleType.Administrator))
            {
                targetRole = UserRole.Admin;
            }
            else if (roles.Contains(RoleType.PrincipalEstimator))
            {
                targetRole = UserRole.PrincipalEstimator;
            }
            else if (roles.Contains(RoleType.PlanningManager)) // Planning Manager maps to Manager or Principal? Let's say Manager for now as per plan
            {
                targetRole = UserRole.Manager;
            }
            else if (roles.Contains(RoleType.SeniorEstimator))
            {
                // Map Senior Estimator to Estimator (or Manager if needed, but per plan Estimator is safer)
                targetRole = UserRole.Estimator; 
            }
            else if (roles.Contains(RoleType.Estimator))
            {
                targetRole = UserRole.Estimator;
            }
            // Supervisor and ReadOnly map to ReadOnly (default)
            
            // 3. Update User
            await _authService.EnsureUserForEmployee(employee, targetRole);
        }

        /// <summary>
        /// Get all regions
        /// </summary>
        public async Task<List<Region>> GetAllRegions()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Regions
                .OrderBy(r => r.RegionName)
                .ToListAsync();
        }

        /// <summary>
        /// Add new region
        /// </summary>
        public async Task<Region> AddRegion(Region region)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Regions.Add(region);
            await context.SaveChangesAsync();
            return region;
        }

        /// <summary>
        /// Get region by ID
        /// </summary>
        public async Task<Region?> GetRegionById(int regionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Regions
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.RegionId == regionId);
        }

        /// <summary>
        /// Update existing region
        /// </summary>
        public async Task<bool> UpdateRegion(Region region)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                context.Regions.Update(region);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete region (only if no employees are assigned)
        /// </summary>
        public async Task<bool> DeleteRegion(int regionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var region = await context.Regions
                    .Include(r => r.Employees)
                    .FirstOrDefaultAsync(r => r.RegionId == regionId);
                
                if (region == null) return false;

                // Check if any employees are assigned to this region
                if (region.Employees.Any())
                {
                    return false; // Cannot delete region with assigned employees
                }

                context.Regions.Remove(region);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all employees including inactive ones (for admin interface)
        /// </summary>
        public async Task<List<Employee>> GetAllEmployeesIncludingInactive()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Employees
                .Include(e => e.Region)
                .Include(e => e.EmployeeRoles)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Get all employees with their roles as a formatted list (for admin display)
        /// </summary>
        public async Task<List<(Employee Employee, List<string> Roles)>> GetAllEmployeesWithRoles()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var employees = await context.Employees
                .Include(e => e.Region)
                .Include(e => e.EmployeeRoles)
                .Where(e => e.Active)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var result = new List<(Employee, List<string>)>();
            foreach (var employee in employees)
            {
                var roles = employee.EmployeeRoles.Select(r => r.RoleType switch
                {
                    RoleType.Supervisor => "Supervisor",
                    RoleType.SeniorEstimator => "Senior Estimator",
                    RoleType.PrincipalEstimator => "Principal Estimator",
                    RoleType.PlanningManager => "Planning Manager",
                    RoleType.Estimator => "Estimator",
                    RoleType.ReadOnly => "Read Only",
                    RoleType.Administrator => "Administrator",
                    _ => "Unknown"
                }).ToList();

                result.Add((employee, roles));
            }

            return result;
        }

        /// <summary>
        /// Get roles for a specific employee
        /// </summary>
        public async Task<List<RoleType>> GetEmployeeRoles(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.EmployeeRoles
                .Where(er => er.EmployeeId == employeeId)
                .Select(er => er.RoleType)
                .ToListAsync();
        }

        /// <summary>
        /// Activate employee (set Active = true)
        /// </summary>
        public async Task<bool> ActivateEmployee(int employeeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null) return false;

                employee.Active = true;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if an employee name already exists (for validation)
        /// </summary>
        public async Task<bool> EmployeeNameExists(string fullName, int? excludeEmployeeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Employees.Where(e => e.FullName.ToLower() == fullName.ToLower());
            
            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.EmployeeId != excludeEmployeeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Check if a region name already exists (for validation)
        /// </summary>
        public async Task<bool> RegionNameExists(string regionName, int? excludeRegionId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Regions.Where(r => r.RegionName.ToLower() == regionName.ToLower());
            
            if (excludeRegionId.HasValue)
            {
                query = query.Where(r => r.RegionId != excludeRegionId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Get employee count by region (for reporting)
        /// </summary>
        public async Task<Dictionary<string, int>> GetEmployeeCountByRegion()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Employees
                .Where(e => e.Active && e.RegionId != null)
                .Include(e => e.Region)
                .GroupBy(e => e.Region!.RegionName)
                .Select(g => new { RegionName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RegionName, x => x.Count);
        }

        /// <summary>
        /// Get employee count by role (for reporting)
        /// </summary>
        public async Task<Dictionary<string, int>> GetEmployeeCountByRole()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var roleCounts = await context.EmployeeRoles
                .Join(context.Employees, er => er.EmployeeId, e => e.EmployeeId, (er, e) => new { er.RoleType, e.Active })
                .Where(x => x.Active)
                .GroupBy(x => x.RoleType)
                .Select(g => new { RoleType = g.Key, Count = g.Count() })
                .ToListAsync();

            return roleCounts.ToDictionary(
                x => x.RoleType switch
                {
                    RoleType.Supervisor => "Supervisor",
                    RoleType.SeniorEstimator => "Senior Estimator",
                    RoleType.PrincipalEstimator => "Principal Estimator",
                    RoleType.PlanningManager => "Planning Manager",
                    RoleType.Estimator => "Estimator",
                    RoleType.ReadOnly => "Read Only",
                    RoleType.Administrator => "Administrator",
                    _ => "Unknown"
                },
                x => x.Count
            );
        }
    }
}
