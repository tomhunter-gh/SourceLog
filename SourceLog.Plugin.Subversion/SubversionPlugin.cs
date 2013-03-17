using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SharpSvn;
using SourceLog.Interface;

namespace SourceLog.Plugin.Subversion
{
	public class SubversionPlugin : Interface.Plugin
	{
		// Need a static lock object as SharpSVN is not thread-safe
		static readonly new Object LockObject = new Object();

		protected override void CheckForNewLogEntriesImpl()
		{
			if (!Monitor.TryEnter(LockObject))
				return;

			try
			{
				using (var svnClient = new SvnClient())
				{
					var uri = new Uri(SettingsXml);
					Collection<SvnLogEventArgs> svnLogEntries;
					if (svnClient.GetLog(uri, new SvnLogArgs { Limit = 30 }, out svnLogEntries))
					{
						var q = svnLogEntries
							.Where(e => e.Time.PrecisionFix() > MaxDateTimeRetrieved)
							.OrderBy(e => e.Time);
						foreach (var svnLogEntry in q)
						{
							var revision = svnLogEntry.Revision;
							Logger.Write(new LogEntry
								{
									Message = "Creating LogEntryDto for revision " + revision,
									Categories = { "Plugin." + GetType().Name }
								});
							var logEntry = new LogEntryDto
								{
									Author = svnLogEntry.Author,
									CommittedDate = svnLogEntry.Time,
									Message = svnLogEntry.LogMessage,
									Revision = revision.ToString(CultureInfo.InvariantCulture),
									ChangedFiles = new List<ChangedFileDto>()
								};

							ProcessChangedPaths(svnLogEntry, revision, logEntry);

							var args = new NewLogEntryEventArgs { LogEntry = logEntry };
							OnNewLogEntry(args);
						}
						MaxDateTimeRetrieved = svnLogEntries.Max(x => x.Time).PrecisionFix();
					}
				}
			}
			finally
			{
				Monitor.Exit(LockObject);
			}
		}

		private void ProcessChangedPaths(SvnLoggingEventArgs svnLogEntry, long revision, LogEntryDto logEntry)
		{
			svnLogEntry.ChangedPaths.AsParallel().WithDegreeOfParallelism(1).ForAll(changedPath =>
			{
				Logger.Write(new LogEntry { Message = "Processing path " + changedPath.Path, Categories = { "Plugin.Subversion" } });
				using (var parallelSvnClient = new SvnClient())
				{
					var changedFile = new ChangedFileDto { FileName = changedPath.Path };

					var nodeKind = changedPath.NodeKind;
					if (nodeKind == SvnNodeKind.Unknown)
					{
						// Use GetInfo to get the NodeKind
						SvnInfoEventArgs svnInfo;
						try
						{
							parallelSvnClient.GetInfo(
								new SvnUriTarget(
									SettingsXml + changedPath.Path,
								// If the file is deleted then using revision causes an exception
									(changedPath.Action == SvnChangeAction.Delete ? revision - 1 : revision)
								),
								out svnInfo);
							nodeKind = svnInfo.NodeKind;
						}
						catch (SvnRepositoryIOException svnRepositoryIoException)
						{
							Logger.Write(new LogEntry
								{
									Message = svnRepositoryIoException.ToString(),
									Categories = { "Plugin.Subversion" },
									Severity = TraceEventType.Warning
								});
						}

					}

					if (nodeKind != SvnNodeKind.File)
					{
						changedFile.OldVersion = new byte[0];
						changedFile.NewVersion = new byte[0];
					}
					else
					{
						if (changedPath.Action == SvnChangeAction.Modify || changedPath.Action == SvnChangeAction.Delete)
						{
							// Use GetInfo to get the last change revision
							var previousRevisionUri = new SvnUriTarget(SettingsXml + changedPath.Path, revision - 1);
							try
							{
								// For some reason we seem to get an exception with a message stating that
								// a previous version doesn't exist for a Modify action.  I'm not sure how
								// you can have a modify without a previous version (surely everything
								// starts with an add..?
								SvnInfoEventArgs previousRevisionInfo;
								parallelSvnClient.GetInfo(previousRevisionUri, out previousRevisionInfo);
								changedFile.OldVersion = ReadFileVersion(
									parallelSvnClient, SettingsXml + changedPath.Path,
									previousRevisionInfo.LastChangeRevision);
							}
							catch (SvnRepositoryIOException e)
							{
								Logger.Write(new LogEntry { Message = "SvnRepositoryIOException: " + e, Categories = { "Plugin.Subversion" }, Severity = TraceEventType.Error });
								changedFile.OldVersion = new byte[0];
							}
							catch (SvnFileSystemException ex)
							{
								// http://stackoverflow.com/questions/12939642/sharpsvn-getinfo-lastchangerevision-is-wrong
								Logger.Write(new LogEntry { Message = "SvnFileSystemException: " + ex, Categories = { "Plugin.Subversion" }, Severity = TraceEventType.Warning });
								changedFile.OldVersion = new byte[0];
							}
						}
						else
						{
							changedFile.OldVersion = new byte[0];
						}

						if (changedPath.Action == SvnChangeAction.Modify || changedPath.Action == SvnChangeAction.Add)
						{
							changedFile.NewVersion = ReadFileVersion(parallelSvnClient, SettingsXml + changedPath.Path, revision);
						}
						else
						{
							changedFile.NewVersion = new byte[0];
						}
					}

					switch (changedPath.Action)
					{
						case SvnChangeAction.Add:
							changedFile.ChangeType = ChangeType.Added;
							break;
						case SvnChangeAction.Delete:
							changedFile.ChangeType = ChangeType.Deleted;
							break;
						default:
							changedFile.ChangeType = ChangeType.Modified;
							break;
					}

					logEntry.ChangedFiles.Add(changedFile);
				}
			});
		}

		private static byte[] ReadFileVersion(SvnClient svnClient, string uriString, long revision)
		{
			using (var versionStream = new MemoryStream())
			{
				var versionTarget = new SvnUriTarget(uriString, revision);
				svnClient.Write(versionTarget, versionStream);
				return versionStream.ToArray();
			}
		}
	}
}
