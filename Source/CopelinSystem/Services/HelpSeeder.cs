using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class HelpSeeder
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public HelpSeeder(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task EnsureGettingStartedGuide()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Check if "Getting Started" section exists
            var sectionTitle = "Getting Started";
            var section = await context.HelpSections
                .Include(s => s.Articles)
                .FirstOrDefaultAsync(s => s.Title == sectionTitle);

            if (section == null)
            {
                // Create Section
                section = new HelpSection
                {
                    Title = sectionTitle,
                    Order = 0 // Top priority
                };
                context.HelpSections.Add(section);
                await context.SaveChangesAsync();
            }

            // 2. Ensure Articles exist
            var articles = new List<(string Title, string Content, int Order)>
            {
                ("Welcome to Copelin", 
                 @"<p><strong>Welcome to the Copelin System!</strong></p>
                   <p>This system is designed to help you manage specific construction projects efficiently. Use this guide to learn the basics.</p>
                   <ul>
                    <li>Manage Projects and Estimates</li>
                    <li>Track Employee Time and Productivity</li>
                    <li>Generate Reports</li>
                   </ul>", 1),

                ("Navigating the Dashboard",
                 @"<p>The <strong>Dashboard</strong> is your central hub.</p>
                   <p>From the sidebar, you can access:</p>
                   <ul>
                    <li><strong>Projects:</strong> View and search all active projects.</li>
                    <li><strong>Estimator Tools:</strong> Access calculation tools.</li>
                    <li><strong>Reports:</strong> Generate detailed internal reports.</li>
                   </ul>", 2),

                ("Creating a Project",
                 @"<p>To create a new project:</p>
                   <ol>
                    <li>Navigate to the <strong>Projects</strong> page.</li>
                    <li>Click the <strong>Add Project</strong> button (if you have permission).</li>
                    <li>Fill in the required details such as Project Name, Client, and Location.</li>
                    <li>Click <strong>Save</strong> to initialize the project folder structure.</li>
                   </ol>", 3),
                   
                ("Managing Files",
                 @"<p>Every project has an integrated <strong>File Explorer</strong>.</p>
                   <p>Click on any project to view its details, then scroll down to the File Explorer section. You can upload, download, and zip files directly from the browser.</p>", 4)
            };

            foreach (var art in articles)
            {
                if (!section.Articles.Any(a => a.Title == art.Title))
                {
                    context.HelpArticles.Add(new HelpArticle
                    {
                        HelpSectionId = section.Id,
                        Title = art.Title,
                        Content = art.Content,
                        Order = art.Order,
                        MediaType = "None"
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
