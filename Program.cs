using CopelinSystem.Components;
using CopelinSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Windows Authentication
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Add HttpContextAccessor to access user info in components
builder.Services.AddHttpContextAccessor();

// Add DbContext Factory
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Register services - IMPORTANT: Order matters here!
builder.Services.AddScoped<CopelinSystem.Services.AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, CopelinAuthStateProvider>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ConsultantService>();
builder.Services.AddScoped<ContractorService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PermissionService>();

// Add memory cache for permission caching
builder.Services.AddMemoryCache();

// Add cascading authentication state
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

