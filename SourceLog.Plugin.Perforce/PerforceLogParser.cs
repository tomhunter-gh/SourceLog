using System;
using SourceLog.Model;
using System.Text.RegularExpressions;

namespace SourceLog.Plugin.Perforce
{
	public static class PerforceLogParser
	{
		internal static LogEntry Parse(string changesetString)
		{
			var logEntry = new LogEntry();

			const string pattern = @"Change\s(?<revision>\d+)\son\s(?<datetime>\d{4}/\d{2}/\d{2}\s\d{2}:\d{2}:\d{2})\sby\s(?<author>\w+)@\w+\n\n\t(?<message>.*)";
			var r = new Regex(pattern);
			var match = r.Match(changesetString);
			if (match.Success)
			{
				int revision;
				if (Int32.TryParse(match.Groups["revision"].Value, out revision))
					logEntry.Revision = revision.ToString();

				DateTime datetime;
				if (DateTime.TryParse(match.Groups["datetime"].Value, out datetime))
					logEntry.CommittedDate = datetime;

				logEntry.Author = match.Groups["author"].Value;
				logEntry.Message = match.Groups["message"].Value;

			}

			return logEntry;
		}

		internal static ChangedFile ParseP4File(string file)
		{
			var changedFile = new ChangedFile();
			const string pattern = @"(?<filename>[^#]*)#(?<revision>\d+)\s-\s(?<action>\w+)\schange\s(?<changeNumber>\d+)\s\((?<filetype>\w+)\)";
			var r = new Regex(pattern);
			var match = r.Match(file);
			if (match.Success)
			{
				changedFile.FileName = match.Groups["filename"].Value;
				switch (match.Groups["action"].Value)
				{
					case "add" :
						changedFile.ChangeType = Interface.ChangeType.Added;
						break;
					case "edit" :
						changedFile.ChangeType = Interface.ChangeType.Modified;
						break;
					case "delete" :
						changedFile.ChangeType = Interface.ChangeType.Deleted;
						break;
					case "branch" :
						changedFile.ChangeType = Interface.ChangeType.Copied;
						break;
				}

			}

			return changedFile;
		}
	}
}
