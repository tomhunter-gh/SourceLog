namespace SourceLog.Interface
{
	public class ChangedFileDto
	{
		public ChangeType ChangeType { get; set; }
		public string FileName { get; set; }
		public byte[] OldVersion { get; set; }
		public byte[] NewVersion { get; set; }
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
