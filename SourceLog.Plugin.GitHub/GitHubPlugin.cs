using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using SourceLog.Model;
using SourceLog.Interface;
using Newtonsoft.Json;

namespace SourceLog.GitHub
{
	public class GitHubLogProvider : ILogProvider<ChangedFile>
	{
		private Timer _timer;
		private readonly Object _lockObject = new Object();

		public string SettingsXml
		{
			get;
			set;
		}

		public DateTime MaxDateTimeRetrieved
		{
			get;
			set;
		}

		public void Initialise()
		{
			//Debug.WriteLine("GitHubPlugin: Initialising.");
			//_timer = new Timer(CheckForNewLogEntries);
			//_timer.Change(0, 3000);
		}


		public event NewLogEntryEventHandler<ChangedFile> NewLogEntry;

		private void CheckForNewLogEntries(object state)
		{
			if (Monitor.TryEnter(_lockObject))
			{
				try
				{
					//Debug.WriteLine("GitHubPlugin: Checking for new entries. Thread: " + Thread.CurrentThread.ManagedThreadId);
					var request = WebRequest.Create(SettingsXml);
					using (var response = request.GetResponse())
					{
						using (var reader = new StreamReader(response.GetResponseStream()))
						{
							var data = reader.ReadToEnd();

							var repoLog = JsonConvert.DeserializeObject<RepoLog>(data);

							if (repoLog.Count() > 0)
							{
								foreach (var repoLogEntry in repoLog.Where(x => DateTime.Parse(x.commit.committer.date) > MaxDateTimeRetrieved))
								{
									var logEntry = new LogEntry
										{
											Author = repoLogEntry.commit.committer.name,
											CommittedDate = DateTime.Parse(repoLogEntry.commit.committer.date),
											ChangedFiles = null,
											Message = repoLogEntry.commit.message
										};

									var args = new NewLogEntryEventArgs<ChangedFile> {LogEntry = logEntry};

									NewLogEntry(this, args);
								}
								MaxDateTimeRetrieved = repoLog.Max(x => DateTime.Parse(x.commit.committer.date));
							}
						}
					}
				}
				finally
				{
					Monitor.Exit(_lockObject);
				}

			}

		}
	}

	public class RepoLog : List<CommitEntry>
	{ }

	public class CommitEntry
	{
		public Commit commit { get; set; }

	}

	public class Committer
	{
		public string date { get; set; }
		public string name { get; set; }
	}

	public class Commit
	{
		public Committer committer { get; set; }
		public string message { get; set; }
	}
}
