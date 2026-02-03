using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class HtmlExportService
    {
        private readonly IWebHostEnvironment _env;

        public HtmlExportService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string ExportProjectToHtml(ProjectList project, List<TaskList> tasks, List<UserProductivity> productivity, Dictionary<int, string> userNames, Dictionary<int, string> taskNames, List<ProjectChecklist>? checklists = null)
        {
            var sb = new StringBuilder();

            // Try to get base64 logo
            string logoBase64 = "";
            try
            {
                string logoPath = Path.Combine(_env.WebRootPath, "uploads", "logo", "Queensland-Government-Logo.png");
                if (File.Exists(logoPath))
                {
                    byte[] imageArray = File.ReadAllBytes(logoPath);
                    logoBase64 = Convert.ToBase64String(imageArray);
                }
            }
            catch { /* Ignore logo if error occurs */ }

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"    <title>Project Export - {project.ProjectName}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.4; color: #333; margin: 20px; background-color: #f4f7f6; }");
            sb.AppendLine("        .container { max-width: 1100px; margin: auto; background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); position: relative; }");
            sb.AppendLine("        h1, h2, h3 { color: #004a99; margin-top: 25px; margin-bottom: 15px; border-bottom: 1px solid #eee; padding-bottom: 5px; }");
            sb.AppendLine("        .header { border-bottom: 3px solid #004a99; padding-bottom: 10px; margin-bottom: 20px; display: flex; justify-content: space-between; align-items: flex-start; }");
            sb.AppendLine("        .header-text { flex-grow: 1; }");
            sb.AppendLine("        .header h1 { border-bottom: none; margin: 0; }");
            sb.AppendLine("        .header p { margin: 5px 0 0 0; color: #666; font-size: 0.9em; }");
            sb.AppendLine("        .logo { max-width: 180px; max-height: 80px; margin-left: 20px; }");
            sb.AppendLine("        .info-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px 40px; margin-bottom: 20px; }");
            sb.AppendLine("        .info-item { display: flex; justify-content: space-between; border-bottom: 1px solid #f0f0f0; padding: 4px 0; }");
            sb.AppendLine("        .info-label { font-weight: bold; color: #555; font-size: 0.9em; }");
            sb.AppendLine("        .info-value { color: #222; font-size: 0.9em; text-align: right; }");
            sb.AppendLine("        .description-box { background: #f9f9f9; padding: 15px; border-left: 4px solid #004a99; margin: 20px 0; font-size: 0.95em; white-space: pre-wrap; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin-top: 15px; background-color: #fff; font-size: 0.85em; }");
            sb.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px 12px; text-align: left; }");
            sb.AppendLine("        th { background-color: #004a99; color: white; }");
            sb.AppendLine("        tr:nth-child(even) { background-color: #f9f9f9; }");
            sb.AppendLine("        .badge { display: inline-block; padding: 2px 6px; border-radius: 4px; font-size: 0.8em; font-weight: bold; }");
            sb.AppendLine("        .badge-pending { background-color: #6c757d; color: white; }");
            sb.AppendLine("        .badge-inprogress { background-color: #007bff; color: white; }");
            sb.AppendLine("        .badge-done { background-color: #28a745; color: white; }");
            sb.AppendLine("        .footer { margin-top: 40px; text-align: center; color: #777; font-size: 0.8em; border-top: 1px solid #ddd; padding-top: 20px; }");
            sb.AppendLine("        .section-title { background: #e9ecef; padding: 5px 10px; font-weight: bold; margin-top: 20px; border-radius: 4px; }");
            sb.AppendLine("        @media print {");
            sb.AppendLine("            body { background-color: white; margin: 0; padding: 0; }");
            sb.AppendLine("            .container { box-shadow: none; max-width: none; width: 100%; border-radius: 0; padding: 0; }");
            sb.AppendLine("            .no-print { display: none; }");
            sb.AppendLine("            .info-grid { grid-template-columns: repeat(2, 1fr); gap: 10px 30px; }");
            sb.AppendLine("        }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            
            // Header
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <div class=\"header-text\">");
            sb.AppendLine($"                <h1>{project.ProjectLocation} - {project.ProjectName}</h1>");
            sb.AppendLine($"                <p>Created {project.ProjectDateCreated:MMMM dd, yyyy} | Exported on {DateTime.Now:MMMM dd, yyyy HH:mm}</p>");
            sb.AppendLine("            </div>");
            if (!string.IsNullOrEmpty(logoBase64))
            {
                sb.AppendLine($"            <img src=\"data:image/png;base64,{logoBase64}\" alt=\"Qld Government Logo\" class=\"logo\">");
            }
            sb.AppendLine("        </div>");

            // Primary Details
            sb.AppendLine("        <div class=\"info-grid\">");
            AddInfoRow(sb, "(WR) Work Request Number", project.ProjectWr);
            AddInfoRow(sb, "Work Order", project.ProjectWo);
            
            AddInfoRow(sb, "Estimated Completion", project.ProjectEndDate?.ToString("MMMM dd, yyyy") ?? "-");
            AddInfoRow(sb, "Quote Tracker Number", project.ProjectQuoteTracker);
            
            AddInfoRow(sb, "Client Requested Date", project.ProjectClientRequired?.ToString("MMMM dd, yyyy") ?? "-");
            AddInfoRow(sb, "Actual Price", project.ProjectActualPrice != null ? $"$ {project.ProjectActualPrice}" : "$ -");
            
            AddInfoRow(sb, "Client Completion Date", project.ProjectClientCompletion?.ToString("MMMM dd, yyyy") ?? "-");
            AddInfoRow(sb, "Indicative Price", project.ProjectIndicative != null ? $"$ {project.ProjectIndicative}" : "$ -");
            
            AddInfoRow(sb, "Tender Close Date", project.ProjectTendered?.ToString("MMMM dd, yyyy") ?? "-");
            AddInfoRow(sb, "Purchase Order", project.ProjectPurchaseOrder);
            
            AddInfoRow(sb, "Client Contact", project.ProjectContact);
            AddInfoRow(sb, "IRIS Contract Number", project.ProjectIrisNo);
            
            AddInfoRow(sb, "Contact Email", project.ProjectContactEmail);
            AddInfoRow(sb, "eTender Contract Number", project.ProjectEtenderNo);
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"description-box\">");
            sb.AppendLine($"<strong>Description:</strong> {project.ProjectDescription}");
            sb.AppendLine("        </div>");

            // Additional Details
            sb.AppendLine("        <div class=\"section-title\">Additional Details</div>");
            sb.AppendLine("        <div class=\"info-grid\" style=\"margin-top: 10px;\">");
            AddInfoRow(sb, "Consultant", project.ProjectConsultant);
            AddInfoRow(sb, "Client", project.ProjectClient);
            
            AddInfoRow(sb, "Contractor", project.ProjectContractor);
            AddInfoRow(sb, "One School Number", project.ProjectOneSchool);
            
            AddInfoRow(sb, "WIC Number", project.ProjectWicNo);
            AddInfoRow(sb, "Fee Proposal", project.ProjectFeeprop);
            
            AddInfoRow(sb, "Works Type", project.ProjectWorkstype);
            AddInfoRow(sb, "Req Order Number", project.ProjectReqOrder);
            
            AddInfoRow(sb, "Program Code", project.ProjectProgramCode);
            AddInfoRow(sb, "Estimators Start Date", project.ProjectStartDate?.ToString("MMMM dd, yyyy") ?? "-");
            
            AddInfoRow(sb, "Supervisor", project.ProjectSupervisor);
            AddInfoRow(sb, "Location", project.ProjectLocation);
            
            AddInfoRow(sb, "Senior Estimator", project.ProjectSeniorEstimator);
            AddInfoRow(sb, "Region", project.ProjectRegion);
            
            AddInfoRow(sb, "Componentisation", project.ProjectComponentisation);
            AddInfoRow(sb, "Folder Location", project.ProjectFolderPath);
            sb.AppendLine("        </div>");

            // Task List
            sb.AppendLine("        <h2>Task List</h2>");
            sb.AppendLine("        <table>");
            sb.AppendLine("            <thead>");
            sb.AppendLine("                <tr>");
            sb.AppendLine("                    <th style=\"width: 25%;\">Task</th>");
            sb.AppendLine("                    <th style=\"width: 15%;\">Status</th>");
            sb.AppendLine("                    <th style=\"width: 15%;\">Date Started</th>");
            sb.AppendLine("                    <th style=\"width: 10%;\">Days</th>");
            sb.AppendLine("                    <th style=\"width: 35%;\">Description</th>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("            </thead>");
            sb.AppendLine("            <tbody>");
            foreach (var task in tasks)
            {
                sb.AppendLine("                <tr>");
                sb.AppendLine($"                    <td>{task.Task}</td>");
                sb.AppendLine($"                    <td><span class=\"badge {GetStatusBadgeClass(task.Status)}\">{GetStatusText(task.Status)}</span></td>");
                sb.AppendLine($"                    <td>{task.DateCreated:dd/MM/yyyy}</td>");
                sb.AppendLine($"                    <td>{(task.EstimatedDays.HasValue ? task.EstimatedDays.Value.ToString() : "0")}</td>");
                sb.AppendLine($"                    <td>{task.Description}</td>");
                sb.AppendLine("                </tr>");
            }
            if (!tasks.Any())
            {
                sb.AppendLine("                <tr><td colspan=\"5\" style=\"text-align:center;\">No tasks found.</td></tr>");
            }
            sb.AppendLine("            </tbody>");
            sb.AppendLine("        </table>");

            // Productivity Comments
            sb.AppendLine("        <h2>Estimator Productivity / Comments</h2>");
            sb.AppendLine("        <table>");
            sb.AppendLine("                <thead>");
            sb.AppendLine("                    <tr>");
            sb.AppendLine("                        <th style=\"width: 15%;\">Date</th>");
            sb.AppendLine("                        <th style=\"width: 15%;\">User</th>");
            sb.AppendLine("                        <th style=\"width: 20%;\">Task</th>");
            sb.AppendLine("                        <th style=\"width: 40%;\">Comment</th>");
            sb.AppendLine("                        <th style=\"width: 10%;\">Hours</th>");
            sb.AppendLine("                    </tr>");
            sb.AppendLine("                </thead>");
            sb.AppendLine("                <tbody>");
            foreach (var entry in productivity.OrderByDescending(p => p.ProductivityDate))
            {
                string userName = entry.ProductivityUserId.HasValue && userNames.ContainsKey(entry.ProductivityUserId.Value) 
                    ? userNames[entry.ProductivityUserId.Value] 
                    : $"User {entry.ProductivityUserId}";
                
                string taskName = entry.ProductivityTaskId.HasValue && taskNames.ContainsKey(entry.ProductivityTaskId.Value)
                    ? taskNames[entry.ProductivityTaskId.Value]
                    : "Unknown Task";

                sb.AppendLine("                    <tr>");
                sb.AppendLine($"                        <td>{entry.ProductivityDate?.ToString("dd/MM/yyyy") ?? "N/A"}</td>");
                sb.AppendLine($"                        <td>{userName}</td>");
                sb.AppendLine($"                        <td>{taskName}</td>");
                sb.AppendLine($"                        <td>{entry.ProductivityComment}</td>");
                sb.AppendLine($"                        <td>{entry.ProductivityTimeRendered?.ToString("F2") ?? "0.00"}</td>");
                sb.AppendLine("                    </tr>");
            }
            if (!productivity.Any())
            {
                sb.AppendLine("                <tr><td colspan=\"5\" style=\"text-align:center;\">No productivity comments found.</td></tr>");
            }
            else
            {
                sb.AppendLine("                <tr style=\"font-weight:bold; background-color: #eee;\">");
                sb.AppendLine("                    <td colspan=\"4\" style=\"text-align:right;\">Total Hours:</td>");
                sb.AppendLine($"                    <td>{productivity.Sum(p => p.ProductivityTimeRendered ?? 0):F2}</td>");
                sb.AppendLine("                </tr>");
            }
            sb.AppendLine("                </tbody>");
            sb.AppendLine("            </table>");

            // ISO Forms / Checklists
            if (checklists != null && checklists.Any())
            {
                sb.AppendLine("        <h2>ISO Forms / Checklists</h2>");
                sb.AppendLine("        <table>");
                sb.AppendLine("            <thead>");
                sb.AppendLine("                <tr>");
                sb.AppendLine("                    <th style=\"width: 30%;\">Template Name</th>");
                sb.AppendLine("                    <th style=\"width: 15%;\">Status</th>");
                sb.AppendLine("                    <th style=\"width: 15%;\">Progress</th>");
                sb.AppendLine("                    <th style=\"width: 20%;\">Created</th>");
                sb.AppendLine("                    <th style=\"width: 20%;\">Updated</th>");
                sb.AppendLine("                </tr>");
                sb.AppendLine("            </thead>");
                sb.AppendLine("            <tbody>");
                foreach (var cl in checklists)
                {
                    string statusClass = cl.Status == "Completed" ? "badge-done" : "badge-inprogress";
                    string progress = cl.Status == "Completed" ? "100%" : "50%";
                    
                    sb.AppendLine("                <tr>");
                    sb.AppendLine($"                    <td>{cl.Template?.Name ?? "Unknown Template"}</td>");
                    sb.AppendLine($"                    <td><span class=\"badge {statusClass}\">{cl.Status}</span></td>");
                    sb.AppendLine($"                    <td>{progress}</td>");
                    sb.AppendLine($"                    <td>{cl.CreatedDate:dd/MM/yyyy}<br/><small style=\"color:#888\">by {cl.CreatedBy}</small></td>");
                    sb.AppendLine($"                    <td>{(cl.CompletedDate.HasValue ? cl.CompletedDate.Value.ToString("dd/MM/yyyy") : "-")}</td>");
                    sb.AppendLine("                </tr>");
                }
                sb.AppendLine("            </tbody>");
                sb.AppendLine("        </table>");
            }

            // Footer
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine($"            <p>&copy; {DateTime.Now.Year} Copelin System - Estimating Module</p>");
            sb.AppendLine("            <p class=\"no-print\"><button onclick=\"window.print()\" style=\"padding: 8px 20px; border-radius: 4px; border: 1px solid #004a99; background: white; color: #004a99; cursor: pointer;\">Print Report</button></p>");
            sb.AppendLine("        </div>");

            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private void AddInfoRow(StringBuilder sb, string label, string? value)
        {
            sb.AppendLine("            <div class=\"info-item\">");
            sb.AppendLine($"                <span class=\"info-label\">{label}</span>");
            sb.AppendLine($"                <span class=\"info-value\">{(string.IsNullOrWhiteSpace(value) ? "-" : value)}</span>");
            sb.AppendLine("            </div>");
        }

        public string GenerateChecklistHtml(ProjectChecklist checklist, ProjectList? project = null)
        {
            var sb = new StringBuilder();

            // Try to get base64 logo
            string logoBase64 = "";
            try
            {
                string logoPath = Path.Combine(_env.WebRootPath, "uploads", "logo", "Queensland-Government-Logo.png");
                if (File.Exists(logoPath))
                {
                    byte[] imageArray = File.ReadAllBytes(logoPath);
                    logoBase64 = Convert.ToBase64String(imageArray);
                }
            }
            catch { /* Ignore */ }

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"    <title>{checklist.Template?.Name ?? "Checklist"}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.5; color: #333; margin: 0; padding: 20px; background-color: #fff; }");
            sb.AppendLine("        .container { max-width: 900px; margin: auto; border: 1px solid #ddd; padding: 30px; }");
            sb.AppendLine("        .header { display: flex; justify-content: space-between; align-items: flex-start; border-bottom: 2px solid #004a99; padding-bottom: 20px; margin-bottom: 30px; }");
            sb.AppendLine("        .header-info h1 { margin: 0; color: #004a99; font-size: 24px; }");
            sb.AppendLine("        .header-info p { margin: 5px 0 0; color: #666; }");
            sb.AppendLine("        .logo { max-height: 60px; }");
            sb.AppendLine("        .section { margin-bottom: 30px; border: 1px solid #eee; border-radius: 4px; overflow: hidden; }");
            sb.AppendLine("        .section-header { background-color: #f8f9fa; padding: 10px 15px; border-bottom: 1px solid #eee; font-weight: bold; color: #004a99; }");
            sb.AppendLine("        .question-row { padding: 15px; border-bottom: 1px solid #eee; }");
            sb.AppendLine("        .question-row:last-child { border-bottom: none; }");
            sb.AppendLine("        .question-text { font-weight: 500; margin-bottom: 8px; }");
            sb.AppendLine("        .response-val { font-weight: bold; color: #222; }");
            sb.AppendLine("        .notes { font-style: italic; color: #666; font-size: 0.9em; margin-top: 5px; }");
            sb.AppendLine("        .badge { display: inline-block; padding: 2px 8px; border-radius: 3px; font-size: 0.85em; background: #eee; }");
            sb.AppendLine("        .badge-completed { background: #d4edda; color: #155724; }");
            sb.AppendLine("        .badge-inprogress { background: #fff3cd; color: #856404; }");
            sb.AppendLine("        .meta-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px; margin-bottom: 30px; background: #f9f9f9; padding: 15px; border-radius: 4px; }");
            sb.AppendLine("        .project-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; margin-bottom: 30px; border: 1px solid #eee; padding: 15px; border-radius: 4px; }");
            sb.AppendLine("        .project-item { display: flex; flex-direction: column; }");
            sb.AppendLine("        .project-label { font-size: 0.85em; color: #666; font-weight: bold; text-transform: uppercase; }");
            sb.AppendLine("        .project-value { font-size: 1em; color: #222; font-weight: 500; }");
            sb.AppendLine("        h2 { color: #004a99; font-size: 1.2em; border-bottom: 1px solid #eee; padding-bottom: 5px; margin-top: 0; }");
            sb.AppendLine("        @media print { body { padding: 0; } .container { border: none; padding: 0; } }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");

            // Header
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <div class=\"header-info\">");
            sb.AppendLine($"                <h1>{checklist.Template?.Name ?? "Checklist Checklist"}</h1>");
            sb.AppendLine($"                <p>{checklist.Template?.Description}</p>");
            sb.AppendLine("            </div>");
            if (!string.IsNullOrEmpty(logoBase64))
            {
                sb.AppendLine($"            <img src=\"data:image/png;base64,{logoBase64}\" alt=\"Logo\" class=\"logo\">");
            }
            sb.AppendLine("        </div>");

            // Project Details (if available)
            if (project != null)
            {
                sb.AppendLine("        <div class=\"project-grid\">");
                sb.AppendLine($"            <div style=\"grid-column: span 2; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px;\">");
                sb.AppendLine($"                <strong style=\"font-size: 1.1em; color: #004a99;\">{project.ProjectLocation} - {project.ProjectName}</strong>");
                sb.AppendLine("            </div>");

                sb.AppendLine("            <div class=\"project-item\">");
                sb.AppendLine("                <span class=\"project-label\">WR Number</span>");
                sb.AppendLine($"                <span class=\"project-value\">{project.ProjectWr}</span>");
                sb.AppendLine("            </div>");

                sb.AppendLine("            <div class=\"project-item\">");
                sb.AppendLine("                <span class=\"project-label\">Work Order</span>");
                sb.AppendLine($"                <span class=\"project-value\">{project.ProjectWo ?? "-"}</span>");
                sb.AppendLine("            </div>");

                sb.AppendLine("            <div class=\"project-item\">");
                sb.AppendLine("                <span class=\"project-label\">Client</span>");
                sb.AppendLine($"                <span class=\"project-value\">{project.ProjectClient ?? "-"}</span>");
                sb.AppendLine("            </div>");
                
                sb.AppendLine("            <div class=\"project-item\">");
                sb.AppendLine("                <span class=\"project-label\">Client Contact</span>");
                sb.AppendLine($"                <span class=\"project-value\">{project.ProjectContact ?? "-"}</span>");
                sb.AppendLine("            </div>");

                if (project.ProjectEndDate.HasValue)
                {
                    sb.AppendLine("            <div class=\"project-item\">");
                    sb.AppendLine("                <span class=\"project-label\">Estimated Completion</span>");
                    sb.AppendLine($"                <span class=\"project-value\">{project.ProjectEndDate.Value:dd/MM/yyyy}</span>");
                    sb.AppendLine("            </div>");
                }
                
                sb.AppendLine("            <div class=\"project-item\">");
                sb.AppendLine("                <span class=\"project-label\">Senior Estimator</span>");
                sb.AppendLine($"                <span class=\"project-value\">{project.ProjectSeniorEstimator ?? "-"}</span>");
                sb.AppendLine("            </div>");
                
                sb.AppendLine("        </div>");
            }

            // Metadata
            string statusClass = checklist.Status == "Completed" ? "badge-completed" : "badge-inprogress";
            sb.AppendLine("        <div class=\"meta-grid\">");
            sb.AppendLine($"            <div><strong>Status:</strong> <span class=\"badge {statusClass}\">{checklist.Status}</span></div>");
            sb.AppendLine($"            <div><strong>Created:</strong> {checklist.CreatedDate:dd/MM/yyyy} by {checklist.CreatedBy}</div>");
            sb.AppendLine($"            <div><strong>Completed:</strong> {(checklist.CompletedDate?.ToString("dd/MM/yyyy") ?? "-")} by {(checklist.CompletedBy ?? "-")}</div>");
            sb.AppendLine($"            <div><strong>Reference ID:</strong> #{checklist.ChecklistInstanceId}</div>");
            sb.AppendLine("        </div>");

            // Sections
            if (checklist.Template?.Sections != null)
            {
                foreach (var section in checklist.Template.Sections.OrderBy(s => s.DisplayOrder))
                {
                    sb.AppendLine("        <div class=\"section\">");
                    sb.AppendLine($"            <div class=\"section-header\">{section.SectionName}</div>");
                    
                    if (section.Questions != null)
                    {
                        foreach (var question in section.Questions.OrderBy(q => q.DisplayOrder))
                        {
                            var response = checklist.Responses.FirstOrDefault(r => r.QuestionId == question.QuestionId);
                            string val = response?.ResponseValue ?? "-";
                            
                            // Format value based on type if needed
                            if (question.QuestionType == "UnixTime" || question.QuestionType == "Date")
                            {
                                if(DateTime.TryParse(val, out var d)) val = d.ToString("dd/MM/yyyy");
                            }
                            else if(question.QuestionType == "Checkbox") {
                                val = (val == "true") ? "Confirmed" : "Not Confirmed";
                            }
                            else if (question.QuestionType == "MultiSelect") {
                                try {
                                    if(val.StartsWith("[")) {
                                        var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(val);
                                        val = list != null ? string.Join(", ", list) : val;
                                    }
                                } catch {}
                            }

                            sb.AppendLine("            <div class=\"question-row\">");
                            sb.AppendLine($"                <div class=\"question-text\">{question.QuestionText}</div>");
                            sb.AppendLine($"                <div class=\"response-val\">{val}</div>");
                            if (!string.IsNullOrWhiteSpace(response?.Notes))
                            {
                                sb.AppendLine($"                <div class=\"notes\"><strong>Note:</strong> {response.Notes}</div>");
                            }
                            sb.AppendLine("            </div>");
                        }
                    }
                    sb.AppendLine("        </div>");
                }
            }

            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GetStatusText(byte? status)
        {
            return status switch
            {
                0 => "Created",
                1 => "In Progress",
                2 => "On Hold",
                3 => "Review",
                4 => "Cancelled",
                5 => "Done",
                _ => "Unknown"
            };
        }

        private string GetStatusBadgeClass(byte? status)
        {
            return status switch
            {
                0 => "badge-pending",
                1 => "badge-inprogress",
                3 => "badge-done",
                _ => "badge-pending"
            };
        }
    }
}
