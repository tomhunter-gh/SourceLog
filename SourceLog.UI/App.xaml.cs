using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

			var logEntry = new LogEntry {Message = "Application starting..", Severity = TraceEventType.Information};
			Logger.Write(logEntry);
		}

		static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Write(e.ExceptionObject);
		}

		private void ApplicationExit(object sender, ExitEventArgs e)
		{
			Logger.Write(new LogEntry
				{
					Message = "Application closing..",
					Severity = TraceEventType.Information
				});
		}
	}
}
