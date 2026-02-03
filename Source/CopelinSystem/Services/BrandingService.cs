using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public interface IBrandingService
    {
        Task<string> GetFooterBrandingAsync();
    }

    public class BrandingService : IBrandingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        
        public BrandingService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<string> GetFooterBrandingAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Get the active branding. 
            // We prioritize the one with ID 1 which is our seeded default.
            var branding = await context.AppBranding.FirstOrDefaultAsync(b => b.Id == 1) 
                           ?? await context.AppBranding.FirstOrDefaultAsync();

            string rawHtml = branding?.FooterHtml ?? "";
            
            if (string.IsNullOrEmpty(rawHtml))
            {
                return "<footer><center>Copelin System</center></footer>";
            }
            
            // Placeholder logic
            string processedHtml = rawHtml
                .Replace("{{Year}}", DateTime.Now.Year.ToString())
                .Replace("{{Version}}", "2.0.0"); 

            return processedHtml;
        }
    }
}
