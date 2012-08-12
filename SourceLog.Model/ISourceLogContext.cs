using System.Data.Entity;

namespace SourceLog.Model
{
	public interface ISourceLogContext
	{
		IDbSet<LogSubscription> LogSubscriptions { get; set; }

		int SaveChanges();
	}
}
