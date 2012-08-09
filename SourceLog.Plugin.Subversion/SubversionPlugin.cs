using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SharpSvn;
using SourceLog.Interface;
using SourceLog.Model;

namespace SourceLog.Subversion
{
	public class SubversionPlugin : ILogProvider<ChangedFile>
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
					using (var svnClient = new SvnClient())
					{
						var uri = new Uri(SettingsXml);
						Collection<SvnLogEventArgs> svnLogEntries;
						if (svnClient.GetLog(uri, out svnLogEntries))
						{
							var q = svnLogEntries
								.Where(e => e.Time > MaxDateTimeRetrieved)
								//.Where(e => e.Time.Year == 2012)
								//.Where(e => e.Revision >= 56759)
								.OrderBy(e => e.Time);
							foreach (var svnLogEntry in q)
							{
								var revision = svnLogEntry.Revision;
								Debug.WriteLine(" Creating LogEntry for revision " + revision);
								var logEntry = new LogEntry
									{
										Author = svnLogEntry.Author,
										CommittedDate = svnLogEntry.Time,
										Message = svnLogEntry.LogMessage,
										Revision = revision.ToString(),
										ChangedFiles = new List<ChangedFile>()
									};

								ProcessChangedPaths(svnLogEntry, revision, logEntry);

								var args = new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry };
								NewLogEntry(this, args);
							}
							MaxDateTimeRetrieved = svnLogEntries.Max(x => x.Time);
						}
					}
				}
				finally
				{
					Monitor.Exit(_lockObject);
				}
			}
		}

		private void ProcessChangedPaths(SvnLoggingEventArgs svnLogEntry, long revision, ILogEntry<ChangedFile> logEntry)
		{
			svnLogEntry.ChangedPaths.AsParallel().WithDegreeOfParallelism(10).ForAll(changedPath =>
			{
				//var debugStartTime = DateTime.Now;
				Debug.WriteLine("  Processing path " + changedPath.Path);
				using (var parallelSvnClient = new SvnClient())
				{
					var changedFile = new ChangedFile { FileName = changedPath.Path };

					var nodeKind = changedPath.NodeKind;
					if (nodeKind == SvnNodeKind.Unknown)
					{
						// Use GetInfo to get the NodeKind
						SvnInfoEventArgs svnInfo;
						parallelSvnClient.GetInfo(
							new SvnUriTarget(
								SettingsXml + changedPath.Path,
							// If the file is deleted then using revision causes an exception
								(changedPath.Action == SvnChangeAction.Delete ? revision - 1 : revision)
							),
							out svnInfo);
						nodeKind = svnInfo.NodeKind;
					}

					if (nodeKind != SvnNodeKind.File)
					{
						changedFile.OldVersion = String.Empty;
						changedFile.NewVersion = String.Empty;
					}
					else
					{
						if (changedPath.Action == SvnChangeAction.Modify || changedPath.Action == SvnChangeAction.Delete)
						{
							// Use GetInfo to get the last change revision
							SvnInfoEventArgs previousRevisionInfo;
							var previousRevisionUri = new SvnUriTarget(SettingsXml + changedPath.Path, revision - 1);
							try
							{
								// For some reason we seem to get an exception with a message stating that
								// a previous version doesn't exist for a Modify action.  I'm not sure how
								// you can have a modify without a previous version (surely everything
								// starts with an add..?
								parallelSvnClient.GetInfo(previousRevisionUri, out previousRevisionInfo);
								changedFile.OldVersion = ReadFileVersion(
									parallelSvnClient, SettingsXml + changedPath.Path,
									previousRevisionInfo.LastChangeRevision);
							}
							catch (SvnRepositoryIOException e)
							{
								Debug.WriteLine(e.Message);
								changedFile.OldVersion = String.Empty;
							}
						}
						else
						{
							changedFile.OldVersion = String.Empty;
						}

						if (changedPath.Action == SvnChangeAction.Modify || changedPath.Action == SvnChangeAction.Add)
						{
							changedFile.NewVersion = ReadFileVersion(parallelSvnClient, SettingsXml + changedPath.Path, revision);
						}
						else
						{
							changedFile.NewVersion = String.Empty;
						}
					}

					switch (changedPath.Action)
					{
						case SvnChangeAction.Add :
							changedFile.ChangeType = ChangeType.Added;
							break;
						case SvnChangeAction.Delete :
							changedFile.ChangeType = ChangeType.Deleted;
							break;
						default :
							changedFile.ChangeType = ChangeType.Modified;
							break;
					}

					logEntry.ChangedFiles.Add(changedFile);
				}

				//Debug.WriteLine("Processed (" + new TimeSpan(DateTime.Now.Ticks - debugStartTime.Ticks) + ") " + changedPath.Path);
			});
		}

		private static string ReadFileVersion(SvnClient svnClient, string uriString, long revision)
		{
			using (var versionStream = new MemoryStream())
			{
				var versionTarget = new SvnUriTarget(uriString, revision);
				svnClient.Write(versionTarget, versionStream);
				var streamReader = new StreamReader(versionStream);
				versionStream.Position = 0;
				return streamReader.ReadToEnd();
			}
		}

		public event NewLogEntryEventHandler<ChangedFile> NewLogEntry;
	}
}
