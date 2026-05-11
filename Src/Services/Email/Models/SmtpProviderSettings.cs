using System;
using System.Collections.Generic;
using System.Text;

namespace Src.Services.Email.Models
{
    public class SmtpProviderSettings
    {
        public string SmtpServer { get; set; }

        public int Port { get; set; }

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool UseSsl { get; set; }
    }
}
