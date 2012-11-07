using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using SourceLog.Interface;
using System.Data.Entity;
using System.Linq;

namespace SourceLog.Model
{
	public class LogEntry : ILogEntry<ChangedFile>, INotifyPropertyChanged
	{
		public int LogEntryId { get; set; }
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
						db.LogEntries.Find(LogEntryId).Read = true;
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
	}
}
