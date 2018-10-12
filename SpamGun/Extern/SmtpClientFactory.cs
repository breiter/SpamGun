using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace System.Net.Mail
{
    public class SmtpClientFactory
    {
        IConfigurationSection SmtpClientConfiguration { get; set; }
        public SmtpClientFactory(IConfiguration configuration)
        {
            SmtpClientConfiguration = configuration.GetSection("SmtpClient");
        }

        public SmtpClient NewSmtpClient()
        {
            var client = new SmtpClient
            {
                Host = SmtpClientConfiguration.GetValue<string>("Host", "localhost"),
                Port = SmtpClientConfiguration.GetValue<int>("Port", 25),
                EnableSsl = SmtpClientConfiguration.GetValue<bool>("EnableSsl", false),
                Timeout = SmtpClientConfiguration.GetValue<int>("Timeout", 1000000)
            };
            if (SmtpClientConfiguration.GetValue<bool>("UseCredentials", false))
            {
                client.UseDefaultCredentials = false;

                var credentialConfiguration = SmtpClientConfiguration.GetSection("Credentials");
                client.Credentials = new NetworkCredential
                {
                    Domain = credentialConfiguration.GetValue<string>("Domain", ""),
                    UserName = credentialConfiguration.GetValue<string>("UserName", ""),
                    Password = credentialConfiguration.GetValue<string>("Password", "")
                };
            }
            return client;
        }
    }
}