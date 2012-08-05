using System.Data.Entity;

namespace SourceLog.Model
{
	public class SourceLogContext : DbContext
	{
		public DbSet<LogSubscription> LogSubscriptions { get; set; }
	}
}
