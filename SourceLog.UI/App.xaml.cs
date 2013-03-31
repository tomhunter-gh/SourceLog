using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

			var logEntry = new LogEntry
				{
					Message = String.Format("Application starting.. (version: {0})", Assembly.GetExecutingAssembly().GetName().Version),
					Severity = TraceEventType.Information
				};
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
