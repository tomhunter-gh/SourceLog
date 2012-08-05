namespace SourceLog.Interface
{
	public interface IChangedFile
	{
		ChangeType ChangeType { get; set; }
		string FileName { get; set; }
		string OldVersion { get; set; }
		string NewVersion { get; set; }
	}

	public enum ChangeType
	{
		Added,
		Modified,
		Deleted
	}
}
