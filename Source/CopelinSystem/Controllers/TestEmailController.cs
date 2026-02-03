using Microsoft.AspNetCore.Mvc;
using CopelinSystem.Models;
using CopelinSystem.Services;

namespace CopelinSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(EmailService emailService, ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to manually submit an email for processing
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Received test email from {From} with subject '{Subject}'", 
                    request.From, request.Subject);

                var attachments = new List<ProjectEmailAttachment>();
                
                // Process any test attachments
                if (request.Attachments != null && request.Attachments.Any())
                {
                    foreach (var att in request.Attachments)
                    {
                        attachments.Add(new ProjectEmailAttachment
                        {
                            FileName = att.FileName,
                            FilePath = $"test/{att.FileName}",
                            FileSize = att.FileSize,
                            ContentType = att.ContentType
                        });
                    }
                }

                var savedEmail = await _emailService.SaveEmail(
                    request.Subject,
                    request.From,
                    request.To,
                    request.Body,
                    request.BodyHtml,
                    request.ReceivedDate ?? DateTime.Now,
                    attachments);

                return Ok(new
                {
                    success = true,
                    emailId = savedEmail.EmailId,
                    projectId = savedEmail.ProjectId,
                    wrNumber = savedEmail.ProjectWr,
                    isMatched = savedEmail.IsMatched,
                    message = savedEmail.IsMatched 
                        ? $"Email saved and matched to project (WR: {savedEmail.ProjectWr})" 
                        : "Email saved but no matching project found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing test email");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }

    public class TestEmailRequest
    {
        public string Subject { get; set; } = "";
        public string From { get; set; } = "";
        public string? To { get; set; }
        public string? Body { get; set; }
        public string? BodyHtml { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public List<TestAttachment>? Attachments { get; set; }
    }

    public class TestAttachment
    {
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
    }
}
