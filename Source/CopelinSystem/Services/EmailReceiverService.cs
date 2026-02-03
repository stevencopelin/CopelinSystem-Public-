using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CopelinSystem.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CopelinSystem.Services
{
    public class EmailReceiverService : BackgroundService
    {
        private readonly ILogger<EmailReceiverService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _pollingIntervalMinutes;
        
        // Fallback storage if DB/Project lookup fails completely
        private readonly string _fallbackStoragePath;

        public EmailReceiverService(
            ILogger<EmailReceiverService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            // Read configuration
            _pollingIntervalMinutes = int.Parse(_configuration["EmailSettings:PollingIntervalMinutes"] ?? "5");
            
            // "Unassigned" default location if all else fails
            var storageRoot = _configuration["FileStorage:RootPath"] ?? @"\\srv2025\Pool2\Qbuild";
            _fallbackStoragePath = Path.Combine(storageRoot, "Unassigned", "Emails");

            // Ensure fallback directory exists
            if (!Directory.Exists(_fallbackStoragePath))
            {
                Directory.CreateDirectory(_fallbackStoragePath);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Receiver Service started. Polling interval: {Interval} minutes", _pollingIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmails(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing emails");
                }

                // Wait for the configured interval before next poll
                await Task.Delay(TimeSpan.FromMinutes(_pollingIntervalMinutes), stoppingToken);
            }

            _logger.LogInformation("Email Receiver Service stopped");
        }

        private async Task ProcessEmails(CancellationToken stoppingToken)
        {
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"] ?? "993");
            var useSsl = bool.Parse(_configuration["EmailSettings:UseSsl"] ?? "true");
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Email settings not configured. Skipping email processing.");
                return;
            }

            using var client = new ImapClient();

            try
            {
                // Connect to the IMAP server
                await client.ConnectAsync(host, port, useSsl, stoppingToken);
                await client.AuthenticateAsync(username, password, stoppingToken);

                _logger.LogInformation("Connected to email server: {Host}", host);

                // Open the INBOX folder
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);

                // Search for unread messages
                var uids = await inbox.SearchAsync(SearchQuery.NotSeen, stoppingToken);

                _logger.LogInformation("Found {Count} unread emails", uids.Count);

                foreach (var uid in uids)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var message = await inbox.GetMessageAsync(uid, stoppingToken);
                        await ProcessMessage(message, stoppingToken);

                        // Mark as read
                        await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email UID {Uid}", uid);
                    }
                }

                await client.DisconnectAsync(true, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to email server");
            }
        }

        private async Task ProcessMessage(MimeMessage message, CancellationToken stoppingToken)
        {
            try
            {
                var subject = message.Subject ?? "(No Subject)";
                var from = message.From.Mailboxes.FirstOrDefault()?.Address ?? "unknown";
                var to = message.To.Mailboxes.FirstOrDefault()?.Address;
                var receivedDate = message.Date.DateTime;
                var bodyText = message.TextBody;
                var bodyHtml = message.HtmlBody;

                // Create scope to resolve services
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                var fileSystemService = scope.ServiceProvider.GetRequiredService<FileSystemService>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Try to match project immediately
                var wrNumber = emailService.ExtractWrNumber(subject);
                if (wrNumber == null && !string.IsNullOrWhiteSpace(bodyText))
                {
                    wrNumber = emailService.ExtractWrNumber(bodyText);
                }

                ProjectList? project = null;
                if (wrNumber != null)
                {
                    project = await emailService.MatchProjectByWr(wrNumber);
                }

                // 2. Prepare storage location
                string targetFolder;
                int? projectId = null;
                int? emailAttachmentsFolderId = null;

                if (project != null)
                {
                    projectId = project.ProjectId;
                    // Get 'Email-Attachments' folder for this project
                    // Note: This relies on CreateDefaultFolders having run, or we create it now.
                    var folder = await fileSystemService.GetFolderByName(project.ProjectId, "Email-Attachments");
                    if (folder == null)
                    {
                         await fileSystemService.CreateFolder(project.ProjectId, null, "Email-Attachments", "System");
                         folder = await fileSystemService.GetFolderByName(project.ProjectId, "Email-Attachments");
                    }
                    
                    if (folder != null)
                    {
                        emailAttachmentsFolderId = folder.Id;
                        targetFolder = fileSystemService.GetAbsolutePath(folder.PhysicalPath);
                    }
                    else
                    {
                        // Fallback if DB insert failed silently or something
                         targetFolder = await fileSystemService.GetPhysicalPath(context, project.ProjectId, null); // Root of project
                    }
                }
                else
                {
                    // Unassigned
                    // Structure: \\srv...\Unassigned\Emails\{GUID_or_TempID}\
                    // We will use a temp ID for now, and update later or keep it there.
                    // Actually, let's just dump in Unassigned root for now or date-based folder?
                    // Let's use Year-Month for unassigned to avoid millions of files in one dir
                    var unassignedSub = Path.Combine(_fallbackStoragePath, DateTime.Now.ToString("yyyy-MM"));
                    if (!Directory.Exists(unassignedSub)) Directory.CreateDirectory(unassignedSub);
                    targetFolder = unassignedSub;
                }

                // 3. Process & Save Attachments
                var attachments = new List<ProjectEmailAttachment>();

                foreach (var attachment in message.Attachments)
                {
                    if (attachment is MimePart mimePart)
                    {
                        var fileName = mimePart.FileName ?? $"attachment_{Guid.NewGuid()}";
                        var fileSize = mimePart.Content?.Stream.Length ?? 0;
                        var contentType = mimePart.ContentType?.MimeType;

                        // Ensure filename uniqueness in target
                        var safeName = SanitizeFileName(fileName);
                        var fullPath = Path.Combine(targetFolder, safeName);
                        int count = 1;
                        while (File.Exists(fullPath))
                        {
                            var nameNoExt = Path.GetFileNameWithoutExtension(safeName);
                            var ext = Path.GetExtension(safeName);
                            fullPath = Path.Combine(targetFolder, $"{nameNoExt} ({count++}){ext}");
                        }
                        
                        var finalFileName = Path.GetFileName(fullPath);

                        // Save to disk
                        using (var stream = File.Create(fullPath))
                        {
                            if (mimePart.Content != null)
                            {
                                await mimePart.Content.DecodeToAsync(stream, stoppingToken);
                            }
                        }

                        // Register in FileSystem if matched
                        if (projectId.HasValue && emailAttachmentsFolderId.HasValue)
                        {
                            try 
                            { 
                                await fileSystemService.RegisterFile(projectId.Value, emailAttachmentsFolderId, fullPath, "System (Email)");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to register email attachment in file system: {File}", fullPath);
                            }
                        }

                        attachments.Add(new ProjectEmailAttachment
                        {
                            FileName = finalFileName,
                            FilePath = fullPath,
                            FileSize = fileSize,
                            ContentType = contentType
                        });

                        _logger.LogInformation("Saved attachment: {FileName} ({Size} bytes) to {Path}", finalFileName, fileSize, fullPath);
                    }
                }

                // 4. Save Email to DB
                await emailService.SaveEmail(
                    subject,
                    from,
                    to,
                    bodyText,
                    bodyHtml,
                    receivedDate,
                    attachments);
                    
                // Note: SaveEmail handles setting the ProjectId on the Email entity if WR matches
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                throw;
            }
        }
        
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());
        }
    }
}
