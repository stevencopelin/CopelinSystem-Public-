using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CopelinSystem.Models;

using System.IO;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;

namespace CopelinSystem.Services
{
    public class HelpService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;
        private const string CACHE_KEY_ALL_HELP = "Help_AllSections";

        public HelpService(IDbContextFactory<ApplicationDbContext> contextFactory, IMemoryCache cache, IWebHostEnvironment environment)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _environment = environment;
        }

        public async Task<string> UploadMediaAsync(IBrowserFile file)
        {
            try 
            {
                // Ensure directory exists
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "help");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate safe filename
                var extension = Path.GetExtension(file.Name);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file (limit to 50MB for now, adjustable)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024).CopyToAsync(stream);
                }

                // Return relative URL
                return $"/uploads/help/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        // --- SECTIONS ---

        public async Task<List<HelpSection>> GetAllSectionsAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY_ALL_HELP, out List<HelpSection>? sections) && sections != null)
            {
                return sections;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            sections = await context.HelpSections
                .Include(s => s.Articles.OrderBy(a => a.Order))
                .OrderBy(s => s.Order)
                .ToListAsync();

            _cache.Set(CACHE_KEY_ALL_HELP, sections, TimeSpan.FromHours(1));
            return sections;
        }

        public async Task<HelpSection?> GetSectionByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.HelpSections
                .Include(s => s.Articles)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<HelpSection> CreateSectionAsync(HelpSection section)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Set order to last if not specified
            if (section.Order == 0)
            {
                var maxOrder = await context.HelpSections.MaxAsync(s => (int?)s.Order) ?? 0;
                section.Order = maxOrder + 1;
            }

            context.HelpSections.Add(section);
            await context.SaveChangesAsync();
            _cache.Remove(CACHE_KEY_ALL_HELP);
            return section;
        }

        public async Task<HelpSection> UpdateSectionAsync(HelpSection section)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var existing = await context.HelpSections.FindAsync(section.Id);
            if (existing == null) throw new Exception("Section not found");

            context.Entry(existing).CurrentValues.SetValues(section);
            await context.SaveChangesAsync();
            _cache.Remove(CACHE_KEY_ALL_HELP);
            return existing;
        }

        public async Task DeleteSectionAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var section = await context.HelpSections.FindAsync(id);
            if (section != null)
            {
                context.HelpSections.Remove(section);
                await context.SaveChangesAsync();
                _cache.Remove(CACHE_KEY_ALL_HELP);
            }
        }

        // --- ARTICLES ---

        public async Task<HelpArticle?> GetArticleByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.HelpArticles
                .Include(a => a.HelpSection)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<HelpArticle> CreateArticleAsync(HelpArticle article)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Set order to last in section if not specified
            if (article.Order == 0)
            {
                var maxOrder = await context.HelpArticles
                    .Where(a => a.HelpSectionId == article.HelpSectionId)
                    .MaxAsync(a => (int?)a.Order) ?? 0;
                article.Order = maxOrder + 1;
            }

            context.HelpArticles.Add(article);
            await context.SaveChangesAsync();
            _cache.Remove(CACHE_KEY_ALL_HELP);
            return article;
        }

        public async Task<HelpArticle> UpdateArticleAsync(HelpArticle article)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var existing = await context.HelpArticles.FindAsync(article.Id);
            if (existing == null) throw new Exception("Article not found");

            context.Entry(existing).CurrentValues.SetValues(article);
            await context.SaveChangesAsync();
            _cache.Remove(CACHE_KEY_ALL_HELP);
            return existing;
        }

        public async Task DeleteArticleAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var article = await context.HelpArticles.FindAsync(id);
            if (article != null)
            {
                context.HelpArticles.Remove(article);
                await context.SaveChangesAsync();
                _cache.Remove(CACHE_KEY_ALL_HELP);
            }
        }
        
        public async Task UpdateSectionOrder(List<HelpSection> orderedSections)
        {
             using var context = await _contextFactory.CreateDbContextAsync();
             foreach(var section in orderedSections)
             {
                 var dbSection = await context.HelpSections.FindAsync(section.Id);
                 if(dbSection != null)
                 {
                     dbSection.Order = section.Order;
                 }
             }
             await context.SaveChangesAsync();
             _cache.Remove(CACHE_KEY_ALL_HELP);
        }
        
        public async Task UpdateArticleOrder(List<HelpArticle> orderedArticles)
        {
             using var context = await _contextFactory.CreateDbContextAsync();
             foreach(var article in orderedArticles)
             {
                 var dbArticle = await context.HelpArticles.FindAsync(article.Id);
                 if(dbArticle != null)
                 {
                     dbArticle.Order = article.Order;
                 }
             }
             await context.SaveChangesAsync();
             _cache.Remove(CACHE_KEY_ALL_HELP);
        }
    }
}
