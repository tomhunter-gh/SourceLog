using System;
using System.Collections.Generic;

namespace SourceLog.Interface
{
	public interface ILogEntry<T> where T : IChangedFile
	{
		string Revision { get; set; }
		DateTime CommittedDate { get; set; }
		string Message { get; set; }
		string Author { get; set; }

		List<T> ChangedFiles { get; set; }
	}
}
