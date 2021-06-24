using Mono.Options;
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BrianReiter.Notification
{
	public class MainClass
	{
		public static void Main (string[] args)
		{ 
			string bodyPath = null, htmlBodyPath = null,subjectPath = null, dataPath = null;
			string fromAddress = null, fromName = null;
			bool help, whatif = false;
			var p = new OptionSet()
			{
				{ "b|body=", "Required: Email {BODY} TEXT template path.", x => bodyPath = x },
				{ "h|html=", "Optional: Email {BODY} HTML template path.", x => htmlBodyPath = x },
				{ "s|subject=", "Required: Email {SUBJECT} TXT file template path.", x => subjectPath = x },
				{ "d|data=", "Required: Email merge {DATA} TSV file path.", x => dataPath = x },
				{ "f|from=", "Required: Email address to send from.", x => fromAddress = x },
				{ "n|from-name=", "Optional: Friendly {FROM-NAME} of the sender.", x => fromName = x },
				{ "whatif", "Process the data file and args but send no email.", x => whatif = (x != null) },
				{ "?|help", "Show this message and exit.", x=> help = (x != null) }
				
			};

			p.Parse( args );
			help = (bodyPath == null) || (subjectPath == null) || (dataPath == null);
			if( help )
			{
				ShowHelp( p );
				return;
			}

			var options = new OptionInfo
			{
                BodyPath = bodyPath,
                HtmlBodyPath = htmlBodyPath,
				SubjectPath = subjectPath,
				DataPath = dataPath,
				FromEmail = fromAddress,
				FromName = fromName
			};

			IConfigurationBuilder builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables()
				.AddCommandLine(args);
			IConfiguration configuration = builder.Build();


			var notifier = new Notifier( configuration, options ) { WhatIf = whatif } ;
			Task.Run(async () => await notifier.SendAsync(Console.Out));
		}

		static void ShowHelp(OptionSet p)
		{
			Console.WriteLine( "Usage: dotnet SpamGun.dll OPTIONS+" );
			Console.WriteLine( "SpamGun notifier. Merges subject text and body HTML with TSV data file." );

			Console.WriteLine( "SpamGun.exe version {0}.",
				Assembly.GetExecutingAssembly().GetName().Version );
			Console.WriteLine();
			Console.WriteLine( "Options:" );
			p.WriteOptionDescriptions( Console.Out );

			Console.WriteLine( "TSV format: \r\n\tEMAIL\r\n" );
			Console.WriteLine ();
			Console.WriteLine( "\n# character at the beginning of a line indicates a comment." );
		}
	}
}
