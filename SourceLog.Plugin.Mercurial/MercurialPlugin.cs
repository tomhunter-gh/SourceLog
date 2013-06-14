using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Mercurial;
using Microsoft.Win32;
using SourceLog.Interface;

namespace SourceLog.Plugin.Mercurial
{
	public class MercurialPlugin : Interface.Plugin
	{
		protected override void CheckForNewLogEntriesImpl()
		{
			string directory;			
            GetMercurialSettings(out directory);

            if (!Client.CouldLocateClient)
            {
                string InstallPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\TortoiseHg", "", @"C:\Program Files\TortoiseHg");
                Client.SetClientPath(InstallPath);
            }

            Repository repo = new Repository(directory);

            repo.Pull();
            
            foreach (var commit in repo.Log((new LogCommand()).WithAdditionalArgument("-l 30"))
                    .Where(c => c.Timestamp > MaxDateTimeRetrieved)
                    .Take(30)
                    .OrderBy(c => c.Timestamp))
            {
                    ProcessLogEntry(repo, commit);                    
            }
		}

		private void ProcessLogEntry(Repository repo, Changeset commit)
		{
			var logEntryDto = new LogEntryDto
				{
					Revision = commit.RevisionNumber.ToString(),
					Author = commit.AuthorName,
					CommittedDate = commit.Timestamp,
					Message = "("+commit.Branch + ") " +  commit.CommitMessage,
					ChangedFiles = new List<ChangedFileDto>()
				};

            

			foreach (ChangesetPathAction change in commit.PathActions)
			{				
				var changeFileDto = new ChangedFileDto
					{
						FileName = change.Path,
						ChangeType = MercurialChangeStatusToChangeType(change.Action)
					};
                


				switch (changeFileDto.ChangeType)
				{
					case ChangeType.Added:
						changeFileDto.OldVersion = new byte[0];
                        changeFileDto.NewVersion = GetFile(repo, commit.RevisionNumber, change.Path);
						break;
					case ChangeType.Deleted:
                        changeFileDto.OldVersion = GetFile(repo, commit.LeftParentRevision, change.Path);
						changeFileDto.NewVersion = new byte[0];
						break;
					default:
                        changeFileDto.OldVersion = GetFile(repo, commit.LeftParentRevision, change.Path);
                        changeFileDto.NewVersion = GetFile(repo, commit.RevisionNumber, change.Path);
						break;
				}

				logEntryDto.ChangedFiles.Add(changeFileDto);
			}

			var args = new NewLogEntryEventArgs { LogEntry = logEntryDto };
			OnNewLogEntry(args);
			MaxDateTimeRetrieved = logEntryDto.CommittedDate;
		}

		private void GetMercurialSettings(out string directory)
		{
			var settingsXml = XDocument.Parse(SettingsXml);
			// ReSharper disable PossibleNullReferenceException
			directory = settingsXml.Root.Element("Directory").Value;
			// ReSharper restore PossibleNullReferenceException
		}

        private static byte[] GetFile(Repository repo, int rev, string path)
		{
            string file = repo.Cat(path,(new CatCommand()).WithAdditionalArgument("-r "+rev));
            return System.Text.Encoding.UTF8.GetBytes(file);
		}        
        
        private static ChangeType MercurialChangeStatusToChangeType(ChangesetPathActionType changeKind)
		{
			switch (changeKind)
			{
                case ChangesetPathActionType.Add:
					return ChangeType.Added;
                case ChangesetPathActionType.Remove:
					return ChangeType.Deleted;
                case ChangesetPathActionType.Modify:
					return ChangeType.Modified;
				default:
					return ChangeType.Modified;
			}
		}
	}
}
