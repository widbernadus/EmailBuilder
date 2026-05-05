using Src.Services.Email.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Src.Services.Email.Interfaces
{
    /// <summary>
    /// Provides an implementation of the IEmailService interface for composing and sending emails using MailKit and
    /// SMTP settings.
    /// </summary>
    /// <remarks>This service supports fluent composition of email messages, including setting the subject,
    /// body (HTML or plain text), recipients (To, CC, BCC), and file attachments. It uses the configured SMTP server
    /// and credentials to send messages asynchronously. The service is designed for reuse; after sending an email, the
    /// state is reset for composing a new message. Logging is integrated for tracking email operations and errors.
    /// Thread safety is not guaranteed; use a separate instance per concurrent operation.</remarks>
    public interface IEmailService
    {
        /// <summary>
        /// Sets the subject line for the email message.
        /// </summary>
        /// <param name="subject">The subject text to assign to the email. Cannot be null.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService SetSubject(string subject);

        /// <summary>
        /// Sets the body content of the email message, specifying whether the content is HTML or plain text.
        /// </summary>
        /// <param name="body">The content to use as the body of the email message.</param>
        /// <param name="isHtml">true to set the body as HTML; false to set it as plain text. The default is true.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService SetBody(
            string body,
            bool isHtml = true);

        // =========================
        // TO
        // =========================
        /// <summary>
        /// Sending email to a recipient. You can call this method multiple times to add multiple recipients.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IEmailService AddTo(
            string email,
            string? name = null);

        /// <summary>
        /// Adds a carbon copy (CC) recipient to the email message.
        /// </summary>
        /// <param name="email">The email address of the recipient to add as a CC. Cannot be null or empty.</param>
        /// <param name="name">The display name of the CC recipient. If null, the email address is used as the display name.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService AddCc(
            string email,
            string? name = null);

        /// <summary>
        /// Adds multiple carbon copy (CC) recipients to the email message.
        /// </summary>
        /// <remarks>Each recipient in the collection is added as a separate CC entry. If a recipient's
        /// display name is not provided, only the email address is used.</remarks>
        /// <param name="ccRecipients">A collection of tuples, each containing the email address and optional display name of a CC recipient to
        /// add. The email address must not be null or empty. The example of multiple recipients is: new List&lt;(string, string?)&gt; { ("email1@example.com", "Name1"), ("email2@example.com", "Name2") }.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService AddMultipleCc(
            IEnumerable<(string Email, string? Name)> ccRecipients);

        /// <summary>
        /// Adds a blind carbon copy (BCC) recipient to the email message.
        /// </summary>
        /// <param name="email">The email address of the recipient to add as a BCC. Cannot be null or empty.</param>
        /// <param name="name">The display name of the recipient. If null, the email address is used as the display name.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService AddBcc(
            string email,
            string? name = null);

        /// <summary>
        /// Adds multiple recipients to the blind carbon copy (BCC) list for the email message.
        /// </summary>
        /// <param name="bccRecipients">A collection of tuples, each containing the email address and optional display name of a recipient to add to
        /// the BCC list. The email address must not be null or empty. The example of multiple recipients is: new List&lt;(string, string?)&gt; { ("email1@example.com", "Name1"), ("email2@example.com", "Name2") }.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService AddMultipleBcc(
            IEnumerable<(string Email, string? Name)> bccRecipients);

        /// <summary>
        /// Adds a file attachment to the email message using the specified file path.
        /// </summary>
        /// <param name="filePath">The full path to the file to attach to the email message. The file must exist and be accessible.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService AddAttachment(
            string filePath);

        /// <summary>
        /// Adds multiple file attachments to the email message using the specified file path.
        /// </summary>
        /// <remarks>If any file path in the collection is invalid or the file does not exist, an
        /// exception may be thrown by the underlying attachment logic. The order of attachments in the email will match
        /// the order of the provided file paths.</remarks>
        /// <param name="filePaths">A collection of file paths representing the attachments to add. Each path must refer to an existing file.
        /// The example of multiple attachments is: new List&lt;string&gt; { "path/to/file1.txt", "path/to/file2.txt" }.</param>
        /// <returns>The current instance of the email service, enabling method chaining.</returns>
        IEmailService AddAttachments(
            IEnumerable<string> filePaths);

        /// <summary>
        /// Sends the composed email message asynchronously using the configured SMTP settings.
        /// </summary>
        /// <remarks>If email validation fails, the method returns a failed result without attempting to
        /// send. The method logs information and errors related to the sending process. The operation may be cancelled
        /// by the provided cancellation token.</remarks>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the email sending operation. The default value is <see
        /// cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="EmailSendResult"/>
        /// indicating whether the email was sent successfully or describing any errors that occurred.</returns>
        Task<EmailSendResult> SendEmailAsync(
            CancellationToken cancellationToken = default);

        void Reset();
    }
}
