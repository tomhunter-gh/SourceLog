using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;
using System.Threading;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using TFS = Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Xml.Linq;

namespace SourceLog.Plugin.TeamFoundationServer2010
{
	public class TeamFoundationServer2010Plugin : ILogProvider
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
                    var settingsXml = XDocument.Parse(SettingsXml);
                    var collectionUrl = settingsXml.Root.Element("CollectionURL").Value;
                    var sourceLocation = settingsXml.Root.Element("SourceLocation").Value;
                    
                    //var settingsArray = SettingsXml.Split('$');
					//string uri = settingsArray[0];
					//string sourceControlPath = "$" + settingsArray[1];

                    var tfsUri = new Uri(collectionUrl);
					var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);

					var vcs = projectCollection.GetService<VersionControlServer>();
					var history = vcs.QueryHistory(
                        path: sourceLocation,
						version: VersionSpec.Latest,
						deletionId: 0,
						recursion: RecursionType.Full,
						user: null,
						versionFrom: null,
						versionTo: null,
						maxCount: 30,
						includeChanges: true,
						slotMode: false
					)
					.Cast<Changeset>()
					.ToList();

					foreach (var changeset in
						history.Where(c => c.CreationDate > MaxDateTimeRetrieved).OrderBy(c => c.CreationDate))
					{
						var changesetId = changeset.ChangesetId;
						Logger.Write(new LogEntry { Message = "Creating LogEntry for Changeset " + changesetId, Categories = { "Plugin.TFS2010" } });

						var logEntry = new LogEntryDto
						{
							Author = changeset.Committer,
							CommittedDate = changeset.CreationDate,
							Message = changeset.Comment,
							Revision = changesetId.ToString(CultureInfo.InvariantCulture),
							ChangedFiles = new List<ChangedFileDto>()
						};

						foreach (var change in changeset.Changes)
						{
							var changedFile = new ChangedFileDto { FileName = change.Item.ServerItem };
							if (change.Item.ItemType != ItemType.File)
							{
								changedFile.OldVersion = String.Empty;
								changedFile.NewVersion = String.Empty;
								continue;
							}

							if (change.ChangeType.HasFlag(TFS.ChangeType.Delete))
							{
								changedFile.NewVersion = String.Empty;
							}
							else
							{
								using (var streamreader = new StreamReader(change.Item.DownloadFile()))
								{
									changedFile.NewVersion = streamreader.ReadToEnd();
								}
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

						var args = new NewLogEntryEventArgs { LogEntry = logEntry };
						NewLogEntry(this, args);
					}
					MaxDateTimeRetrieved = history.Max(c => c.CreationDate);
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

		private static void SetChangeType(ChangedFileDto changedFile, Change change)
		{
			if (change.ChangeType.HasFlag(TFS.ChangeType.Add))
				changedFile.ChangeType = Interface.ChangeType.Added;
			else if (change.ChangeType.HasFlag(TFS.ChangeType.Delete))
				changedFile.ChangeType = Interface.ChangeType.Deleted;
			else
				changedFile.ChangeType = Interface.ChangeType.Modified;
		}

		public event NewLogEntryEventHandler NewLogEntry;
		public event LogProviderExceptionEventHandler LogProviderException;
	}
}
