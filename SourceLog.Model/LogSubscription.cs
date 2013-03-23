using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;

namespace SourceLog.Model
{
	public class LogSubscription : INotifyPropertyChanged
	{
		public int LogSubscriptionId { get; set; }
		private string _name;
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				NotifyPropertyChanged("Name");
			}
		}
		public string Url { get; set; }
		public string PluginTypeName { get; set; }
		private TrulyObservableCollection<LogEntry> _log;
		public TrulyObservableCollection<LogEntry> Log
		{
			get { return _log; }
			set
			{
				_log = value;
				_log.CollectionChanged += (s, e) => NotifyPropertyChanged("Log");
			}
		}

		public IPlugin LogProvider { get; set; }
		private readonly SynchronizationContext _uiThread;
		private Func<ISourceLogContext> SourceLogContextProvider { get; set; }

		public LogSubscription()
		{
			_uiThread = SynchronizationContext.Current;
			SourceLogContextProvider = () => new SourceLogContext();
		}

		public LogSubscription(Func<ISourceLogContext> sourceLogContextProvider)
			: this()
		{
			SourceLogContextProvider = sourceLogContextProvider;
		}

		public LogSubscription(string name, string pluginName, string url)
			: this()
		{
			Name = name;
			Url = url;
			PluginTypeName = pluginName;

			LoadPlugin();
		}

		public void LoadPlugin()
		{
			var type = PluginManager.PluginTypes[PluginTypeName];
			LogProvider = (IPlugin)Activator.CreateInstance(type);
			LogProvider.NewLogEntry += AddNewLogEntry;
			LogProvider.PluginException += LogProviderLogProviderException;
			LogProvider.SettingsXml = Url;
			DateTime maxDateTimeRetrieved = DateTime.MinValue;
			if (Log != null && Log.Count > 0)
			{
				maxDateTimeRetrieved = Log.Max(x => x.CommittedDate);
			}
			LogProvider.MaxDateTimeRetrieved = maxDateTimeRetrieved;
			LogProvider.Initialise();
		}

		static void LogProviderLogProviderException(object sender, PluginExceptionEventArgs args)
		{
			var logEntry =
				new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
					{
						Severity = TraceEventType.Error,
						Message = args.Exception.ToString()
					};
			Logger.Write(logEntry);
		}

		public void AddNewLogEntry(object sender, NewLogEntryEventArgs e)
		{
			var logEntry = new LogEntry(e.LogEntry);
			if (Log.Count(l => l.Revision == logEntry.Revision) > 0)
			{
				Logger.Write(new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
					{
						Message = String.Format("Subscription \"{2}\" already contains revision {0}, date (ticks) {1}", logEntry.Revision, logEntry.CommittedDate.Ticks, Name),
						Severity = TraceEventType.Error
					});
			}

			logEntry.GenerateFlowDocuments();

			using (var db = SourceLogContextProvider())
			{
				logEntry.LogSubscription = db.LogSubscriptions.Find(LogSubscriptionId);
				db.LogEntries.Add(logEntry);
				db.SaveChanges();
			}

			logEntry.UnloadChangedFiles();

			if (_uiThread != null)
			{
				_uiThread.Post(entry =>
					{
						Log.Add((LogEntry)entry);
						NotifyPropertyChanged("Log");
						var logEntryInfo = new NewLogEntryInfoEventHandlerArgs
							{
								LogSubscriptionName = Name,
								Author = ((LogEntry)entry).Author,
								Message = ((LogEntry)entry).Message
							};
						NewLogEntry(this, logEntryInfo);
					}, logEntry);
			}

		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public event NewLogEntryInfoEventHandler NewLogEntry;

		public void Update(string name, string pluginName, string settingsXml)
		{
			using (var db = new SourceLogContext())
			{
				db.LogSubscriptions.Attach(this);
				
				Name = name;
				PluginTypeName = pluginName;
				Url = settingsXml;
				
				db.SaveChanges();
			}

			// Not sure if this is necessary
			LogProvider.NewLogEntry -= AddNewLogEntry;
			LogProvider.PluginException -= LogProviderLogProviderException;

			LoadPlugin();
		}

		public override bool Equals(object obj)
		{
			var logSubscription = obj as LogSubscription;
			if (logSubscription == null)
				return false;
			return LogSubscriptionId.Equals(logSubscription.LogSubscriptionId);
		}

		public override int GetHashCode()
		{
			return LogSubscriptionId.GetHashCode();
		}

		internal void UnsubscribeEvents()
		{
			LogProvider.NewLogEntry -= AddNewLogEntry;
			LogProvider.PluginException -= LogProviderLogProviderException;
		}
	}
}
