

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Src.Services.Email.Interfaces;
using Src.Services.Email.Models;

namespace Src.Services.Email.Implementations
{
    public class MailKitEmailService: IEmailService
    {
        private ILogger<MailKitEmailService> _logger;
        private readonly EmailSettings _settings;

        private MimeMessage _message;
        private BodyBuilder _bodyBuilder;

        public MailKitEmailService(IOptions<EmailSettings> settings, ILogger<MailKitEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            InitializeMessage();
        }

        /// <summary>
        /// Initializes the email message and prepares the message body for composition.
        /// </summary>
        /// <remarks>This method sets the sender information using the configured sender name and email
        /// address. It should be called before adding recipients or composing the message body to ensure the message is
        /// properly initialized.</remarks>
        private void InitializeMessage()
        {
            _message = new MimeMessage();

            _message.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            _bodyBuilder = new BodyBuilder();
        }

        public IEmailService SetSubject(string subject)
        {
            _message.Subject = subject;
            return this;
        }

        public IEmailService SetBody(string body, bool isHtml = true)
        {
            if (isHtml)
                _bodyBuilder.HtmlBody = body;
            else
                _bodyBuilder.TextBody = body;

            return this;
        }
        
        public IEmailService AddTo(string email, string? name = null)
        {
            _message.To.Add(
                new MailboxAddress(name ?? email, email)
                );
            return this;
        }

        
        public IEmailService AddCc(string email, string? name = null)
        {
            _message.Cc.Add(
                new MailboxAddress(
                    name ?? email,
                    email)
                );

            return this;
        }

        
        public IEmailService AddMultipleCc(IEnumerable<(string Email, string? Name)> ccRecipients)
        {
            foreach (var (email, name) in ccRecipients)
            {
                AddCc(email, name);
            }
            return this;
        }
    
        public IEmailService AddBcc(string email, string? name = null)
        {
            _message.Bcc.Add(
                new MailboxAddress(
                    name ?? email,
                    email)
                );
            return this;
        }
        
        public IEmailService AddMultipleBcc(IEnumerable<(string Email, string? Name)> bccRecipients)
        {
            foreach (var (email, name) in bccRecipients)
            {
                AddBcc(email, name);
            }
            return this;
        }
        
        public IEmailService AddAttachment(string filePath)
        {
            _bodyBuilder.Attachments.Add(filePath);

            return this;
        }

        public IEmailService AddAttachments(IEnumerable<string> filePaths)
        {
            foreach (var path in filePaths)
            {
                AddAttachment(path);
            }

            return this;
        }

        
        public async Task<EmailSendResult> SendEmailAsync(CancellationToken cancellationToken = default)
        {
            var validationErrors = ValidateEmail();

            if (validationErrors.Any())
            {
                return EmailSendResult.Fail(
                    "Email validation failed",
                    validationErrors);
            }

            try
            {
                _message.Body = _bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();

                smtp.Timeout = 10000;

                await smtp.ConnectAsync(
                    _settings.SmtpServer,
                    _settings.Port,
                    _settings.UseSsl
                        ? SecureSocketOptions.SslOnConnect
                        : SecureSocketOptions.StartTls,
                    cancellationToken
                );

                if (!string.IsNullOrWhiteSpace(_settings.Username))
                {
                    await smtp.AuthenticateAsync(
                        _settings.Username,
                        _settings.Password,
                        cancellationToken);
                }

                await smtp.SendAsync(
                    _message,
                    cancellationToken);

                await smtp.DisconnectAsync(
                    true,
                    cancellationToken);

                Reset();

                _logger.LogInformation(
                    "Email sent successfully to: {Recipients}",
                    string.Join(", ", _message.To.Select(r => r.ToString())));

                return EmailSendResult.Ok();
            }
            catch (OperationCanceledException)
            {
                string cancellationInfo = "Email sending was cancelled at UTC " + DateTime.UtcNow + " and local time " + DateTime.Now;

                _logger.LogWarning(cancellationInfo);

                return EmailSendResult.Fail(cancellationInfo);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(
                    ex,
                    "SMTP command error while sending email to: {Recipients}",
                    string.Join(", ", _message.To.Select(r => r.ToString()))
                );
                return EmailSendResult.Fail(
                    "SMTP command failed",
                    new List<string>
                    {
                        ex.Message,
                        $"StatusCode: {ex.StatusCode}"
                    });
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(
                    ex,
                    "SMTP protocol error while sending email to: {Recipients}",
                    string.Join(", ", _message.To.Select(r => r.ToString()))
                );
                return EmailSendResult.Fail(
                    "SMTP protocol error",
                    new List<string>
                    {
                        ex.Message
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while sending email to: {Recipients}",
                    string.Join(", ", _message.To.Select(r => r.ToString()))
                );

                return EmailSendResult.Fail(
                    "Unexpected error while sending email",
                    new List<string>
                    {
                        ex.Message
                    }
                );
            }
        }

        /// <summary>
        /// Resets the state of the object to its initial configuration.
        /// </summary>
        /// <remarks>Call this method to clear any changes and restore the object to its default state.
        /// This is useful when reusing the same instance for multiple operations.</remarks>
        public void Reset()
        {
            InitializeMessage();
        }

        /// <summary>
        /// Validates the email message and returns a list of error messages for any missing required fields.
        /// </summary>
        /// <remarks>The validation checks for at least one recipient (To, CC, or BCC), a non-empty
        /// subject, and a non-empty body (either HTML or text).</remarks>
        /// <returns>A list of strings containing validation error messages. The list is empty if the email message is valid.</returns>
        private List<string> ValidateEmail()
        {
            var errors = new List<string>();

            if (!_message.To.Any() &&
                !_message.Cc.Any() &&
                !_message.Bcc.Any())
            {
                errors.Add("At least one recipient (To, CC, or BCC) is required.");
            }

            if (string.IsNullOrWhiteSpace(
                _message.Subject))
            {
                errors.Add("Email subject is required.");
            }

            if (string.IsNullOrWhiteSpace(
                _bodyBuilder.HtmlBody) &&
                string.IsNullOrWhiteSpace(
                _bodyBuilder.TextBody))
            {
                errors.Add("Email body is required.");
            }

            return errors;
        }
    }
}
