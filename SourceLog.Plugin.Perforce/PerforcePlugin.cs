using System;
using System.Collections.Generic;
using System.Linq;
using SourceLog.Interface;
using System.Threading;
using P4COM;
using System.IO;
using System.Diagnostics;

namespace SourceLog.Plugin.Perforce
{
	public class PerforcePlugin : ILogProvider
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
					List<LogEntryDto> logEntries = new List<LogEntryDto>();
					P4COM.p4 p4 = new P4COM.p4();
					p4.Connect();
					var p4changes = p4.run("changes -t -l -s submitted -m 30 \"" + SettingsXml + "\"");
					//p4.Disconnect();
					foreach (string changeset in p4changes)
					{
						var logEntry = PerforceLogParser.Parse(changeset);
						if (logEntry.CommittedDate <= MaxDateTimeRetrieved)
							continue;
						logEntries.Add(logEntry);
					}

					foreach (var logEntry in logEntries.OrderBy(le => le.CommittedDate))
					{
						logEntry.ChangedFiles = new List<ChangedFileDto>();
						ChangedFileDto changedFile;

						// grab changed files
						var p4files = p4.run("files @=" + logEntry.Revision);
						foreach (string file in p4files)
						{
							changedFile = PerforceLogParser.ParseP4File(file);
							if (changedFile.ChangeType == ChangeType.Added
								|| changedFile.ChangeType == ChangeType.Copied
								|| changedFile.ChangeType == ChangeType.Modified
								|| changedFile.ChangeType == ChangeType.Moved)
							{
								if (changedFile.ChangeType == ChangeType.Copied
									|| changedFile.ChangeType == ChangeType.Moved)
								{
									// Add new path to top of NewVersion
									changedFile.NewVersion = String.Empty;
								}
								else
								{
									changedFile.NewVersion = String.Empty;
								}
								
								// Always create a new p4 object otherwise TempFilename doesn't always update
								p4 p4Print = new p4();
								p4Print.Connect();
								p4Print.run("print \"" + changedFile.FileName + "@" + logEntry.Revision + "\"");
								string tempFilename = ((dynamic)p4Print).TempFilename;
								Debug.WriteLine(" " + DateTime.Now.ToString("HH:mm:ss.fff tt") + " - [PerforcePlugin] changedFile.FileName: " + changedFile.FileName + "; tempFilename: " + tempFilename);
								using (var streamReader = new StreamReader(tempFilename))
								{
									changedFile.NewVersion += streamReader.ReadToEnd();
								}
								p4Print.Disconnect();
								File.SetAttributes(tempFilename, FileAttributes.Normal);
								File.Delete(tempFilename);
							}
							else
							{
								changedFile.NewVersion = String.Empty;
							}

							if (changedFile.ChangeType == ChangeType.Deleted
								|| changedFile.ChangeType == ChangeType.Modified)
							{
								p4 p4Print = new p4();
								p4Print.Connect();
								p4Print.run("print \"" + changedFile.FileName + "@" + (Int32.Parse(logEntry.Revision) - 1) + "\"");
								string tempFilename = ((dynamic)p4Print).TempFilename;
								using (var streamReader = new StreamReader(tempFilename))
								{
									changedFile.OldVersion = streamReader.ReadToEnd();
								}
								p4Print.Disconnect();
								File.SetAttributes(tempFilename, FileAttributes.Normal);
								File.Delete(tempFilename);
							}
							else if (changedFile.ChangeType == ChangeType.Copied
								|| changedFile.ChangeType == ChangeType.Moved)
							{
								// TODO: get previous path and contents and put both in OldVersion
								changedFile.OldVersion = String.Empty;
							}
							else
							{
								changedFile.OldVersion = String.Empty;
							}

							

							logEntry.ChangedFiles.Add(changedFile);
						}

						var args = new NewLogEntryEventArgs { LogEntry = logEntry };
						NewLogEntry(this, args);
					}

					p4.Disconnect();
					if (logEntries.Count > 0)
					{
						MaxDateTimeRetrieved = logEntries.Max(x => x.CommittedDate);
					}
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

		public event NewLogEntryEventHandler NewLogEntry;
		public event LogProviderExceptionEventHandler LogProviderException;
	}
}
