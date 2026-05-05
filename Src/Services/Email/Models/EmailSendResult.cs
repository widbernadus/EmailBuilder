using System.Collections.Generic;

namespace Src.Services.Email.Models
{
    public class EmailSendResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public List<string> Errors { get; set; }
            = new List<string>();

        public static EmailSendResult Ok(
            string message = "Email sent successfully")
        {
            return new EmailSendResult
            {
                Success = true,
                Message = message
            };
        }

        public static EmailSendResult Fail(
            string message,
            List<string>? errors = null)
        {
            return new EmailSendResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
