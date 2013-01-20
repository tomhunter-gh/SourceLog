using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SourceLog.Model;

namespace SourceLog.ViewModel
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public static readonly LogSubscriptionManager LogSubscriptionManager = new LogSubscriptionManager();

		public ObservableCollection<LogSubscription> LogSubscriptions
		{
			get { return LogSubscriptionManager.LogSubscriptions; }
		}

		private LogSubscription _selectedLogSubscription;
		public LogSubscription SelectedLogSubscription
		{
			get { return _selectedLogSubscription; }
			set
			{
				_selectedLogSubscription = value;
				RaisePropertyChanged("Log");
				
				if (value != null)
				{
					SelectedLogEntry = value.Log.Count > 0 ? value.Log.Last() : null;
					SelectedChangedFile = SelectedLogEntry != null ? SelectedLogEntry.ChangedFiles.FirstOrDefault() : null;
				}
				else
				{
					SelectedLogEntry = null;
					SelectedChangedFile = null;
				}
			}
		}

		public ObservableCollection<LogEntry> Log
		{
			get
			{
				if (_selectedLogSubscription != null)
				{
					return _selectedLogSubscription.Log;
				}
				return null;
			}
		}

		private LogEntry _selectedLogEntry;
		public LogEntry SelectedLogEntry
		{
			get
			{
				return _selectedLogEntry;
			}
			set
			{
				if (_selectedLogEntry != null)
					_selectedLogEntry.UnloadChangedFiles();

				_selectedLogEntry = value;

				if (_selectedLogEntry != null)
					_selectedLogEntry.LoadChangedFiles();

				RaisePropertyChanged("SelectedLogEntryChangedFiles");
				RaisePropertyChanged("SelectedLogEntryMessage");
			}
		}

		public string SelectedLogEntryMessage
		{
			get
			{
				if (_selectedLogEntry != null)
				{
					return _selectedLogEntry.Message;
				}
				return String.Empty;
			}
		}

		public List<ChangedFile> SelectedLogEntryChangedFiles
		{
			get
			{
				if (SelectedLogEntry != null)
				{
					return SelectedLogEntry.ChangedFiles;
				}
				return null;
			}
		}

		private ChangedFile _selectedChangedFile;
		public ChangedFile SelectedChangedFile
		{
			get { return _selectedChangedFile; }
			set
			{
				_selectedChangedFile = value;
				RaisePropertyChanged("SelectedChangedFile");
			}
		}


		public MainWindowViewModel()
		{
			SelectedLogSubscription = LogSubscriptionManager.LogSubscriptions.FirstOrDefault();
			LogSubscriptionManager.NewLogEntry += (o, e) => NewLogEntry(o, e);
		}

		public void MarkEntryRead(LogEntry readItem)
		{
			if (!readItem.Read)
			{
				readItem.Read = true;
				readItem.MarkAsReadAndSave();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event NewLogEntryInfoEventHandler NewLogEntry;

		public void DeleteSubscription(LogSubscription logSubscription)
		{
			LogSubscriptionManager.DeleteSubscription(logSubscription);
		}
	}
}
