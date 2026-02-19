using CopelinSystem.Models;
using CopelinSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace CopelinSystem.Services
{
    public class CopelinAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationService _authService;
        private readonly IWebHostEnvironment _environment;
        private User? _currentUser;
        private static User? _devUser; // Static for persistence across circuits in Dev

        public CopelinAuthStateProvider(
            IHttpContextAccessor httpContextAccessor,
            AuthenticationService authService,
            IWebHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
            _environment = environment;
        }

        public User? CurrentUser => _currentUser;

        private AuthenticationState? _cachedState;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Development Mode Override
            if (_environment.IsDevelopment() && _devUser != null)
            {
                _currentUser = _devUser;
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, _currentUser.DisplayName),
                    new Claim(ClaimTypes.Email, _currentUser.Email ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, _currentUser.UserId.ToString()),
                    new Claim("UserId", _currentUser.UserId.ToString()),
                    new Claim(ClaimTypes.Role, _currentUser.Role.ToString()),
                    new Claim("Region", _currentUser.Region ?? "")
                };

                var identity = new ClaimsIdentity(claims, "DevAuth");
                var principal = new ClaimsPrincipal(identity);
                return new AuthenticationState(principal);
            }

            // Return cached state if available (for SignalR circuits)
            if (_cachedState != null)
            {
                return _cachedState;
            }

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    // Get user from Principal (supports both Cookies and Windows)
                    _currentUser = await _authService.GetUserFromPrincipal(httpContext.User);

                    if (_currentUser != null)
                    {
                        // Create claims with user info
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, _currentUser.DisplayName),
                            new Claim(ClaimTypes.Email, _currentUser.Email ?? ""),
                            new Claim(ClaimTypes.NameIdentifier, _currentUser.UserId.ToString()),
                            new Claim("UserId", _currentUser.UserId.ToString()),
                            new Claim(ClaimTypes.Role, _currentUser.Role.ToString()),
                            new Claim("Region", _currentUser.Region ?? "")
                        };

                        // Use the original authentication type (e.g. "Cookies" or "Windows") or default to "Custom"
                        var authType = httpContext.User.Identity.AuthenticationType ?? "Custom";
                        var identity = new ClaimsIdentity(claims, authType);
                        var principal = new ClaimsPrincipal(identity);

                        _cachedState = new AuthenticationState(principal);
                        return _cachedState;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // HttpContext is disposed (likely in SignalR circuit after initial request)
                // If we haven't cached a user by now, we assume unauthenticated.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth Provider Error: {ex.Message}");
            }

            // Not authenticated - Cache this result to avoid retrying HttpContext
            _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            return _cachedState;
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // Method to clear cached state (useful when permissions change)
        public void ClearCache()
        {
            _cachedState = null;
            NotifyAuthenticationStateChanged();
        }

        // Method to set user in development mode
        public void SetDevUser(User user)
        {
            if (_environment.IsDevelopment())
            {
                _devUser = user;
                NotifyAuthenticationStateChanged();
            }
        }

        // Method to logout in development mode
        public void Logout()
        {
            if (_environment.IsDevelopment())
            {
                _devUser = null;
                _currentUser = null;
                _cachedState = null;
                NotifyAuthenticationStateChanged();
            }
        }
    }
}