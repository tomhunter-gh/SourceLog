using System;
using System.Collections.Generic;

namespace SourceLog.Interface
{
	public class LogEntryDto
	{
		public string Revision { get; set; }
		public DateTime CommittedDate { get; set; }
		public string Message { get; set; }
		public string Author { get; set; }

		public List<ChangedFileDto> ChangedFiles { get; set; }
	}
}
