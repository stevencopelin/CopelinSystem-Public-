using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CopelinSystem.Models;
using CopelinSystem.Pages.FileExplorer;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CopelinSystem.Services
{
    public class FileSystemService
    {
        public string StorageRoot { get; }

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<FileSystemService> _logger;
        private readonly PermissionService _permissionService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public FileSystemService(
            IDbContextFactory<ApplicationDbContext> contextFactory, 
            ILogger<FileSystemService> logger, 
            PermissionService permissionService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _permissionService = permissionService;
            _environment = environment;
            _configuration = configuration;
            
            StorageRoot = _configuration["FileStorage:RootPath"] ?? @"\\srv2025\Pool2\Qbuild";
        }

        public async Task<List<string>> GetAuthorizedRegions(User user)
        {
            if (user == null) return new List<string>();

            using var context = await _contextFactory.CreateDbContextAsync();

            // Permissions check:
            if (await _permissionService.UserHasPermission(user, "ViewAllRegions"))
            {
                var regions = await context.ProjectLists
                    .Select(p => p.ProjectRegion)
                    .Distinct()
                    .ToListAsync();
                
                return regions
                    .Select(r => string.IsNullOrEmpty(r) ? "Unassigned" : r)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
            }
            else
            {
                // Estimators see regions where they have assigned projects
                var userIdStr = user.UserId.ToString();
                
                var regions = await context.ProjectLists
                    .Where(p => 
                        (p.ProjectSeniorEstimator == user.DisplayName) || 
                        (p.ProjectUserIds != null && p.ProjectUserIds.Contains(userIdStr)))
                    .Select(p => p.ProjectRegion)
                    .Distinct()
                    .ToListAsync();

                 return regions
                    .Select(r => string.IsNullOrEmpty(r) ? "Unassigned" : r)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
            }
        }

        public async Task<List<ProjectList>> GetProjectsInRegion(string region, User user)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.ProjectLists.AsQueryable();

            if (region == "Unassigned")
            {
                query = query.Where(p => string.IsNullOrEmpty(p.ProjectRegion));
            }
            else
            {
                query = query.Where(p => p.ProjectRegion == region);
            }

            // Permission check
            if (!await _permissionService.UserHasPermission(user, "ViewAllRegions"))
            {
                var userIdStr = user.UserId.ToString();
                 query = query.Where(p => 
                        (p.ProjectSeniorEstimator == user.DisplayName) ||
                        (p.ProjectUserIds != null && p.ProjectUserIds.Contains(userIdStr)));
            }

            return await query
                .OrderBy(p => p.ProjectName)
                .ToListAsync();
        }

        public async Task<ProjectList?> GetProjectById(int projectId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ProjectLists.FindAsync(projectId);
        }

        public async Task<List<FileSystemItem>> GetItems(int projectId, int? parentId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.FileSystemItems
                .Where(i => i.ProjectId == projectId && i.ParentId == parentId)
                .OrderByDescending(i => i.IsFolder)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<List<FileSystemSearchItem>> SearchFileSystem(string term, User user, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(term) || user == null) return new List<FileSystemSearchItem>();

            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.FileSystemItems.AsQueryable();

            // Permission Check: Filter projects user can see
            // This is complex. We must join with ProjectList.
            // Improve: Get list of allowed ProjectIDs first?
            // If "ViewAllRegions", they can see all.
            // Else restricted.

            var allowedProjectIds = new List<int>();

            if (await _permissionService.UserHasPermission(user, "ViewAllRegions"))
            {
                // All projects allowed.
                // Just search all items, then join project name.
                 query = query.Where(i => i.Name.Contains(term));
            }
            else
            {
                // Restricted
                var userIdStr = user.UserId.ToString();
                var allowedProjects = await context.ProjectLists
                    .Where(p => (p.ProjectSeniorEstimator == user.DisplayName) || 
                               (p.ProjectUserIds != null && p.ProjectUserIds.Contains(userIdStr)))
                    .Select(p => p.ProjectId)
                    .ToListAsync();
                
                query = query.Where(i => allowedProjects.Contains(i.ProjectId) && i.Name.Contains(term));
            }

            var results = await query
                .OrderByDescending(i => i.ModifiedDate)
                .Take(limit)
                .Join(context.ProjectLists,
                    item => item.ProjectId,
                    proj => proj.ProjectId,
                    (item, proj) => new FileSystemSearchItem
                    {
                        Item = item,
                        ProjectName = proj.ProjectName,
                        Region = proj.ProjectRegion,
                        Location = proj.ProjectLocation
                    })
                .ToListAsync();

            return results;
        }

        public async Task CreateDefaultFolders(int projectId, string userId)
        {
            var defaultFolders = new List<string>
            {
                "Asbestos & Lead",
                "Plans",
                "Photos",
                "Doc Preperation",
                "Doc Tender",
                "Clarrifications",
                "Quote",
                "Inter-Departmental Files",
                "Email-Attachments"
            };

            foreach (var folderName in defaultFolders)
            {
                try 
                {
                    // Assuming root level for default folders (parentId = null)
                    await CreateFolder(projectId, null, folderName, userId);
                }
                catch (Exception ex)
                {
                   _logger.LogError(ex, "Failed to create default folder '{FolderName}' for project {ProjectId}", folderName, projectId);
                   // Continue creating other folders even if one fails (e.g. if it already exists)
                }
            }
        }

        public async Task<int> CreateFolder(int projectId, int? parentId, string name, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Verify project exists
            var project = await context.ProjectLists.FindAsync(projectId);
            if (project == null) throw new ArgumentException("Project not found");

            // Ensure physical path structure
            var parentPath = await GetPhysicalPath(context, projectId, parentId);
            var newFolderPath = Path.Combine(parentPath, name);
            
            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
            }

            var item = new FileSystemItem
            {
                ProjectId = projectId,
                ParentId = parentId,
                Name = name,
                IsFolder = true,
                PhysicalPath = GetRelativePath(newFolderPath),
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            context.FileSystemItems.Add(item);
            await context.SaveChangesAsync();
            return item.Id;
        }

        public async Task<FileSystemItem> UploadFile(int projectId, int? parentId, IBrowserFile file, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

             // Verify project exists
            var project = await context.ProjectLists.FindAsync(projectId);
            if (project == null) throw new ArgumentException("Project not found");

            var parentPath = await GetPhysicalPath(context, projectId, parentId);
            if (!Directory.Exists(parentPath)) Directory.CreateDirectory(parentPath);

            var fileName = file.Name;
            // Handle duplicate names? For now overwrite or append
            // Let's safe-rename if exists?
            var fullPath = Path.Combine(parentPath, fileName);
            int count = 1;
            while (File.Exists(fullPath))
            {
                var nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);
                fullPath = Path.Combine(parentPath, $"{nameNoExt} ({count++}){ext}");
            }
            
            // Re-update filename if changed
            var storedFileName = Path.GetFileName(fullPath);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024).CopyToAsync(stream); // 50MB limit
            }

            var item = new FileSystemItem
            {
                ProjectId = projectId,
                ParentId = parentId,
                Name = storedFileName,
                IsFolder = false,
                PhysicalPath = GetRelativePath(fullPath),
                ContentType = file.ContentType,
                Size = file.Size,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            context.FileSystemItems.Add(item);
            await context.SaveChangesAsync();
            return item;
        }

        public async Task<FileSystemItem> SaveFileContentAsync(int projectId, string folderName, string fileName, byte[] content, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

             // Verify project exists
            var project = await context.ProjectLists.FindAsync(projectId);
            if (project == null) throw new ArgumentException("Project not found");

            // Check or Create Folder
            var folder = await GetFolderByName(projectId, folderName);
            int? parentId = null;
            if (folder != null)
            {
                parentId = folder.Id;
            }
            else
            {
                // Create folder at root if not exists
                try {
                    parentId = await CreateFolder(projectId, null, folderName, userId);
                } catch {
                     // Race condition or exists now
                     folder = await GetFolderByName(projectId, folderName);
                     parentId = folder?.Id;
                }
            }

            var parentPath = await GetPhysicalPath(context, projectId, parentId);
            if (!Directory.Exists(parentPath)) Directory.CreateDirectory(parentPath);

            // Handle duplicate names
            var fullPath = Path.Combine(parentPath, fileName);
            int count = 1;
            while (File.Exists(fullPath))
            {
                var nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);
                fullPath = Path.Combine(parentPath, $"{nameNoExt} ({count++}){ext}");
            }
            
            // Re-update filename if changed
            var storedFileName = Path.GetFileName(fullPath);

            await File.WriteAllBytesAsync(fullPath, content);

            // Register in DB
            var item = new FileSystemItem
            {
                ProjectId = projectId,
                ParentId = parentId,
                Name = storedFileName,
                IsFolder = false,
                PhysicalPath = GetRelativePath(fullPath),
                ContentType = "text/html", // Usually HTML for this flow
                Size = content.Length,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            context.FileSystemItems.Add(item);
            await context.SaveChangesAsync();
            return item;
        }

        public async Task<FileSystemItem> RegisterFile(int projectId, int? parentId, string filePath, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var fileInfo = new FileInfo(filePath);
            
            var item = new FileSystemItem
            {
                ProjectId = projectId,
                ParentId = parentId,
                Name = fileInfo.Name,
                IsFolder = false,
                PhysicalPath = GetRelativePath(filePath),
                ContentType = "application/octet-stream", // Identify correctly if possible, or update later
                Size = fileInfo.Length,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            context.FileSystemItems.Add(item);
            await context.SaveChangesAsync();
            return item;
        }

        public async Task DeleteItem(int itemId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var item = await context.FileSystemItems.FindAsync(itemId);
            if (item == null) return;

            // Recursive delete for folders? 
            // Simplified: Only allow delete if empty for folders, or implement recursive logic.
            // Let's implement recursive delete for DB items.
            await DeleteRecursive(context, item);
            await context.SaveChangesAsync();
        }
        
        private async Task DeleteRecursive(ApplicationDbContext context, FileSystemItem item)
        {
            if (item.IsFolder)
            {
                var children = await context.FileSystemItems.Where(i => i.ParentId == item.Id).ToListAsync();
                foreach (var child in children)
                {
                    await DeleteRecursive(context, child);
                }
            }

            // Physical Delete
            var fullPath = GetAbsolutePath(item.PhysicalPath);
            if (item.IsFolder)
            {
                if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);
            }
            else
            {
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            
            context.FileSystemItems.Remove(item);
        }

        public async Task RenameItem(int itemId, string newName)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var item = await context.FileSystemItems.FindAsync(itemId);
            if (item == null) return;

            var oldPath = GetAbsolutePath(item.PhysicalPath);
            var parentPath = Path.GetDirectoryName(oldPath)!;
            var newPath = Path.Combine(parentPath, newName);

            if (item.IsFolder)
            {
                if (Directory.Exists(oldPath)) Directory.Move(oldPath, newPath);
            }
            else
            {
                 if (File.Exists(oldPath)) File.Move(oldPath, newPath);
            }

            item.Name = newName;
            item.PhysicalPath = GetRelativePath(newPath);
            item.ModifiedDate = DateTime.Now;

            await context.SaveChangesAsync();
        }

        public async Task<FileSystemItem?> GetAuthorizedFile(int fileId, User user)
        {
            if (user == null) return null;

            using var context = await _contextFactory.CreateDbContextAsync();
            var item = await context.FileSystemItems.FindAsync(fileId);
            if (item == null) return null;

            // Permission Check
            if (await _permissionService.UserHasPermission(user, "ViewAllRegions"))
            {
                return item;
            }

            var project = await context.ProjectLists.FindAsync(item.ProjectId);
            if (project == null) return null; // Should not happen

            // Check if user is assigned or Senior Estimator
            var userIdStr = user.UserId.ToString();
            if (project.ProjectSeniorEstimator == user.DisplayName || 
                (project.ProjectUserIds != null && project.ProjectUserIds.Contains(userIdStr)))
            {
                return item;
            }

            return null;
        }

        public Stream GetFileStream(FileSystemItem item)
        {
            var fullPath = GetAbsolutePath(item.PhysicalPath);
            if (!File.Exists(fullPath)) throw new FileNotFoundException("File not found on disk");
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // Helpers
        
        public async Task<string> GetPhysicalPath(ApplicationDbContext context, int projectId, int? parentId)
        {
            // Base path: \\srv2025\Pool2\Qbuild\FY\Location\ProjectName
            var project = await context.ProjectLists.FindAsync(projectId);
            if (project == null) throw new Exception("Project not found");
            
            var location = project.ProjectLocation ?? "Unassigned Location";
            // Sanitize location
            location = string.Join("_", location.Split(Path.GetInvalidFileNameChars()));

            var fy = GetFinancialYear(project.ProjectDateCreated);
            // Sanitize FY just in case
            fy = string.Join("_", fy.Split(Path.GetInvalidFileNameChars()));

            var projectName = string.Join("_", project.ProjectName!.Split(Path.GetInvalidFileNameChars()));
            
            // Structure: Year \ Location \ Project
            var basePath = Path.Combine(StorageRoot, fy, location, projectName);

            if (parentId == null) return basePath;
            
            // Build path from parent hierarchy
            var parent = await context.FileSystemItems.FindAsync(parentId);
            if (parent == null) return basePath;
            
            return GetAbsolutePath(parent.PhysicalPath);
        }

        public string GetFinancialYear(DateTime date)
        {
            // Australian FY: July 1 - June 30
            // If Month >= 7, FY is Year-(Year+1)
            // If Month < 7, FY is (Year-1)-Year
            
            int startYear = date.Month >= 7 ? date.Year : date.Year - 1;
            int endYear = startYear + 1;
            
            return $"{startYear}-{endYear}";
        }

        public string GetAbsolutePath(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";
            var safeRelative = relativePath.TrimStart('/', '\\');
            return Path.Combine(StorageRoot, safeRelative);
        }
        
        public async Task<FileSystemItem?> GetFolderByName(int projectId, string folderName)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.FileSystemItems
                .Where(i => i.ProjectId == projectId && i.Name == folderName && i.IsFolder)
                .FirstOrDefaultAsync();
        }

        private string GetRelativePath(string absolutePath)
        {
            return Path.GetRelativePath(StorageRoot, absolutePath);
        }
    }
}
