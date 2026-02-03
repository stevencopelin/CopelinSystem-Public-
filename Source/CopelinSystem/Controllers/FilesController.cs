using System;
using System.IO;
using System.Threading.Tasks;
using CopelinSystem.Services;
using CopelinSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;

namespace CopelinSystem.Controllers
{
    // [Authorize] removed to allow token authentication
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileSystemService _fileService;
        private readonly AuthenticationService _authService;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            FileSystemService fileService, 
            AuthenticationService authService, 
            IMemoryCache cache,
            ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _authService = authService;
            _contentTypeProvider = new FileExtensionContentTypeProvider();
            _cache = cache;
            _logger = logger;
        }

        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetFileContent(int id, [FromQuery] string? token = null)
        {
            var user = await GetUser(token);
            if (user == null) return Unauthorized();

            return await ServeFile(id, user, inline: false);
        }
        
        [HttpGet("{id}/view")]
        public async Task<IActionResult> ViewFile(int id, [FromQuery] string? token = null)
        {
            _logger.LogInformation("Requesting view for file ID: {Id}", id);
            
            var user = await GetUser(token);
            if (user == null) 
            {
                _logger.LogWarning("User not found or not authenticated for file ID: {Id}", id);
                return Unauthorized();
            }

            return await ServeFile(id, user, inline: true);
        }

        private async Task<User?> GetUser(string? token)
        {
             // Priority 1: Standard Auth (Cookies/Windows)
             var user = await _authService.GetUserFromPrincipal(User);
             if (user != null) return user;

             // Priority 2: Token Auth (for non-cookie scenarios like Mac Dev)
             if (!string.IsNullOrEmpty(token) && _cache.TryGetValue(token, out int userId))
             {
                 return await _authService.GetUserById(userId);
             }

             return null;
        }

        private async Task<IActionResult> ServeFile(int id, User user, bool inline)
        {
            var item = await _fileService.GetAuthorizedFile(id, user);
            if (item == null) 
            {
                if (inline) _logger.LogWarning("Authorization failed for ID: {Id} User: {User}", id, user.AdUsername);
                return NotFound();
            }
            if (item.IsFolder) return BadRequest("Cannot download a folder");

            try 
            {
                var stream = _fileService.GetFileStream(item);
                
                if (!_contentTypeProvider.TryGetContentType(item.Name, out var contentType))
                {
                    contentType = item.ContentType ?? "application/octet-stream";
                }
                
                if (inline)
                {
                    Response.Headers.Append("Content-Disposition", "inline; filename=\"" + item.Name + "\"");
                    return File(stream, contentType);
                }
                
                return File(stream, contentType, item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving file ID: {Id}", id);
                if (ex is FileNotFoundException) return NotFound("File resource missing");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
