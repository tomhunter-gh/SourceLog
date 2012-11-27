using System;

namespace SourceLog.Interface
{
	public class NewLogEntryEventArgs : EventArgs
	{
		public LogEntryDto LogEntry { get; set; }
	}
}