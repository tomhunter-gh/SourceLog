using System;
using System.Windows.Input;
using SourceLog.Model;

namespace SourceLog
{
	public class MarkAsReadCommand : ICommand
	{
		public void Execute(object parameter)
		{
			var readItem = parameter as LogEntry;
			if (readItem != null)
				if (!readItem.Read)
				{
					readItem.Read = true;
					readItem.MarkAsReadAndSave();
				}
		}

		public bool CanExecute(object parameter)
		{
			return (parameter as LogEntry) != null;
		}

		public event EventHandler CanExecuteChanged;
	}
}
