using Microsoft.AspNetCore.Mvc;
using Src.Services.Email.Interfaces;
using Src.Services.Email.Models;
using Src.Services.Progress.Interfaces;

namespace SendingEmail.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IJobProgressService _progress;

        private string jobId = "personal_job_id";
        public EmailController(IEmailService emailService, IJobProgressService progress)
        {
            _emailService = emailService;
            _progress = progress;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Send(CancellationToken cancellationToken)
        {

            List<(string Email, string? Name)> cc = new List<(string Email, string? Name)>();
            cc.Add(new("email1@gmail.com", "cc1"));
            cc.Add(new("email2@gmail.com", "cc2"));

            string firstFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", "1st.txt");
            string secondFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", "2nd.txt");

            List<string> attachments = new List<string>
            {
                firstFile,
                secondFile
            };

            string emailBody = $"This is a test email contain {attachments.Count} attachments.";
            EmailSendResult result = await _emailService.AddTo("sender@gmail.com", "Recipient Name")
                .AddMultipleCc(cc)
                .AddCc("cc1@gmail.com", "cc3")
                .SetSubject("Test Email")
                .SetBody(emailBody)
                .AddAttachments(attachments)
                .AddAttachment(secondFile)
                .UseSmtpProvider("gmail")
                .SendEmailAsync(cancellationToken);

            return Ok(result);
        }


        public async Task<IActionResult> StartSending(CancellationToken cancellationToken)
        {
            List<EmailSendResult> results = new List<EmailSendResult>();
            List<Task> tasks = new List<Task>();

            string sendTo = "sender@gmail.com";
            string recipientName = "Bernadus Widaryanto";
            string emailBody = "This is the Email content";

            var semaphore = new SemaphoreSlim(2); // Limit to 2 concurrent sends

            int nTest = 5;

            _progress.Start(nTest, jobId);
            for (int i = 1; i <= nTest; i++)
            {
                int emailIndex = i; // Capture the value to avoid closure issues
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var result = await _emailService.AddTo(sendTo, recipientName)
                            .SetSubject($"Test Email {emailIndex}")
                            .SetBody(emailBody)
                            .SendEmailAsync(cancellationToken);
                        lock (results)
                        {
                            results.Add(result);
                        }
                        if(result.Success)
                        {
                            _progress.ReportSuccess();
                        }
                        else
                        {
                            _progress.ReportFailed();
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (results)
                         {
                            results.Add(EmailSendResult.Fail($"Failed to send email {emailIndex} {sendTo}: {ex.Message}"));
                        }
                        _progress.ReportFailed();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            _progress.MarkCompleted();

            return Ok(results);
        }

        public IActionResult EmailProgressNotification()
        {
            var progress = _progress.GetProgress(jobId);
            return Ok(progress);
        }
    }
}
