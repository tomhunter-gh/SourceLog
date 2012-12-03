using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Newtonsoft.Json;
using SourceLog.Interface;

namespace SourceLog.Plugin.GitHub
{
	public class GitHubLogProvider : ILogProvider
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
			Logger.Write(new LogEntry
				{
					Message = "Plugin initialising",
					Categories = {"Plugin.GitHub"},
					Severity = TraceEventType.Information
				});

			const string pattern = @"https://github.com/(?<username>[^/]+)/(?<reponame>[^/]+)/?";
			var r = new Regex(pattern);
			var match = r.Match(SettingsXml);
			if (match.Success)
			{
				_username = match.Groups["username"].Value;
				_reponame = match.Groups["reponame"].Value;
			}

			_timer = new Timer(CheckForNewLogEntries);
			_timer.Change(0, 60000);
		}


		public event NewLogEntryEventHandler NewLogEntry;
		public event LogProviderExceptionEventHandler LogProviderException;

		private void CheckForNewLogEntries(object state)
		{
			if (Monitor.TryEnter(_lockObject))
			{
				try
				{
					Logger.Write(new LogEntry {Message = "Checking for new entries", Categories = {"Plugin.GitHub"}});

					var repoLog = JsonConvert.DeserializeObject<RepoLog>(GitHubApiGet(
						"https://api.github.com/repos/" + _username + "/"
						+ _reponame + "/commits"
					));

					if (repoLog.Count() > 0)
					{
						var maxDateTimeRetrievedAtStartOfProcessing = MaxDateTimeRetrieved;
						foreach (var commitEntry in repoLog.Where(x => DateTime.Parse(x.commit.committer.date) > maxDateTimeRetrievedAtStartOfProcessing)
							.OrderBy(x => DateTime.Parse(x.commit.committer.date)))
						{
							var logEntry = new LogEntryDto
								{
									Revision = commitEntry.sha.Substring(0, 7),
									Author = commitEntry.commit.committer.name,
									CommittedDate = DateTime.Parse(commitEntry.commit.committer.date),
									Message = commitEntry.commit.message,
									ChangedFiles = new List<ChangedFileDto>()
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
								var changedFile = new ChangedFileDto
									{
										FileName = file.filename,
										//NewVersion = GitHubApiGet(file.raw_url),
										//ChangeType = ChangeType.Modified
									};

								if (file.status == "removed")
								{
									changedFile.ChangeType = ChangeType.Deleted;
									changedFile.OldVersion = GitHubApiGet(file.raw_url);
									changedFile.NewVersion = String.Empty;
								}
								else
								{
									changedFile.ChangeType = ChangeType.Modified;
									changedFile.NewVersion = GitHubApiGet(file.raw_url);

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
								}

								logEntry.ChangedFiles.Add(changedFile);
							});

							var args = new NewLogEntryEventArgs { LogEntry = logEntry };

							NewLogEntry(this, args);
							MaxDateTimeRetrieved = logEntry.CommittedDate;
						}
					}
				}
				catch (GitHubApiRateLimitException)
				{
					Logger.Write(new LogEntry { Message = "[GitHubPlugin] GitHubApiRateLimitException - sleeping for 1 hr", Categories = {"Plugin.GitHub"}});
					Thread.Sleep(TimeSpan.FromHours(1));
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

		static string GitHubApiGet(string uri)
		{
			Logger.Write(new LogEntry { Message = "GitHubApiGet: " + uri, Categories = {"Plugin.GitHub"}});
			var request = WebRequest.Create(uri);
			try
			{
				using (var response = request.GetResponse())
				{
					using (var reader = new StreamReader(response.GetResponseStream()))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (WebException ex)
			{
				if (ex.Response.Headers["X-RateLimit-Remaining"] == "0")
				{
					throw new GitHubApiRateLimitException("RateLimit-Remaining = 0", ex);
				}
				throw;
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
		public string status { get; set; }
		public string raw_url { get; set; }
	}

	public class GitHubApiRateLimitException : Exception
	{
		public GitHubApiRateLimitException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}
