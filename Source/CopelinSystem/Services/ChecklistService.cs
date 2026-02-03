using CopelinSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CopelinSystem.Services
{
    public class ChecklistService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ChecklistService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // ---------------- Templates ----------------

        public async Task<List<ChecklistTemplate>> GetAllTemplatesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChecklistTemplates
                .OrderBy(t => t.Category).ThenBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ChecklistTemplate?> GetTemplateByIdAsync(int templateId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChecklistTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);
        }

        public async Task<ChecklistTemplate> CreateTemplateAsync(ChecklistTemplate template)
        {
            using var context = _contextFactory.CreateDbContext();
            context.ChecklistTemplates.Add(template);
            await context.SaveChangesAsync();
            return template;
        }

        public async Task UpdateTemplateAsync(ChecklistTemplate template)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var existing = await context.ChecklistTemplates
                .Include(t => t.Sections)
                .ThenInclude(s => s.Questions)
                .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId);

            if (existing != null)
            {
                // 1. Update Template Scalars
                context.Entry(existing).CurrentValues.SetValues(template);

                // 2. Sync Sections
                foreach (var section in template.Sections)
                {
                    var existingSection = existing.Sections.FirstOrDefault(s => s.SectionId == section.SectionId);

                    if (existingSection == null)
                    {
                        // New Section
                        // We must create a new instance to avoid attaching the 'section' object which might be tracked by another context or have issues
                        // Actually 'section' is from Blazor (detached), so we can add it, but safer to treat it as detached data.
                        // However, adding 'section' directly to 'existing.Sections' is fine if 'section' is not tracked.
                        // But wait, 'section' contains 'Questions'. We need to be careful.
                        // Simplest: Add it. EF will treat it as new if ID=0.
                        if (section.SectionId == 0)
                        {
                            existing.Sections.Add(section);
                        }
                    }
                    else
                    {
                        // Update Existing Section
                        context.Entry(existingSection).CurrentValues.SetValues(section);

                        // 3. Sync Questions within Section
                        foreach (var question in section.Questions)
                        {
                             var existingQuestion = existingSection.Questions.FirstOrDefault(q => q.QuestionId == question.QuestionId);
                             if (existingQuestion == null)
                             {
                                 if (question.QuestionId == 0)
                                 {
                                     existingSection.Questions.Add(question);
                                 }
                             }
                             else
                             {
                                 context.Entry(existingQuestion).CurrentValues.SetValues(question);
                                 
                                 // 4. Sync Options within Question
                                 foreach (var option in question.Options)
                                 {
                                     var existingOption = existingQuestion.Options.FirstOrDefault(o => o.OptionId == option.OptionId);
                                     if (existingOption == null)
                                     {
                                         if (option.OptionId == 0)
                                         {
                                             existingQuestion.Options.Add(option);
                                         }
                                     }
                                     else
                                     {
                                         context.Entry(existingOption).CurrentValues.SetValues(option);
                                     }
                                 }
                             }
                        }
                    }
                }
                
                // Note: Deletions are handled by specific delete methods called from UI immediately.
                // We rely on that for now to keep this sync logic simpler (additive/update only).

                await context.SaveChangesAsync();
            }
        }
        
        // Granular Template Management for easier Blazor interaction
        public async Task AddSectionAsync(ChecklistSection section)
        {
             using var context = _contextFactory.CreateDbContext();
             context.ChecklistSections.Add(section);
             // Using context.Set<ChecklistSection>() is safer if DbSet isn't explicit in basic scaffolding.
             context.ChecklistSections.Add(section);
             await context.SaveChangesAsync();
        }
        
        public async Task UpdateSectionAsync(ChecklistSection section)
        {
             using var context = _contextFactory.CreateDbContext();
             context.ChecklistSections.Update(section);
             await context.SaveChangesAsync();
        }

        public async Task DeleteSectionAsync(int sectionId)
        {
             using var context = _contextFactory.CreateDbContext();
             var s = await context.ChecklistSections.FindAsync(sectionId);
             if(s != null) { context.ChecklistSections.Remove(s); await context.SaveChangesAsync(); }
        }

        public async Task AddQuestionAsync(ChecklistQuestion question)
        {
             using var context = _contextFactory.CreateDbContext();
             context.ChecklistQuestions.Add(question);
             await context.SaveChangesAsync();
        }
        
        public async Task UpdateQuestionAsync(ChecklistQuestion question)
        {
             using var context = _contextFactory.CreateDbContext();
             context.ChecklistQuestions.Update(question);
             await context.SaveChangesAsync();
        }
        
         public async Task DeleteQuestionAsync(int questionId)
        {
             using var context = _contextFactory.CreateDbContext();
             var q = await context.ChecklistQuestions.FindAsync(questionId);
             if(q != null) { context.ChecklistQuestions.Remove(q); await context.SaveChangesAsync(); }
        }

        public async Task DeleteQuestionOptionAsync(int optionId)
        {
             using var context = _contextFactory.CreateDbContext();
             var o = await context.ChecklistQuestionOptions.FindAsync(optionId);
             if(o != null) { context.ChecklistQuestionOptions.Remove(o); await context.SaveChangesAsync(); }
        }

        public async Task DeleteTemplateAsync(int templateId)
        {
            using var context = _contextFactory.CreateDbContext();
            var t = await context.ChecklistTemplates.FindAsync(templateId);
            if (t != null)
            {
                context.ChecklistTemplates.Remove(t);
                await context.SaveChangesAsync();
            }
        }

        // ---------------- Instances ----------------

        public async Task<List<ProjectChecklist>> GetProjectChecklistsAsync(int projectId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ProjectChecklists
                .Include(c => c.Template)
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProjectChecklist?> GetChecklistInstanceAsync(int instanceId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ProjectChecklists
                .Include(c => c.Template!)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(s => s.Questions)
                            .ThenInclude(q => q.Options)
                .Include(c => c.Responses)
                .FirstOrDefaultAsync(c => c.ChecklistInstanceId == instanceId);
        }

        public async Task<ProjectChecklist> StartChecklistAsync(int projectId, int templateId, string user)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var instance = new ProjectChecklist
            {
                ProjectId = projectId,
                TemplateId = templateId,
                CreatedBy = user,
                CreatedDate = DateTime.Now,
                Status = "InProgress"
            };

            context.ProjectChecklists.Add(instance);
            await context.SaveChangesAsync();
            return instance;
        }

        public async Task SaveResponseAsync(ChecklistResponse response)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var existing = await context.ChecklistResponses
                .FirstOrDefaultAsync(r => r.ChecklistInstanceId == response.ChecklistInstanceId && r.QuestionId == response.QuestionId);

            if (existing != null)
            {
                existing.ResponseValue = response.ResponseValue;
                existing.Notes = response.Notes;
                existing.ResponseDate = DateTime.Now;
                existing.RespondedBy = response.RespondedBy;
            }
            else
            {
                context.ChecklistResponses.Add(response);
            }

            await context.SaveChangesAsync();
        }

        public async Task CompleteChecklistAsync(int instanceId, string user)
        {
            using var context = _contextFactory.CreateDbContext();
            var instance = await context.ProjectChecklists.FindAsync(instanceId);
            if (instance != null)
            {
                instance.Status = "Completed";
                instance.CompletedBy = user;
                instance.CompletedDate = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }
    }
}
