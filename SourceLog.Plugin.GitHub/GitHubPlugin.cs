using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using SourceLog.Model;
using SourceLog.Interface;
using Newtonsoft.Json;

namespace SourceLog.Plugin.GitHub
{
	public class GitHubLogProvider : ILogProvider<ChangedFile>
	{
		private Timer _timer;
		private readonly Object _lockObject = new Object();

		private string _username;
		private string _reponame;

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

			//https://github.com/tomhunter-gh/SourceLog

			//const string pattern = @"Change\s(?<revision>\d+)\son\s(?<datetime>\d{4}/\d{2}/\d{2}\s\d{2}:\d{2}:\d{2})\sby\s(?<author>\w+)@\w+\n\n\t(?<message>.*)";
			const string pattern = @"https://github.com/(?<username>[^/]+)/(?<reponame>[^/]+)/?";
			var r = new Regex(pattern);
			var match = r.Match(SettingsXml);
			if (match.Success)
			{
				_username = match.Groups["username"].Value;
				_reponame = match.Groups["reponame"].Value;
			}

			_timer = new Timer(CheckForNewLogEntries);
			_timer.Change(0, 15000);
		}


		public event NewLogEntryEventHandler<ChangedFile> NewLogEntry;

		private void CheckForNewLogEntries(object state)
		{
			if (Monitor.TryEnter(_lockObject))
			{
				try
				{
					//Debug.WriteLine("GitHubPlugin: Checking for new entries. Thread: " + Thread.CurrentThread.ManagedThreadId);
					var repoLog = JsonConvert.DeserializeObject<RepoLog>(GitHubApiGet(
						"https://api.github.com/repos/" + _username + "/"
						+ _reponame + "/commits"
					));

					if (repoLog.Count() > 0)
					{
						foreach (var commitEntry in repoLog.Where(x => DateTime.Parse(x.commit.committer.date) > MaxDateTimeRetrieved)
							.OrderBy(x => DateTime.Parse(x.commit.committer.date)))
						{
							var logEntry = new LogEntry
								{
									Revision = commitEntry.sha.Substring(0,7),
									Author = commitEntry.commit.committer.name,
									CommittedDate = DateTime.Parse(commitEntry.commit.committer.date),
									Message = commitEntry.commit.message,
									ChangedFiles = new List<ChangedFile>()
								};

							var fullCommitEntry = JsonConvert.DeserializeObject<CommitEntry>(
								GitHubApiGet(
									"https://api.github.com/repos/" + _username + "/"
									+ _reponame + "/commits/"
									+ commitEntry.sha
								)
							);
							
							// process changed files in parallel
							fullCommitEntry.files.AsParallel().ForAll(file =>
							{
								var changedFile = new ChangedFile
									{
										FileName = file.filename,
										NewVersion = GitHubApiGet(file.raw_url),
										ChangeType = ChangeType.Modified
									};

								// get the previous version
								// first get the list of commits for the file
								var fileLog = JsonConvert.DeserializeObject<RepoLog>(
									GitHubApiGet(
										"https://api.github.com/repos/" + _username + "/"
										+ _reponame + "/commits?path=" + file.filename
										)
									);

								// get most recent commit before this one
								var previousCommit = fileLog.Where(f => DateTime.Parse(f.commit.committer.date) < logEntry.CommittedDate)
									.OrderByDescending(f => DateTime.Parse(f.commit.committer.date))
									.FirstOrDefault();

								if (previousCommit != null)
								{
									// get the raw contents of the path at the previous commit sha
									changedFile.OldVersion = GitHubApiGet(
											"https://github.com/" + _username + "/"
											+ _reponame + "/raw/"
											+ previousCommit.sha + "/"
											+ changedFile.FileName
										);
								}
								else
								{
									changedFile.OldVersion = String.Empty;
									changedFile.ChangeType = ChangeType.Added;
								}

								logEntry.ChangedFiles.Add(changedFile);
							});

							var args = new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry };

							NewLogEntry(this, args);
						}
						MaxDateTimeRetrieved = repoLog.Max(x => DateTime.Parse(x.commit.committer.date));
					}
				}
				finally
				{
					Monitor.Exit(_lockObject);
				}

			}

		}

		static string GitHubApiGet(string uri)
		{
			Debug.WriteLine(" [GitHubPlugin] GitHubApiGet: " + uri);
			var request = WebRequest.Create(uri);
			using (var response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					return reader.ReadToEnd();
				}
			}

		}
	}

	public class RepoLog : List<CommitEntry>
	{ }

	public class CommitEntry
	{
		public Commit commit { get; set; }
		public string sha { get; set; }
		public List<GitHubFile> files { get; set; }
	}

	public class Commit
	{
		public Committer committer { get; set; }
		public string message { get; set; }
	}

	public class Committer
	{
		public string date { get; set; }
		public string name { get; set; }
	}

	public class GitHubFile
	{
		public string filename { get; set; }
		public string raw_url { get; set; }
	}
}
