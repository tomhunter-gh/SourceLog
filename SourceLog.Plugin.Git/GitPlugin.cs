using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LibGit2Sharp;
using SourceLog.Interface;

namespace SourceLog.Plugin.Git
{
	public class GitPlugin : Interface.Plugin
	{
		protected override void CheckForNewLogEntriesImpl()
		{
			string directory;
			string remote;
			string branch;
			GetGitSettings(out directory, out remote, out branch);

			using (var repo = new Repository(directory))
			{
				repo.Fetch(remote);

				foreach (var commit in repo.Branches[remote + "/" + branch].Commits
					.Where(c => c.Committer.When > MaxDateTimeRetrieved)
					.Take(30)
					.OrderBy(c => c.Committer.When))
				{
					ProcessLogEntry(repo, commit);
				}
			}
		}

		private void ProcessLogEntry(IRepository repo, Commit commit)
		{
			var logEntryDto = new LogEntryDto
				{
					Revision = commit.Sha.Substring(0, 7),
					Author = commit.Author.Name,
					CommittedDate = commit.Committer.When.DateTime,
					Message = commit.Message,
					ChangedFiles = new List<ChangedFileDto>()
				};

			foreach (var change in repo.Diff.Compare(commit.Parents.First().Tree, commit.Tree))
			{
				// For GitLinks there isn't really a file to compare
				// (actually I think there is but it comes in a separate change)
				// See for example: https://github.com/libgit2/libgit2sharp/commit/a2efc1a4d433b9e3056b17645c8c1f146fcceecb
				if (change.Mode == Mode.GitLink)
					continue;

				var changeFileDto = new ChangedFileDto
					{
						FileName = change.Path,
						ChangeType = GitChangeStatusToChangeType(change.Status)
					};

				switch (changeFileDto.ChangeType)
				{
					case ChangeType.Added:
						changeFileDto.OldVersion = String.Empty;
						changeFileDto.NewVersion = GetNewVersion(commit, change);
						break;
					case ChangeType.Deleted:
						changeFileDto.OldVersion = GetOldVersion(commit, change);
						changeFileDto.NewVersion = String.Empty;
						break;
					default:
						changeFileDto.OldVersion = GetOldVersion(commit, change);
						changeFileDto.NewVersion = GetNewVersion(commit, change);
						break;
				}

				logEntryDto.ChangedFiles.Add(changeFileDto);
			}

			var args = new NewLogEntryEventArgs { LogEntry = logEntryDto };
			OnNewLogEntry(args);
			MaxDateTimeRetrieved = logEntryDto.CommittedDate;
		}

		private void GetGitSettings(out string directory, out string remote, out string branch)
		{
			var settingsXml = XDocument.Parse(SettingsXml);
			// ReSharper disable PossibleNullReferenceException
			directory = settingsXml.Root.Element("Directory").Value;
			remote = settingsXml.Root.Element("Remote").Value;
			branch = settingsXml.Root.Element("Branch").Value;
			// ReSharper restore PossibleNullReferenceException
		}

		private static string GetOldVersion(Commit commit, TreeEntryChanges change)
		{
			return Encoding.UTF8.GetString(((Blob)commit.Parents.First()[change.OldPath].Target).Content);
		}

		private static string GetNewVersion(Commit commit, TreeEntryChanges change)
		{
			return Encoding.UTF8.GetString(((Blob)commit[change.Path].Target).Content);
		}

		private static ChangeType GitChangeStatusToChangeType(ChangeKind changeKind)
		{
			switch (changeKind)
			{
				case ChangeKind.Added:
					return ChangeType.Added;
				case ChangeKind.Deleted:
					return ChangeType.Deleted;
				case ChangeKind.Modified:
					return ChangeType.Modified;
				case ChangeKind.Renamed:
					return ChangeType.Moved;
				case ChangeKind.Copied:
					return ChangeType.Copied;
				case ChangeKind.Untracked:
					return ChangeType.Deleted;
				//case ChangeKind.Unmodified:
				//    return ChangeType.Modified;
				//case ChangeKind.Ignored:
				//    return ChangeType.Modified;
				default:
					return ChangeType.Modified;
			}
		}
	}
}
