using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
	}
}
