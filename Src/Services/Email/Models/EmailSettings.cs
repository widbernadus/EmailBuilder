namespace Src.Services.Email.Models
{
    public class EmailSettings
    {
        public string DefaultProvider { get; set; }

        public Dictionary<string, SmtpProviderSettings> Providers{ get; set; }
    }
}
