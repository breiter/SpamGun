using System;
using System.IO;
using MimeKit;
using MailKit.Net.Smtp;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BrianReiter.Notification
{
    public class Notifier
    {
        readonly FileInfo BodyFile; 
        readonly FileInfo HtmlBodyFile;
        readonly FileInfo SubjectFile;
        readonly FileInfo DataFile;
        readonly string FromEmail;
        readonly string FromName;
        public bool WhatIf { get; set; }

        protected IConfiguration Configuration { get; set; }
        protected SmtpClientService SmtpClient => new SmtpClientService(Configuration);
        public Notifier() { WhatIf = false; }
        public Notifier(IConfiguration configuration, OptionInfo options) : this()
        {
            Configuration = configuration;
            BodyFile = new FileInfo(options.BodyPath);
            HtmlBodyFile = null;
            if (!String.IsNullOrEmpty(options.HtmlBodyPath))
                HtmlBodyFile = new FileInfo(options.HtmlBodyPath);
            SubjectFile = new FileInfo( options.SubjectPath );
            DataFile = new FileInfo( options.DataPath );
            FromEmail = options.FromEmail;
            FromName = options.FromName;
        }

        public async Task SendAsync(TextWriter output)
        {
            string bodyTemplate, htmlBodyTemplate = null, subjectTemplate;
            using (var subjectReader = SubjectFile.OpenText())
            { subjectTemplate = subjectReader.ReadLine(); }
            using( var bodyReader = BodyFile.OpenText() )
            { bodyTemplate = bodyReader.ReadToEnd(); }
            if (HtmlBodyFile != null)
            {
                using var htmlBodyReader = HtmlBodyFile.OpenText();
                htmlBodyTemplate = htmlBodyReader.ReadToEnd();
            }

            MailboxAddress fromAddress = new MailboxAddress(FromName, FromEmail);

            using var dataReader = DataFile.OpenText();
            if (WhatIf)
            { output.WriteLine("*** WhatIf mode. No messages will be sent. ***"); }

            string line, name;
            long lineNumber = 0;
            while (null != (line = dataReader.ReadLine()))
            {
                lineNumber++;
                line = line.Trim();
                name = "Applicant";
                if (line == string.Empty || line.StartsWith("#")) { continue; }

                var message = new MimeMessage();
                if (line.Contains("\t"))
                {
                    var fields = line.Split('\t');
                    line = fields[0];
                    name = !String.IsNullOrEmpty(fields[1]) ? fields[1] : "Applicant";
                }

                message.From.Add(fromAddress);
                message.Subject = subjectTemplate;
                var builder = new BodyBuilder()
                {
                    TextBody = string.Format(bodyTemplate, name)
                };

                if (!String.IsNullOrEmpty(htmlBodyTemplate))
                {
                    builder.HtmlBody = string.Format(htmlBodyTemplate, name);
                }

                message.Body = builder.ToMessageBody();
                try
                {
                    message.To.Add(new MailboxAddress(name: null, address: line));

                    if (WhatIf)
                    {
                        output.WriteLine("Line {0}: Notification would be to \"{1}\".", lineNumber, line);
                    }
                    else
                    {
                        await SmtpClient.SendMessageAsync(message);
                        output.WriteLine("Line {0}: Notification sent to \"{1}\".", lineNumber, line);
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Line {0}: Error. {1}", lineNumber, ex.Message);
                }
            }
        }
    }
}

