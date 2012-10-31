using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;
using System.Data.Entity;

namespace SourceLog.Model
{
	public class LogSubscription : INotifyPropertyChanged
	{
		public int LogSubscriptionId { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public string LogProviderTypeName { get; set; }
		private TrulyObservableCollection<LogEntry> _log;
		public virtual TrulyObservableCollection<LogEntry> Log
		{
			get { return _log; }
			set
			{
				_log = value;
				_log.CollectionChanged += (s, e) => NotifyPropertyChanged("Log");
			}
		}

		public ILogProvider<ChangedFile> LogProvider { get; set; }
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

		public LogSubscription(string name, string vcsLogProviderName, string url)
			: this()
		{
			Name = name;
			Url = url;
			LogProviderTypeName = vcsLogProviderName;

			LoadLogProviderPlugin();
		}

		public void LoadLogProviderPlugin()
		{
			Type type = LogProviderPluginManager.LogProviderPluginTypes[LogProviderTypeName];
			LogProvider = Activator.CreateInstance(type) as ILogProvider<ChangedFile>;
			LogProvider.NewLogEntry += AddNewLogEntry;
			LogProvider.LogProviderException += LogProviderLogProviderException;
			LogProvider.SettingsXml = Url;
			DateTime maxDateTimeRetrieved = DateTime.MinValue;
			if (Log != null && Log.Count > 0)
			{
				maxDateTimeRetrieved = Log.Max(x => x.CommittedDate);
			}
			LogProvider.MaxDateTimeRetrieved = maxDateTimeRetrieved;
			LogProvider.Initialise();
		}

		static void LogProviderLogProviderException(object sender, LogProviderExceptionEventArgs args)
		{
			var logEntry =
				new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
					{
						Severity = TraceEventType.Error,
						Message = args.Exception.ToString()
					};
			Logger.Write(logEntry);
		}

		public void AddNewLogEntry(object sender, NewLogEntryEventArgs<ChangedFile> e)
		{
			GenerateFlowDocuments(e.LogEntry);

			using (var db = SourceLogContextProvider())
			{
				((LogEntry)e.LogEntry).LogSubscription = db.LogSubscriptions.Find(LogSubscriptionId);
				db.LogEntries.Add((LogEntry)e.LogEntry);
				db.SaveChanges();
			}

			if (_uiThread != null)
			{
				_uiThread.Post(entry =>
					{
						Log.Add((LogEntry)entry);
						NotifyPropertyChanged("Log");
					}, e.LogEntry);
			}
		}

		private static void GenerateFlowDocuments(ILogEntry<ChangedFile> logEntry)
		{
			logEntry.ChangedFiles.AsParallel().ForAll(changedFile =>
			{
				Debug.WriteLine("   GeneratingFlowDocument: " + changedFile.FileName);

				var diff = new SideBySideFlowDocumentDiffGenerator(changedFile.OldVersion, changedFile.NewVersion);
				changedFile.LeftFlowDocument = diff.LeftDocument;
				changedFile.RightFlowDocument = diff.RightDocument;
				changedFile.FirstModifiedLineVerticalOffset = diff.FirstModifiedLineVerticalOffset;
			});
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
	}
}
