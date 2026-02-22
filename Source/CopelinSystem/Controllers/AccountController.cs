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

        [HttpGet("windows-login")]
        public async Task<IActionResult> WindowsLogin([FromQuery] string? returnUrl = "/")
        {
            var identity = HttpContext.User.Identity;

#pragma warning disable CA1416 // Validate platform compatibility
            // Check if Windows Auth was successful at the IIS level
            if (identity is System.Security.Principal.WindowsIdentity windowsIdentity && windowsIdentity.IsAuthenticated)
            {
                var user = await _authService.GetOrCreateUserFromWindowsIdentity(windowsIdentity);

                if (user != null)
                {
                    // Build local claims to authenticate via our cookie scheme
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.DisplayName),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim("UserId", user.UserId.ToString()),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim("Region", user.Region ?? ""),
                        new Claim("DisplayName", user.DisplayName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in with Cookies
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return Redirect(returnUrl ?? "/");
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility

            // Fallback: If no Windows Identity exists or it failed to map to a user, send them back to manual login
            // flag adCheck=false prevents an infinite redirect loop
            return Redirect($"/login?adCheck=false&returnUrl={System.Net.WebUtility.UrlEncode(returnUrl)}");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}
