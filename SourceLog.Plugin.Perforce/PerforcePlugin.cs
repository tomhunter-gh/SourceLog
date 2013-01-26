using System;
using System.Collections.Generic;
using System.Linq;
using SourceLog.Interface;
using P4COM;
using System.IO;
using System.Reflection;

namespace SourceLog.Plugin.Perforce
{
	public class PerforcePlugin : Interface.Plugin
	{
		protected override void CheckForNewLogEntriesImpl()
		{
			var p4 = new p4();
			p4.Connect();
			var repoPath = SettingsXml;
			if (repoPath.EndsWith(@"/"))
				repoPath += "...";
			var p4Changes = p4.run("changes -t -l -s submitted -m 30 \"" + repoPath + "\"");

			var logEntries = p4Changes.Cast<string>().Select(PerforceLogParser.Parse)
				.Where(logEntry => logEntry.CommittedDate > MaxDateTimeRetrieved).ToList();

			foreach (var logEntry in logEntries.OrderBy(le => le.CommittedDate))
			{
				logEntry.ChangedFiles = new List<ChangedFileDto>();

				// grab changed files
				var p4Files = p4.run("files @=" + logEntry.Revision);
				foreach (string file in p4Files)
				{
					ChangedFileDto changedFile = PerforceLogParser.ParseP4File(file);
					if (changedFile.ChangeType == ChangeType.Added
						|| changedFile.ChangeType == ChangeType.Copied
						|| changedFile.ChangeType == ChangeType.Modified
						|| changedFile.ChangeType == ChangeType.Moved)
					{
						if (changedFile.ChangeType == ChangeType.Copied
							|| changedFile.ChangeType == ChangeType.Moved)
						{
							// TODO: Add new path to top of NewVersion
							changedFile.NewVersion = String.Empty;
						}
						else
						{
							changedFile.NewVersion = String.Empty;
						}

						LoadNewVersion(logEntry, changedFile);
					}
					else
					{
						changedFile.NewVersion = String.Empty;
					}

					if (changedFile.ChangeType == ChangeType.Deleted
						|| changedFile.ChangeType == ChangeType.Modified)
					{
						LoadOldVersion(logEntry, changedFile);
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
				OnNewLogEntry(args);
			}

			p4.Disconnect();
			if (logEntries.Count > 0)
			{
				MaxDateTimeRetrieved = logEntries.Max(x => x.CommittedDate);
			}
		}

		private static void LoadOldVersion(LogEntryDto logEntry, ChangedFileDto changedFile)
		{
			var p4Print = new p4();
			p4Print.Connect();
			p4Print.run("print \"" + changedFile.FileName + "@"
			            + (Int32.Parse(logEntry.Revision) - 1) + "\"");
			string tempFilename = ((dynamic)p4Print).TempFilename;
			using (var streamReader = new StreamReader(tempFilename))
			{
				changedFile.OldVersion = streamReader.ReadToEnd();
			}
			p4Print.Disconnect();
			File.SetAttributes(tempFilename, FileAttributes.Normal);
			File.Delete(tempFilename);
		}

		private static void LoadNewVersion(LogEntryDto logEntry, ChangedFileDto changedFile)
		{
			// Always create a new p4 object otherwise TempFilename doesn't always update
			var p4Print = new p4();
			p4Print.Connect();
			p4Print.run("print \"" + changedFile.FileName + "@" + logEntry.Revision + "\"");
			string tempFilename = ((dynamic)p4Print).TempFilename;

			using (var streamReader = new StreamReader(tempFilename))
			{
				changedFile.NewVersion += streamReader.ReadToEnd();
			}
			p4Print.Disconnect();
			File.SetAttributes(tempFilename, FileAttributes.Normal);
			File.Delete(tempFilename);
		}

		// Manage registration of p4com
		// using static constructor to ensure registration happens only once
		static PerforcePlugin()
		{
			var registrar = new Registrar(
						Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\p4com.dll");
			registrar.RegisterComDLL();
			PluginFinalizer = new Finalizer(registrar);
		}

		// ReSharper disable UnaccessedField.Local
		static readonly Finalizer PluginFinalizer;
		// ReSharper restore UnaccessedField.Local

		sealed class Finalizer
		{
			private readonly Registrar _registrar;
			internal Finalizer(Registrar registrar)
			{
				_registrar = registrar;
			}

			~Finalizer()
			{
				_registrar.Dispose();
			}
		}
	}
}
