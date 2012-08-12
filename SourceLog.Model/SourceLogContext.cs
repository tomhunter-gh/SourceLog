using System;
using System.Data.Entity;

namespace SourceLog.Model
{
	public class SourceLogContext : DbContext, ISourceLogContext
	{
		[ThreadStatic]
		private static ISourceLogContext _threadStaticContext;

		public static ISourceLogContext ThreadStaticContext
		{
			get { return _threadStaticContext ?? (_threadStaticContext = new SourceLogContext()); }
			set { _threadStaticContext = value; }
		}

		public IDbSet<LogSubscription> LogSubscriptions { get; set; }
	}
}
