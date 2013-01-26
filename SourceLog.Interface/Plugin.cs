using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SourceLog.Interface
{
	public class Plugin : IPlugin
	{
		protected Timer Timer;
		protected readonly Object LockObject = new Object();

		public string SettingsXml { get; set; }

		public DateTime MaxDateTimeRetrieved { get; set; }
		public void Initialise()
		{
			Logger.Write(new LogEntry
			{
				Message = "Plugin initialising",
				Categories = { "Plugin." + GetType().Name },
				Severity = TraceEventType.Information
			});

			Timer = new Timer(CheckForNewLogEntries);
			Timer.Change(0, 15000);
		}

		private void CheckForNewLogEntries(object state)
		{
			if (Monitor.TryEnter(LockObject))
			{
				try
				{
					Logger.Write(new LogEntry { Message = "Checking for new entries", 
						Categories = { "Plugin." + GetType().Name } });
					CheckForNewLogEntriesImpl();
				}
				catch (Exception ex)
				{
					var args = new PluginExceptionEventArgs { Exception = ex };
					if (PluginException != null)
						PluginException(this, args);
				}
				finally
				{
					Monitor.Exit(LockObject);
				}
			}
		}

		protected virtual void CheckForNewLogEntriesImpl()
		{
			throw new NotImplementedException("Please implement an overriding method in a derived class.");
		}

		public event NewLogEntryEventHandler NewLogEntry;
		public event PluginExceptionEventHandler PluginException;

		protected void OnNewLogEntry(NewLogEntryEventArgs e)
		{
			if (NewLogEntry != null)
				NewLogEntry(this, e);
		}

		protected void OnLogProviderException(PluginExceptionEventArgs e)
		{
			if (PluginException != null)
				PluginException(this, e);
		}
		
		public void Dispose()
		{
			Timer.Dispose();
			Timer = null;
		}
	}
}
