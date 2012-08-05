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
		private static readonly SourceLogContext Db = new SourceLogContext();

		public ObservableCollection<LogSubscription> LogSubscriptions
		{
			get
			{
				Db.LogSubscriptions.ToList().ForEach(EnsureInitialised);
				return Db.LogSubscriptions.Local;
			}
		}

		private static void EnsureInitialised(LogSubscription s)
		{
			if (!s.NewLogEntryWired)
			{
				s.NewLogEntry += SaveNewLogEntry;
			}
			if (s.LogProvider == null)
			{
				s.LoadLogProviderPlugin();
			}
		}

		static void SaveNewLogEntry(object sender, NewLogEntryEventArgs<ChangedFile> e)
		{
			SaveChanges();
		}

		internal static readonly object DbSaveLockObject = new object();
		public static void SaveChanges()
		{
			Task.Factory.StartNew(() =>
			{
				//Debug.WriteLine(DateTime.Now + ": SaveChanges before lock");
				lock (DbSaveLockObject)
				{
					Debug.WriteLine(DateTime.Now + ": SaveChanges in lock");
					Db.SaveChanges();
				}
				//Debug.WriteLine(DateTime.Now + ": SaveChanges after lock");
			});
		}

		public static void AddLogSubscription(string name, string logProviderTypeName, string url)
		{
			var logSubscription = new LogSubscription(name, logProviderTypeName, url)
				{Log = new ObservableCollection<LogEntry>()};
			Db.LogSubscriptions.Add(logSubscription);
			SaveChanges();
		}
	}
}
