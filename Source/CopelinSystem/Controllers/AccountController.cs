using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CopelinSystem.Services;
using AuthenticationService = CopelinSystem.Services.AuthenticationService;

namespace CopelinSystem.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly AuthenticationService _authService;
        
        public AccountController(AuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl = "/")
        {
            var user = await _authService.ValidateUser(username, password);
            
            if (user == null)
            {
                return Redirect($"/login?error=Invalid credentials&returnUrl={returnUrl}");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Region", user.Region ?? "")
            };
            
            // Add DisplayName claim explicitly if not covered by Name
            claims.Add(new Claim("DisplayName", user.DisplayName));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in with Cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}
