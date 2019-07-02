using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace BrianReiter.Notification
{
	public class Notifier
	{
		public FileInfo BodyFile { get; set; }
        public FileInfo HtmlBodyFile { get; set; }
		public FileInfo SubjectFile { get; set; }
		public FileInfo DataFile { get; set; }
		public string FromEmail { get; set; }
		public string FromName { get; set; }
        public bool WhatIf { get; set; }

		protected IConfiguration Configuration { get; set; }
		protected SmtpClientFactory SmtpClientFactory => new SmtpClientFactory(Configuration);
        public Notifier() { WhatIf = false; }
		public Notifier( IConfiguration configuration, OptionInfo options ) : this()
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

		public void Send( TextWriter output )
		{
			string bodyTemplate, htmlBodyTemplate = null, subjectTemplate;
			using( var bodyReader = BodyFile.OpenText() )
			{ bodyTemplate = bodyReader.ReadToEnd(); }
            if (HtmlBodyFile != null)
            {
                using (var htmlBodyReader = HtmlBodyFile.OpenText())
                { htmlBodyTemplate = htmlBodyReader.ReadToEnd(); }
            }
            using (var subjectReader = SubjectFile.OpenText())
            { subjectTemplate = subjectReader.ReadLine(); }
			MailAddress fromAddress;
			if( !string.IsNullOrEmpty( FromName ) )
			{
				fromAddress = new MailAddress( FromEmail, FromName );
			}
			else 
			{
				fromAddress = new MailAddress( FromEmail );
			}
			var smtpClient = SmtpClientFactory.NewSmtpClient();

			using( var dataReader = DataFile.OpenText() )
			{
				if( WhatIf )
				{ output.WriteLine( "*** WhatIf mode. No messages will be sent. ***" ); }

				string line;
				string[] formatParameters;
				long lineNumber = 0;
				while( null != (line = dataReader.ReadLine()) )
				{
					lineNumber++;
					line = line.Trim();
                    formatParameters = new string[] {};
					if( line == string.Empty || line.StartsWith( "#" ) ) { continue ; }

					var message = new MailMessage();
                    if (line.Contains("\t")) 
                    {
                        var fields = line.Split('\t');
                        line = fields[0];
                        formatParameters = fields.Skip(1).ToArray();
                    }
					message.Subject = subjectTemplate;
                    message.Body = string.Format(bodyTemplate, formatParameters);
					message.From = fromAddress;
                    if (!String.IsNullOrEmpty(htmlBodyTemplate))
                    {
                        ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                        // Add the alternate body to the message.
                        AlternateView alternate = AlternateView.CreateAlternateViewFromString(string.Format(htmlBodyTemplate, formatParameters), mimeType);
                        message.AlternateViews.Add(alternate);
                    }
					try
					{  
						message.To.Add( new MailAddress(line) );

						if( WhatIf )
						{
							output.WriteLine( "Line {0}: Notification would be to \"{1}\".", lineNumber, line);
						}
						else
						{
							smtpClient.Send( message );
							output.WriteLine( "Line {0}: Notification sent to \"{1}\".", lineNumber, line);
						}
					}
					catch( Exception ex )
					{
						output.WriteLine( "Line {0}: Error. {1}", lineNumber, ex.Message );
					}
				}
			}
		}
	}
}

