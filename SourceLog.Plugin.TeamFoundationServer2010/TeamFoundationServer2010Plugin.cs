using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceLog.Interface;
using SourceLog.Model;
using System.Threading;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using TFS = Microsoft.TeamFoundation.VersionControl.Client;
using System.Diagnostics;
using System.IO;

namespace SourceLog.Plugin.TeamFoundationServer2010
{
	public class TeamFoundationServer2010Plugin : ILogProvider<ChangedFile>
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
					var settingsArray = SettingsXml.Split('$');
					string uri = settingsArray[0];
					string sourceControlPath = "$" + settingsArray[1];

					Uri tfsUri = new Uri(uri);
					var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);

					var vcs = projectCollection.GetService<VersionControlServer>();
					var history = vcs.QueryHistory(
						path: sourceControlPath,
						version: VersionSpec.Latest,
						deletionId: 0,
						recursion: RecursionType.Full,
						user: null,
						versionFrom: null,
						versionTo: null,
						maxCount: 100,
						includeChanges: true,
						slotMode: false
					).Cast<Changeset>();

					foreach (var changeset in
						history.Where(c => c.CreationDate > MaxDateTimeRetrieved).OrderBy(c => c.CreationDate))
					{
						var changesetId = changeset.ChangesetId;
						Debug.WriteLine(" [TFS]Creating LogEntry for Changeset " + changesetId);

						var logEntry = new LogEntry
						{
							Author = changeset.Committer,
							CommittedDate = changeset.CreationDate,
							Message = changeset.Comment,
							Revision = changesetId.ToString(),
							ChangedFiles = new List<ChangedFile>()
						};

						foreach (var change in changeset.Changes)
						{
							var changedFile = new ChangedFile { FileName = change.Item.ServerItem };
							if (change.Item.ItemType != ItemType.File)
							{
								changedFile.OldVersion = String.Empty;
								changedFile.NewVersion = String.Empty;
								continue;
							}

							using (var streamreader = new StreamReader(change.Item.DownloadFile()))
							{
								changedFile.NewVersion = streamreader.ReadToEnd();
							}

							var previousVersion = vcs.GetItem(change.Item.ItemId, changesetId - 1, true);
							if (previousVersion != null)
							{
								using (var previousVersionStreamreader = new StreamReader(previousVersion.DownloadFile()))
								{
									changedFile.OldVersion = previousVersionStreamreader.ReadToEnd();
								}
							}
							else
							{
								changedFile.OldVersion = String.Empty;
							}

							SetChangeType(changedFile, change);

							logEntry.ChangedFiles.Add(changedFile);
						}

						var args = new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry };
						NewLogEntry(this, args);
					}
					MaxDateTimeRetrieved = history.Max(c => c.CreationDate);
				}
				finally
				{
					Monitor.Exit(_lockObject);
				}
			}
		}

		private void SetChangeType(ChangedFile changedFile, Change change)
		{
			if (change.ChangeType.HasFlag(TFS.ChangeType.Add))
				changedFile.ChangeType = Interface.ChangeType.Added;
			else if (change.ChangeType.HasFlag(TFS.ChangeType.Delete))
				changedFile.ChangeType = Interface.ChangeType.Deleted;
			//if (change.ChangeType.HasFlag(TFS.ChangeType.Edit))
			else
				changedFile.ChangeType = Interface.ChangeType.Modified;
		}

		public event NewLogEntryEventHandler<ChangedFile> NewLogEntry;
	}
}
