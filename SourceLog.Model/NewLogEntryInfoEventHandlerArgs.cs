namespace SourceLog.Model
{
	public class NewLogEntryInfoEventHandlerArgs
	{
		public string LogSubscriptionName { get; set; }
		public string Author { get; set; }
		public string Message { get; set; }
	}
}