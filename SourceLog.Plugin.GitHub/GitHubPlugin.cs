using System;
using System.Collections.Generic;
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
	public class GitHubPlugin : Interface.Plugin
	{
		private string _username;
		private string _reponame;

		public new void Initialise()
		{
			const string pattern = @"https://github.com/(?<username>[^/]+)/(?<reponame>[^/]+)/?";
			var r = new Regex(pattern);
			var match = r.Match(SettingsXml);
			if (match.Success)
			{
				_username = match.Groups["username"].Value;
				_reponame = match.Groups["reponame"].Value;
			}

			base.Initialise();
			// 60 second interval to try and comply with 60 reqs per hour limit
			// should really count how many instances of the GitHub plugin are alive
			Timer.Change(0, 60000);
		}

		protected override void CheckForNewLogEntriesImpl()
		{
			var repoLog = JsonConvert.DeserializeObject<RepoLog>(GitHubApiGetString(
				"https://api.github.com/repos/" + _username + "/"
				+ _reponame + "/commits"
			));

			if (repoLog.Any())
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
						GitHubApiGetString(
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
							changedFile.OldVersion = GitHubApiGetBinary(file.raw_url);
							changedFile.NewVersion = new byte[0];
						}
						else
						{
							changedFile.ChangeType = ChangeType.Modified;
							changedFile.NewVersion = GitHubApiGetBinary(file.raw_url);

							// get the previous version
							// first get the list of commits for the file
							var fileLog = JsonConvert.DeserializeObject<RepoLog>(
								GitHubApiGetString(
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
								changedFile.OldVersion = GitHubApiGetBinary(
									"https://github.com/" + _username + "/"
									+ _reponame + "/raw/"
									+ previousCommit.sha + "/"
									+ changedFile.FileName
									);
							}
							else
							{
								changedFile.OldVersion = new byte[0];
								changedFile.ChangeType = ChangeType.Added;
							}
						}

						logEntry.ChangedFiles.Add(changedFile);
					});

					var args = new NewLogEntryEventArgs { LogEntry = logEntry };

					OnNewLogEntry(args);
					MaxDateTimeRetrieved = logEntry.CommittedDate;
				}
			}
		}

		static byte[] GitHubApiGetBinary(string uri)
		{
			Logger.Write(new LogEntry { Message = "GitHubApiGet: " + uri, Categories = { "Plugin.GitHub" } });
			var request = WebRequest.Create(uri);
			try
			{
				using (var response = request.GetResponse())
				{
					using (var memoryStream = new MemoryStream())
					{
						response.GetResponseStream().CopyTo(memoryStream);
						return memoryStream.ToArray();
					}
				}
			}
			catch (WebException ex)
			{
				if (ex.Response.Headers["X-RateLimit-Remaining"] == "0")
				{
					Logger.Write(new LogEntry { Message = "GitHub API rate limit met - sleeping for 1 hr", Categories = { "Plugin.GitHub" } });
					Thread.Sleep(TimeSpan.FromHours(1));
					return GitHubApiGetBinary(uri);
				}

				// Getting a 404 from the raw_url on a changed subproject.
				// E.g. filename "libgit2" on this commit: 
				//  https://github.com/libgit2/libgit2sharp/commit/39c2ed2233b3d99e33c031183e26996d1210c329
				//  https://api.github.com/repos/libgit2/libgit2sharp/commits/39c2ed2233b3d99e33c031183e26996d1210c329
				if (ex.Response.Headers["status"] == "404 Not Found")
				{
					return System.Text.Encoding.UTF8.GetBytes(
						ex.Response.Headers["status"] + Environment.NewLine
						   + "URI: " + uri + Environment.NewLine
						   + Environment.NewLine
						   + "Subproject link?"
					);
				}

				// ReSharper disable AssignNullToNotNullAttribute
				var response = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
				// ReSharper restore AssignNullToNotNullAttribute
				if (response == "Error: blob is too big")
					return System.Text.Encoding.UTF8.GetBytes(response + Environment.NewLine + "URI: " + uri);

				throw new Exception(response, ex);
			}
		}

		static string GitHubApiGetString(string uri)
		{
			return System.Text.Encoding.UTF8.GetString(GitHubApiGetBinary(uri));
		}
	}

	// Classes used for JSON deserialisation
	// ReSharper disable InconsistentNaming

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

	// ReSharper restore InconsistentNaming
}
