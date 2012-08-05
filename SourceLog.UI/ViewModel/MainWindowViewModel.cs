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
		private readonly LogSubscriptionManager _logSubscriptionManager;

		public List<LogSubscription> LogSubscriptions
		{
			get { return _logSubscriptionManager.LogSubscriptions; }
		}

		private LogSubscription _selectedLogSubscription;
		public LogSubscription SelectedLogSubscription
		{
			get { return _selectedLogSubscription; }
			set
			{
				_selectedLogSubscription = value;
				RaisePropertyChanged("Log");
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
				_selectedLogEntry = value;
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

		public MainWindowViewModel()
		{
			_logSubscriptionManager = new LogSubscriptionManager();
			SelectedLogSubscription = _logSubscriptionManager.LogSubscriptions.FirstOrDefault();
		}

		public void MarkEntryRead(LogEntry readItem)
		{
			if (!readItem.Read)
			{
				readItem.Read = true;
				LogSubscriptionManager.SaveChanges();
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
	}
}
