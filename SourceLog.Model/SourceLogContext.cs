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
			get
			{
				if (_threadStaticContext == null)
				{
					_threadStaticContext = new SourceLogContext();
				}
				return _threadStaticContext;
			}
			set { _threadStaticContext = value; }
		}

		// Background context used to avoid getting a reference to objects added to a UI collection
		[ThreadStatic]
		private static ISourceLogContext _threadStaticContextBackground;
		public static ISourceLogContext ThreadStaticContextBackground
		{
			get
			{
				if (_threadStaticContextBackground == null)
				{
					_threadStaticContextBackground = new SourceLogContext();
				}
				return _threadStaticContextBackground;
			}
			set { _threadStaticContextBackground = value; }
		}

		public IDbSet<LogSubscription> LogSubscriptions { get; set; }

		public SourceLogContext()
		{
			Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SourceLogContext>());
		}
	}
}
