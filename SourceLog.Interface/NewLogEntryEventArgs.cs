using System;

namespace SourceLog.Interface
{
	public class NewLogEntryEventArgs<T> : EventArgs where T : IChangedFile
	{
		public ILogEntry<T> LogEntry { get; set; }
	}
}
