using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System;
using System.IO;

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

			var databaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,Environment.SpecialFolderOption.Create),
								 @"SourceLog");

			Directory.CreateDirectory(databaseDirectory);

			Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0",
																		   databaseDirectory,
																		   "Max Database Size=4000;Default Lock Timeout=30000");
		}
	}
}
