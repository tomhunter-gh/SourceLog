using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;

namespace SourceLog.Model
{
	public interface ISourceLogContext
	{
		IDbSet<LogSubscription> LogSubscriptions { get; set; }

		int SaveChanges();
		DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
		DbEntityEntry Entry(object entity);
		//ObjectContext ObjectContext { get; }
	}
}
