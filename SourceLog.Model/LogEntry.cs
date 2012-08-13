using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
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
					//db.Entry(this).Entity.Read = true;
					// TODO: there must be a better way..
					db.LogSubscriptions.ToList().ForEach(s => s.Log.Where(e => e.LogEntryId == LogEntryId).ToList().ForEach(e => e.Read = true));
					//db.Entry(this).Entity._read = true;
					//((IObjectContextAdapter)db).ObjectContext.Attach(db.Entry(this));
					db.SaveChanges();
				});
			
		}
	}
}
