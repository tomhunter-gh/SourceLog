using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using SourceLog.Interface;

namespace SourceLog.Model
{
	public class LogEntry : ILogEntry<ChangedFile>, INotifyPropertyChanged
	{
		public int LogEntryId
		{
			get;
			set;
		}
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

		public virtual List<ChangedFile> ChangedFiles
		{
			get;
			set;
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
					var db = (SourceLogContext)SourceLogContext.ThreadStaticContext;
					db.Set<LogEntry>().Find(LogEntryId).Read = true;
					db.SaveChanges();
				});
			
		}
	}
}
