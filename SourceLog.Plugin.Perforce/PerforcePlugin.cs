using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceLog.Interface;
using SourceLog.Model;
using System.Threading;

namespace SourceLog.Plugin.Perforce
{
	public class PerforcePlugin : ILogProvider<ChangedFile>
	{
		private Timer _timer;
		private readonly Object _lockObject = new Object();

		public string SettingsXml { get; set; }

		public DateTime MaxDateTimeRetrieved { get; set; }

		public void Initialise()
		{
			_timer = new Timer(CheckForNewLogEntries);
			_timer.Change(0, 15000);
		}

		private void CheckForNewLogEntries(object state)
		{
			if (Monitor.TryEnter(_lockObject))
			{
				try
				{
					List<LogEntry> logEntries = new List<LogEntry>();
					P4COM.p4 p4 = new P4COM.p4();
					p4.Connect();
					var p4changes = p4.run("changes -t -l -s submitted -m 30 \"//depot/trunk/...\"");
					foreach (string changeset in p4changes)
					{
						var logEntry = PerforceLogParser.Parse(changeset);
						if (logEntry.CommittedDate <= MaxDateTimeRetrieved)
							continue;
						logEntries.Add(logEntry);
					}

					foreach (var logEntry in logEntries.OrderBy(le => le.CommittedDate))
					{
						ChangedFile changedFile;

						// grab changed files
						var p4files = p4.run("files @=" + logEntry.Revision);
						foreach (string file in p4files)
						{
							changedFile = PerforceLogParser.ParseP4File(file);
							if (changedFile.ChangeType == ChangeType.Added
								|| changedFile.ChangeType == ChangeType.Copied
								|| changedFile.ChangeType == ChangeType.Modified
								|| changedFile.ChangeType == ChangeType.Moved)
							{
								changedFile.NewVersion = String.Join(Environment.NewLine, p4.run("print " + changedFile.FileName + "@" + logEntry.Revision));
							}

							if (changedFile.ChangeType == ChangeType.Copied
								|| changedFile.ChangeType == ChangeType.Deleted
								|| changedFile.ChangeType == ChangeType.Modified
								|| changedFile.ChangeType == ChangeType.Moved)
							{
								changedFile.OldVersion = String.Join(Environment.NewLine, p4.run("print " + changedFile.FileName + "@" + (Int32.Parse(logEntry.Revision) - 1)));
							}
						}

						var args = new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry };
						NewLogEntry(this, args);
					}

					MaxDateTimeRetrieved = logEntries.Max(x => x.CommittedDate);
				}
				finally
				{
					Monitor.Exit(_lockObject);
				}
			}
		}

		public event NewLogEntryEventHandler<ChangedFile> NewLogEntry;
	}
}
