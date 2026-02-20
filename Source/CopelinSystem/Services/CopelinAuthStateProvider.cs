public override async Task<AuthenticationState> GetAuthenticationStateAsync()
{
    // Development Mode Override
    if (_environment.IsDevelopment() && _devUser != null)
    {
        _currentUser = _devUser;
        return BuildAuthState(_currentUser, "DevAuth");
    }

    // Return cached state if available (SignalR circuit reuse)
    if (_cachedState != null && _currentUser != null)
    {
        return _cachedState;
    }

    try
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            _currentUser = await _authService.GetUserFromPrincipal(httpContext.User);

            if (_currentUser != null)
            {
                var authType = httpContext.User.Identity.AuthenticationType ?? "Cookies";
                _cachedState = BuildAuthState(_currentUser, authType);
                return _cachedState;
            }
        }
    }
    catch (ObjectDisposedException)
    {
        // HttpContext disposed during SignalR - return cached if we have it
        if (_cachedState != null) return _cachedState;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Auth Provider Error: {ex.Message}");
        if (_cachedState != null) return _cachedState;
    }

    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
}

private AuthenticationState BuildAuthState(User user, string authType)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.DisplayName),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim("UserId", user.UserId.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim("Region", user.Region ?? "")
    };

    var identity = new ClaimsIdentity(claims, authType);
    return new AuthenticationState(new ClaimsPrincipal(identity));
}