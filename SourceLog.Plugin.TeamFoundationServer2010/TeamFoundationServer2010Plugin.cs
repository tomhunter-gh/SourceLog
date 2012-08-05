using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceLog.Interface;
using SourceLog.Model;

namespace SourceLog.Plugin.TeamFoundationServer2010
{
	public class TeamFoundationServer2010Plugin : ILogProvider<ChangedFile>
	{
		public string SettingsXml
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public DateTime MaxDateTimeRetrieved
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public void Initialise()
		{
			throw new NotImplementedException();
		}

		public event NewLogEntryEventHandler<ChangedFile> NewLogEntry;
	}
}
