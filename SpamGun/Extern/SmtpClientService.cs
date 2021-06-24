using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace MailKit.Net.Smtp
{
    public class SmtpClientService
    {
        IConfiguration Configuration { get; }

        string SmtpHost => Configuration.GetValue<string>("Mail:Smtp:Host", "localhost");
        int SmtpPort => Configuration.GetValue<int>("Mail:Smtp:Port", 25);
        SecureSocketOptions SmtpSecureSocketOptions => Configuration.GetValue<SecureSocketOptions>("Mail:Smtp:SecureSocketOptions", SecureSocketOptions.Auto);
        bool SmtpDisableCertificateValidation => Configuration.GetValue<bool>("Mail:Smtp:DisableCertificateValidation", false);
        bool SmtpAuthenticate => Configuration.GetValue<bool>("Mail:Smtp:Authenticate", false);
        string SmtpAuthenticationUsername => Configuration.GetValue<string>("Mail:Smtp:Authentication:Username", string.Empty);
        string SmtpAuthenticationPassword => Configuration.GetValue<string>("Mail:Smtp:Authentication:Password", string.Empty);

        public SmtpClientService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public async Task SendMessageAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            if (SmtpDisableCertificateValidation) client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(host: SmtpHost, port: SmtpPort, options: SmtpSecureSocketOptions);
            if (SmtpAuthenticate) await client.AuthenticateAsync(userName: SmtpAuthenticationUsername, password: SmtpAuthenticationPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}