using System.Data.Entity;

namespace SourceLog.Model
{
	public class SourceLogContext : DbContext, ISourceLogContext
	{
		public IDbSet<LogSubscription> LogSubscriptions { get; set; }
		public IDbSet<LogEntry> LogEntries { get; set; }
		public IDbSet<ChangedFile> ChangedFiles { get; set; }

		public SourceLogContext()
		{
			Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SourceLogContext>());
			Configuration.ProxyCreationEnabled = false;
		}
	}
}
