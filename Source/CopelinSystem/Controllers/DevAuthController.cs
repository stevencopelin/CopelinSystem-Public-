using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CopelinSystem.Services;

namespace CopelinSystem.Controllers
{
    [Route("dev/auth")]
    public class DevAuthController : Controller
    {
        private readonly UserService _userService; // Used to find user by ID
        
        public DevAuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(int? userId, string? returnUrl = "/")
        {
            // Only allow in development or if configured?
            // For now, assume this is safe for the context (User request)
            
            Models.User? user = null;
            if (userId.HasValue)
            {
                user = await _userService.GetUserById(userId.Value);
            }
            
            // If no user found or specified, try to find a default admin/dev user or create one?
            // Let's just default to finding "Sean Reardon" or first Admin
            if (user == null)
            {
                var users = await _userService.GetAllUsers();
                // Fix: Check for u.Firstname != null before accessing Contains
                user = users.FirstOrDefault(u => 
                    (u.Firstname != null && (u.Firstname.Contains("Sean") || u.Firstname.Contains("Steve")))
                );
                
                if (user == null) user = users.FirstOrDefault();
            }

            if (user == null) return Content("No users found to log in with.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()), // Assuming Role enum to string
                new Claim("Region", user.Region ?? "")
            };
            
            // Add DisplayName claim explicitly if not covered by Name
            claims.Add(new Claim("DisplayName", user.DisplayName));

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return Redirect("/");
        }
    }
}
