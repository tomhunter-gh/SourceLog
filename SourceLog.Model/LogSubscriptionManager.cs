using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using EntLib = Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SourceLog.Model
{
	public class LogSubscriptionManager
	{
		private ObservableCollection<LogSubscription> _logSubscriptions;
		public ObservableCollection<LogSubscription> LogSubscriptions
		{
			get
			{
				if (_logSubscriptions == null)
				{
					using (var db = new SourceLogContext())
					{
						EntLib.Logger.Write(new EntLib.LogEntry
							{
								Message = "Initialising LogSubscriptions..",
								Severity = TraceEventType.Information
							});
						db.LogSubscriptions.Include(s => s.Log).ToList().ForEach(EnsureInitialised);
						_logSubscriptions = new ObservableCollection<LogSubscription>(db.LogSubscriptions.ToList());
					}
				}

				return _logSubscriptions;
			}
		}

		private void EnsureInitialised(LogSubscription s)
		{
			if (s.LogProvider == null)
			{
				s.LoadLogProviderPlugin();
				s.NewLogEntry += (o, e) => NewLogEntry(o, e);
			}
		}

		public LogSubscription AddLogSubscription(string name, string logProviderTypeName, string url)
		{
			var logSubscription = new LogSubscription(name, logProviderTypeName, url) { Log = new TrulyObservableCollection<LogEntry>() };
			using (var db = new SourceLogContext())
			{
				db.LogSubscriptions.Add(logSubscription);
				db.SaveChanges();
			}
			logSubscription.NewLogEntry += (o, e) => NewLogEntry(o, e);
			LogSubscriptions.Add(logSubscription);
			return logSubscription;
		}

		public event NewLogEntryInfoEventHandler NewLogEntry;

		public void DeleteSubscription(LogSubscription logSubscription)
		{
			using (var db = new SourceLogContext())
			{
				foreach (var logEntry in logSubscription.Log)
				{
					logEntry.ChangedFiles = null;
					logEntry.LogSubscription = logSubscription;
				}
				db.LogSubscriptions.Attach(logSubscription);
				db.LogSubscriptions.Remove(logSubscription);
				db.SaveChanges();
			}
			_logSubscriptions.Remove(logSubscription);
		}
	}
}
