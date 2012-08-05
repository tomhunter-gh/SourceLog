using System;

namespace SourceLog.Interface
{
	public class NewLogEntryEventArgs<T> : EventArgs where T : IChangedFile
	{
		public ILogEntry<T> LogEntry { get; set; }
	}

	public delegate void NewLogEntryEventHandler<T>(object sender, NewLogEntryEventArgs<T> e) where T : IChangedFile;

	public interface ILogProvider<T> where T : IChangedFile
	{
		/// <summary>
		/// Represents plugin specific repo connection information
		/// </summary>
		string SettingsXml { get; set; }

		DateTime MaxDateTimeRetrieved { get; set; }

		/// <summary>
		/// Create an interval function that uses uses SettingsXml and MaxDateTimeRetrieved
		/// to check for new log entries.  Calls NewLogEntry for each new entry.
		/// </summary>
		void Initialise();

		event NewLogEntryEventHandler<T> NewLogEntry;
	}
}