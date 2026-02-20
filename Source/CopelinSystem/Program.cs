
using CopelinSystem;
using CopelinSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;


// Robust root finding for Parallels/Mac shared folders
var contentRoot = Directory.GetCurrentDirectory();
var wwwRoot = Path.Combine(contentRoot, "wwwroot");

if (!Directory.Exists(wwwRoot))
{
    // Try BaseDirectory (bin folder)
    var baseDir = AppContext.BaseDirectory;
    if (Directory.Exists(Path.Combine(baseDir, "wwwroot")))
    {
        contentRoot = baseDir;
    }
    // Else... let it fail or defaults handle it
}

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    // WebRootPath = "wwwroot" // Implicit
};

var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddControllers(); // Add API controller support
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Authentication
// Add Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => 
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });
builder.Services.AddAuthorization();

// Add HttpContextAccessor to access user info in components
builder.Services.AddHttpContextAccessor();

// Add DbContext Factory
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions => 
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Register services - IMPORTANT: Order matters here!
builder.Services.AddScoped<CopelinSystem.Services.PasswordHasher>();
builder.Services.AddScoped<CopelinSystem.Services.AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, CopelinAuthStateProvider>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ConsultantService>();
builder.Services.AddScoped<ContractorService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<RegionEmailService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<TaskConfigurationService>();
builder.Services.AddScoped<ReportingService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<HelpService>();
builder.Services.AddScoped<HelpSeeder>();

builder.Services.AddScoped<SubmissionTokenService>();
builder.Services.AddScoped<HtmlExportService>();
builder.Services.AddScoped<FileSystemService>();
builder.Services.AddScoped<ChecklistService>();

// Add background service for email processing
// NOTE: Commented out for development - enable in production when email credentials are configured
// builder.Services.AddHostedService<EmailReceiverService>();

// Add memory cache for permission caching
builder.Services.AddMemoryCache();

// Add cascading authentication state
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\inetpub\wwwroot\CopelinSystem\keys"))
    .SetApplicationName("CopelinSystem");

var app = builder.Build();
    
// Ensure permissions are seeded
using (var scope = app.Services.CreateScope())
{
    var permissionService = scope.ServiceProvider.GetRequiredService<PermissionService>();
    // Fire and forget or wait? Better to wait to ensure consistent startup state
    // Since we are in top-level context, we can just block relative to startup
    try 
    {
        Task.Run(async () => await permissionService.EnsureReportsPermission()).GetAwaiter().GetResult();
        await permissionService.EnsureReportsPermission();
        await permissionService.EnsureEstimatorToolsPermission();
        await permissionService.EnsureHelpSystemPermission();
        await permissionService.EnsureRequestInfoPermission();
        await permissionService.EnsureExternalRegionPermission();
        await permissionService.EnsureFileSystemPermission();
        await permissionService.EnsureProjectViewPermissions();
        
        // Seed Help Content
        var helpSeeder = scope.ServiceProvider.GetRequiredService<HelpSeeder>();
        await helpSeeder.EnsureGettingStartedGuide();
        Task.Run(async () => await permissionService.EnsureRequestInfoPermission()).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding permissions: {ex.Message}");
        // Continue startup anyway
    }

    // Seed Passwords if needed
    try 
    {
        Console.WriteLine("--- STARTING PASSWORD SEEDING CHECK ---");
        var authService = scope.ServiceProvider.GetRequiredService<CopelinSystem.Services.AuthenticationService>();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        
        using var context = dbContextFactory.CreateDbContext();
        
        // Ensure Admin user exists so we can always login
        await authService.EnsureAdminUserExists();
        
        var usersWithoutPassword = await context.Users
            .Where(u => u.PasswordHash == null)
            .ToListAsync();

        if (usersWithoutPassword.Any())
        {
            Console.WriteLine($"Seeding passwords for {usersWithoutPassword.Count} users...");
            // Use a default password for migration
            string defaultPassword = "Password123!";
            
            foreach (var user in usersWithoutPassword)
            {
                // We need to use AuthService to set password to ensure hashing
                await authService.SetPassword(user.UserId, defaultPassword);
            }
            Console.WriteLine("Passwords seeded successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--- ERROR SEEDING PASSWORDS: {ex.Message} ---");
        Console.WriteLine(ex.StackTrace);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

// Add authentication and authorization middleware - ORDER IS IMPORTANT
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers(); // Map API controller endpoints
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

