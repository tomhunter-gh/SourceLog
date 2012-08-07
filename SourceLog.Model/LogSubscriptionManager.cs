using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SourceLog.Interface;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SourceLog.Model
{
	public class LogSubscriptionManager : ILogConsumer
	{
		private static readonly SourceLogContext DbLazyLoadContext = new SourceLogContext();

		private ObservableCollection<LogSubscription> _logSubscriptions;
		public ObservableCollection<LogSubscription> LogSubscriptions
		{
			get
			{
				if (_logSubscriptions == null)
				{
					//using (var db = new SourceLogContext())
					//{
					DbLazyLoadContext.LogSubscriptions.ToList().ForEach(EnsureInitialised);
					_logSubscriptions = DbLazyLoadContext.LogSubscriptions.Local;
					//}
				}

				return _logSubscriptions;
			}
		}

		private static void EnsureInitialised(LogSubscription s)
		{
			//if (!s.NewLogEntryWired)
			//{
			//    s.NewLogEntry += SaveNewLogEntry;
			//}
			if (s.LogProvider == null)
			{
				s.LoadLogProviderPlugin();
			}
		}

		//static void SaveNewLogEntry(object sender, NewLogEntryEventArgs<ChangedFile> e)
		//{
		//    SaveChanges();
		//}

		//internal static readonly object DbSaveLockObject = new object();
		//public static void SaveChanges()
		//{
		//    Task.Factory.StartNew(() =>
		//    {
		//        //Debug.WriteLine(DateTime.Now + ": SaveChanges before lock");
		//        lock (DbSaveLockObject)
		//        {
		//            Debug.WriteLine(DateTime.Now + ": SaveChanges in lock");
		//            Db.SaveChanges();
		//        }
		//        //Debug.WriteLine(DateTime.Now + ": SaveChanges after lock");
		//    });
		//}

		public void AddLogSubscription(string name, string logProviderTypeName, string url)
		{
			var logSubscription = new LogSubscription(name, logProviderTypeName, url)
				{Log = new ObservableCollection<LogEntry>()};
			using (var db = new SourceLogContext())
			{
				db.LogSubscriptions.Add(logSubscription);
				db.SaveChanges();
			}

			LogSubscriptions.Add(logSubscription);
		}
	}
}
