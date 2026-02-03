using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CopelinSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp; // For SmtpClient conflict resolution if needed, though fully qualified in code

namespace CopelinSystem.Services
{
    public class EmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly FileSystemService _fileSystemService;

        public EmailService(ApplicationDbContext context, ILogger<EmailService> logger, IConfiguration configuration, FileSystemService fileSystemService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _fileSystemService = fileSystemService;
        }

        public async Task<(bool Success, string Error)> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var host = "smtp.office365.com"; // Hardcoded or from config if available (appsettings usually has IMAP host)
                var port = 587;
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];

                if (string.IsNullOrEmpty(password))
                {
                    return (false, "Email password is not configured.");
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Copelin System", username));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Sent email to {To} with subject {Subject}", to, subject);
                return (true, string.Empty);
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Authentication failed for {To}", to);
                return (false, $"Authentication failed: {authEx.Message}. Check your password, or use an App Password if MFA is enabled. Ensure SMTP AUTH is active.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Extracts a 6-8 digit WR number from text (subject or body)
        /// Pattern matches: 123456, 1234567, 12345678, or numbers preceded by "WR", "wr", etc.
        /// </summary>
        public string? ExtractWrNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // Pattern 1: Look for "WR" followed by optional separator and 6-8 digits
            // Examples: WR-909090, WR-1234567, WR 1234567, wr1234567, WR:1234567
            var wrPattern = @"(?:WR|wr)[\s\-:]*(\d{6,8})\b";
            var wrMatch = Regex.Match(text, wrPattern);
            
            if (wrMatch.Success)
            {
                return wrMatch.Groups[1].Value;
            }

            // Pattern 2: Look for standalone 6-8 digit numbers
            // This is more permissive and might catch false positives
            var standalonePattern = @"\b(\d{6,8})\b";
            var standaloneMatch = Regex.Match(text, standalonePattern);
            
            if (standaloneMatch.Success)
            {
                return standaloneMatch.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// Attempts to match a project by WR number
        /// </summary>
        public async Task<ProjectList?> MatchProjectByWr(string wrNumber)
        {
            if (string.IsNullOrWhiteSpace(wrNumber))
                return null;

            try
            {
                // Try exact match first
                var project = await _context.ProjectLists
                    .FirstOrDefaultAsync(p => p.ProjectWr == wrNumber);

                if (project != null)
                    return project;

                // Try with leading zeros removed (in case WR is stored differently)
                var trimmedWr = wrNumber.TrimStart('0');
                if (trimmedWr != wrNumber)
                {
                    project = await _context.ProjectLists
                        .FirstOrDefaultAsync(p => p.ProjectWr == trimmedWr);
                }

                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching project by WR number: {WrNumber}", wrNumber);
                return null;
            }
        }

        /// <summary>
        /// Saves an email and its attachments to the database
        /// </summary>
        public async Task<ProjectEmail> SaveEmail(
            string subject,
            string from,
            string? to,
            string? bodyText,
            string? bodyHtml,
            DateTime receivedDate,
            List<ProjectEmailAttachment>? attachments = null)
        {
            try
            {
                // Extract WR number from subject first, then body
                var wrNumber = ExtractWrNumber(subject);
                if (wrNumber == null && !string.IsNullOrWhiteSpace(bodyText))
                {
                    wrNumber = ExtractWrNumber(bodyText);
                }

                // Try to match project
                ProjectList? project = null;
                if (wrNumber != null)
                {
                    project = await MatchProjectByWr(wrNumber);
                }

                var email = new ProjectEmail
                {
                    ProjectId = project?.ProjectId,
                    ProjectWr = wrNumber,
                    EmailSubject = subject,
                    EmailFrom = from,
                    EmailTo = to,
                    EmailBody = bodyText,
                    EmailBodyHtml = bodyHtml,
                    ReceivedDate = receivedDate,
                    ProcessedDate = DateTime.Now,
                    IsMatched = project != null,
                    CreatedDate = DateTime.Now
                };

                _context.ProjectEmails.Add(email);
                await _context.SaveChangesAsync();

                // Save attachments if any
                if (attachments != null && attachments.Any())
                {
                    foreach (var attachment in attachments)
                    {
                        attachment.EmailId = email.EmailId;
                        _context.ProjectEmailAttachments.Add(attachment);
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation(
                    "Saved email {EmailId} from {From} with subject '{Subject}'. WR: {WrNumber}, Matched: {IsMatched}",
                    email.EmailId, from, subject, wrNumber ?? "None", email.IsMatched);

                return email;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email from {From} with subject '{Subject}'", from, subject);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all emails for a specific project
        /// </summary>
        public async Task<List<ProjectEmail>> GetProjectEmails(int projectId)
        {
            try
            {
                return await _context.ProjectEmails
                    .Where(e => e.ProjectId == projectId)
                    .OrderByDescending(e => e.ReceivedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emails for project {ProjectId}", projectId);
                return new List<ProjectEmail>();
            }
        }

        /// <summary>
        /// Retrieves the count of emails for a specific project
        /// </summary>
        public async Task<int> GetProjectEmailCount(int projectId)
        {
            try
            {
                return await _context.ProjectEmails
                    .Where(e => e.ProjectId == projectId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email count for project {ProjectId}", projectId);
                return 0;
            }
        }

        /// <summary>
        /// Retrieves all attachments for a specific email
        /// </summary>
        public async Task<List<ProjectEmailAttachment>> GetEmailAttachments(int emailId)
        {
            try
            {
                return await _context.ProjectEmailAttachments
                    .Where(a => a.EmailId == emailId)
                    .OrderBy(a => a.FileName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for email {EmailId}", emailId);
                return new List<ProjectEmailAttachment>();
            }
        }

        /// <summary>
        /// Retrieves a single email by ID
        /// </summary>
        public async Task<ProjectEmail?> GetEmailById(int emailId)
        {
            try
            {
                return await _context.ProjectEmails
                    .Include(e => e.Project)
                    .FirstOrDefaultAsync(e => e.EmailId == emailId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email {EmailId}", emailId);
                return null;
            }
        }

        /// <summary>
        /// Gets all unmatched emails (no project association)
        /// </summary>
        public async Task<List<ProjectEmail>> GetUnmatchedEmails()
        {
            try
            {
                return await _context.ProjectEmails
                    .Where(e => !e.IsMatched || e.ProjectId == null)
                    .OrderByDescending(e => e.ReceivedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unmatched emails");
                return new List<ProjectEmail>();
            }
        }

        /// <summary>
        /// Manually assigns an email to a project
        /// </summary>
        public async Task<bool> AssignEmailToProject(int emailId, int projectId)
        {
            try
            {
                var email = await _context.ProjectEmails
                    .Include(e => e.Attachments)
                    .FirstOrDefaultAsync(e => e.EmailId == emailId);

                if (email == null)
                    return false;

                var project = await _context.ProjectLists.FindAsync(projectId);
                if (project == null)
                    return false;

                email.ProjectId = projectId;
                email.ProjectWr = project.ProjectWr;
                email.IsMatched = true;
                email.ProcessedDate = DateTime.Now;

                // Move attachments if any
                if (email.Attachments != null && email.Attachments.Any())
                {
                    // 1. Get/Create Target Folder
                    var folder = await _fileSystemService.GetFolderByName(projectId, "Email-Attachments");
                    if (folder == null)
                    {
                         await _fileSystemService.CreateFolder(projectId, null, "Email-Attachments", "System");
                         folder = await _fileSystemService.GetFolderByName(projectId, "Email-Attachments");
                    }

                    if (folder != null)
                    {
                        var targetPath = _fileSystemService.GetAbsolutePath(folder.PhysicalPath);

                        foreach (var diff in email.Attachments)
                        {
                            if (File.Exists(diff.FilePath))
                            {
                                var fileName = Path.GetFileName(diff.FilePath);
                                var newPath = Path.Combine(targetPath, fileName);
                                
                                // Ensure unique name
                                int count = 1;
                                while (File.Exists(newPath))
                                {
                                    var nameNoExt = Path.GetFileNameWithoutExtension(fileName);
                                    var ext = Path.GetExtension(fileName);
                                    newPath = Path.Combine(targetPath, $"{nameNoExt} ({count++}){ext}");
                                }

                                // Move File
                                File.Move(diff.FilePath, newPath);

                                // Register in File System
                                await _fileSystemService.RegisterFile(projectId, folder.Id, newPath, "System (Email Assigned)");

                                // Update Attachment Record
                                diff.FilePath = newPath;
                                diff.FileName = Path.GetFileName(newPath); // In case of rename
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Manually assigned email {EmailId} to project {ProjectId}", emailId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning email {EmailId} to project {ProjectId}", emailId, projectId);
                return false;
            }
        }

        /// <summary>
        /// Deletes an email and its attachments
        /// </summary>
        public async Task<bool> DeleteEmail(int emailId)
        {
            try
            {
                var email = await _context.ProjectEmails.FindAsync(emailId);
                if (email == null)
                    return false;

                _context.ProjectEmails.Remove(email);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted email {EmailId}", emailId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting email {EmailId}", emailId);
                return false;
            }
        }
    }
}
