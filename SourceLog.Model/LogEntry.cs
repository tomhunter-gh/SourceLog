using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;
using System.Data.Entity;
using System.Linq;

namespace SourceLog.Model
{
	public class LogEntry : INotifyPropertyChanged
	{
		public int LogEntryId { get; set; }
		[Required]
		public LogSubscription LogSubscription { get; set; }
		public string Revision { get; set; }
		public DateTime CommittedDate { get; set; }
		public string Message { get; set; }
		public string Author { get; set; }

		private bool _read;
		public bool Read
		{
			get { return _read; }
			set
			{
				_read = value;
				OnPropertyChanged("Read");
			}
		}

		public List<ChangedFile> ChangedFiles { get; set; }

		public LogEntry() { }

		public LogEntry(LogEntryDto dto)
		{
			Revision = dto.Revision;
			CommittedDate = dto.CommittedDate;
			Message = dto.Message;
			Author = dto.Author;

			ChangedFiles = new List<ChangedFile>();

			dto.ChangedFiles.ForEach(changedFileDto => ChangedFiles.Add(new ChangedFile(changedFileDto)));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public void MarkAsReadAndSave()
		{
			Task.Factory.StartNew(() =>
				{
					using (var db = new SourceLogContext())
					{
						//var logEntry = db.LogEntries.Find(LogEntryId);
						var logEntry = db.LogEntries.Where(le => le.LogEntryId == LogEntryId).Include(le => le.LogSubscription).Single();
						db.LogEntries.Attach(logEntry);
						logEntry.Read = true;
						db.SaveChanges();
					}
				});
		}

		public void UnloadChangedFiles()
		{
			ChangedFiles = null;
		}

		public void LoadChangedFiles()
		{
			if (ChangedFiles == null)
			{
				using (var db = new SourceLogContext())
				{
					ChangedFiles = db.LogEntries.Where(x => x.LogEntryId == LogEntryId).Include(x => x.ChangedFiles).Single().ChangedFiles;
				}
			}
		}

		public void GenerateFlowDocuments()
		{
			ChangedFiles.AsParallel().ForAll(changedFile =>
			{
				Logger.Write(new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
				{
					Message = "GeneratingFlowDocuments - File: " + changedFile.FileName,
					Severity = TraceEventType.Information
				});

				var diff = new SideBySideFlowDocumentDiffGenerator(changedFile.OldVersion, changedFile.NewVersion);
				changedFile.LeftFlowDocument = diff.LeftDocument;
				changedFile.RightFlowDocument = diff.RightDocument;
				changedFile.FirstModifiedLineVerticalOffset = diff.FirstModifiedLineVerticalOffset;
			});
		}

		public override bool Equals(object obj)
		{
			var logEntry = obj as LogEntry;
			if (logEntry == null)
				return false;
			return LogEntryId.Equals(logEntry.LogEntryId);
		}

		public override int GetHashCode()
		{
			return LogEntryId.GetHashCode();
		}
	}
}
