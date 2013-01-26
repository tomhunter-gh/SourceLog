namespace SourceLog.Interface
{
	public class ChangedFileDto
	{
		public ChangeType ChangeType { get; set; }
		public string FileName { get; set; }
		public string OldVersion { get; set; }
		public string NewVersion { get; set; }
	}

	public enum ChangeType
	{
		Added,
		Modified,
		Deleted,
		Copied,
		Moved
	}
}
