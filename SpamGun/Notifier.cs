using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Collections.Generic;

namespace BrianReiter.Notification
{
	public class Notifier
	{
		public FileInfo BodyFile { get; set; }
		public FileInfo SubjectFile { get; set; }
		public FileInfo DataFile { get; set; }
		public string FromEmail { get; set; }
		public string FromName { get; set; }
        public bool WhatIf { get; set; }
        public bool IsBodyHtml { get; set; }

		public Notifier() { WhatIf = false; }
		public Notifier( OptionInfo options ) : this()
		{
			BodyFile = new FileInfo( options.BodyPath );
			SubjectFile = new FileInfo( options.SubjectPath );
			DataFile = new FileInfo( options.DataPath );
			FromEmail = options.FromEmail;

			FromName = options.FromName;
            IsBodyHtml = options.IsBodyHtml;
		}

		public void Send( TextWriter output )
		{
			string bodyTemplate, subjectTemplate;
			using( var bodyReader = BodyFile.OpenText() )
			{ bodyTemplate = bodyReader.ReadToEnd(); }
			using( var subjectReader = SubjectFile.OpenText() )
			{ subjectTemplate = subjectReader.ReadToEnd(); }
			MailAddress fromAddress;
			if( !string.IsNullOrEmpty( FromName ) )
			{
				fromAddress = new MailAddress( FromEmail, FromName );
			}
			else 
			{
				fromAddress = new MailAddress( FromEmail );
			}
			var smtpClient = new SmtpClient();

			using( var dataReader = DataFile.OpenText() )
			{
				if( WhatIf )
				{ output.WriteLine( "*** WhatIf mode. No messages will be sent. ***" ); }

				string line;
				long lineNumber = 0;
				while( null != (line = dataReader.ReadLine()) )
				{
					lineNumber++;
					line = line.Trim();
					if( line == string.Empty || line.StartsWith( "#" ) ) { continue ; }

					var message = new MailMessage();
					message.Subject = subjectTemplate;
					message.Body = bodyTemplate;
					message.From = fromAddress;
                    if (IsBodyHtml)
                    {
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        message.IsBodyHtml = IsBodyHtml;

                        ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                        // Add the alternate body to the message.
                        AlternateView alternate = AlternateView.CreateAlternateViewFromString(bodyTemplate, mimeType);
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

