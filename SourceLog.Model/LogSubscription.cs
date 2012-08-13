using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using SourceLog.Interface;
using System.Collections.ObjectModel;
using System.Linq;

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
				_log.CollectionChanged += (s,e) => NotifyPropertyChanged("Log");
			}
		}

		public ILogProvider<ChangedFile> LogProvider { get; set; }
		private readonly SynchronizationContext _uiThread;

		//[NotMapped]
		//public int UnreadLogEntryCount
		//{
		//    get { return Log.Count(le => !le.Read); }
		//}

		public LogSubscription()
		{
			_uiThread = SynchronizationContext.Current;
		}

		//void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		//{
		//    NotifyCollectionChangedEventArgs a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
		//    OnCollectionChanged(a);
		//}

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
			LogProvider.SettingsXml = Url;
			DateTime maxDateTimeRetrieved = DateTime.MinValue;
			if (Log != null && Log.Count > 0)
			{
				maxDateTimeRetrieved = Log.Max(x => x.CommittedDate);
			}
			LogProvider.MaxDateTimeRetrieved = maxDateTimeRetrieved;
			LogProvider.Initialise();
		}

		public void AddNewLogEntry(object sender, NewLogEntryEventArgs<ChangedFile> e)
		{
			GenerateFlowDocuments(e.LogEntry);

			var db = SourceLogContext.ThreadStaticContext;
			db.LogSubscriptions.First(s => s.LogSubscriptionId == LogSubscriptionId).Log.Add((LogEntry)e.LogEntry);
			db.SaveChanges();

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

				changedFile.OldVersion = CheckForBinary(changedFile.OldVersion);
				changedFile.NewVersion = CheckForBinary(changedFile.NewVersion);

				var diff = new SideBySideFlowDocumentDiffGenerator(changedFile.OldVersion, changedFile.NewVersion);
				changedFile.LeftFlowDocument = diff.LeftDocument;
				changedFile.RightFlowDocument = diff.RightDocument;
				changedFile.FirstModifiedLineVerticalOffset = diff.FirstModifiedLineVerticalOffset;
			});
		}

		private static string CheckForBinary(string s)
		{
			if (s.Contains("\0\0\0\0"))
			{
				Debug.WriteLine("    [Binary]");
				return "[Binary]";
			}
			return s;
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
