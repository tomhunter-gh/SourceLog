using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using SourceLog.Interface;

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
						db.LogSubscriptions.Include(s => s.Log).ToList().ForEach(EnsureInitialised);
						_logSubscriptions = db.LogSubscriptions.Local;
					}
				}

				return _logSubscriptions;
			}
		}

		private static void EnsureInitialised(LogSubscription s)
		{
			if (s.LogProvider == null)
			{
				s.LoadLogProviderPlugin();
			}
		}

		public void AddLogSubscription(string name, string logProviderTypeName, string url)
		{
			var logSubscription = new LogSubscription(name, logProviderTypeName, url) { Log = new TrulyObservableCollection<LogEntry>() };
			using (var db = new SourceLogContext())
			{
				db.LogSubscriptions.Add(logSubscription);
				db.SaveChanges();
			}

			LogSubscriptions.Add(logSubscription);
		}
	}
}
