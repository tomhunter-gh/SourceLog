using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using LibGit2Sharp;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;

namespace SourceLog.Plugin.Git
{
	public class GitPlugin : ILogProvider
	{
		private Timer _timer;
		private readonly Object _lockObject = new Object();

		public string SettingsXml { get; set; }

		public DateTime MaxDateTimeRetrieved { get; set; }

		public void Initialise()
		{
			Logger.Write(new LogEntry
			{
				Message = "Plugin initialising",
				Categories = { "Plugin.Git" },
				Severity = TraceEventType.Information
			});

			_timer = new Timer(CheckForNewLogEntries);
			_timer.Change(0, 30000);
		}

		private void CheckForNewLogEntries(object state)
		{
			if (Monitor.TryEnter(_lockObject))
			{
				try
				{
					Logger.Write(new LogEntry { Message = "Checking for new entries", Categories = { "Plugin.Git" } });
					
					using (var repo = new Repository(SettingsXml))
					{

						foreach (var commit in repo.Branches["origin/HEAD"].Commits.Where(c => c.Committer.When > MaxDateTimeRetrieved).Take(30))
						{
							var logEntry = new LogEntryDto()
								{
									Revision = commit.Sha.Substring(0, 7),
									Author = commit.Author.Name,
									CommittedDate = commit.Committer.When.DateTime,
									Message = commit.Message,
									ChangedFiles = new List<ChangedFileDto>()
								};

							TreeChanges changes = repo.Diff.Compare(commit.Parents.First().Tree, commit.Tree);
							changes.First().
							commit.Tree.
						}

					}
				}
				catch (Exception ex)
				{
					var args = new LogProviderExceptionEventArgs { Exception = ex };
					LogProviderException(this, args);
				}
				finally
				{
					Monitor.Exit(_lockObject);
				}
			}
		}

		public event NewLogEntryEventHandler NewLogEntry;
		public event LogProviderExceptionEventHandler LogProviderException;
	}
}
