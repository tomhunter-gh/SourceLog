using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace SourceLog.Model
{
	public interface ISourceLogContext : IDisposable
	{
		IDbSet<LogSubscription> LogSubscriptions { get; set; }
		IDbSet<LogEntry> LogEntries { get; set; }
		IDbSet<ChangedFile> ChangedFiles { get; set; }

		int SaveChanges();
		DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
		DbEntityEntry Entry(object entity);
	}
}
