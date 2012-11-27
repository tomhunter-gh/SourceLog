using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SourceLog.Interface;

namespace SourceLog.Plugin.CrashTestDummy
{
	public class CrashTestDummyPlugin : ILogProvider
	{
		public string SettingsXml { get; set; }
		public DateTime MaxDateTimeRetrieved { get; set; }

		public void Initialise()
		{
			var thread = new Thread(() =>
										{
											var window = new Window
												{
													Content = new CrashTestDummyWindow(this),
													Height = 100,
													Width = 200,
													Title = "CrashTestDummy"
												};
											window.Show();
											window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
											Dispatcher.Run();
										});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

		}

		public event NewLogEntryEventHandler NewLogEntry;
		public event LogProviderExceptionEventHandler LogProviderException;

		public void FireNewLogEntry()
		{
			var args = new NewLogEntryEventArgs
				{
					LogEntry = new LogEntryDto
						{
							Author = "Tom",
							CommittedDate = DateTime.Now,
							Message = "Test message..",
							ChangedFiles = new List<ChangedFileDto>
								{
									new ChangedFileDto
										{
											ChangeType = ChangeType.Modified,
											FileName = "C:\\temp\\file.ext",
											OldVersion = "Old",
											NewVersion = "New"
										}
								}
						}
				};

			NewLogEntry(this, args);
		}
	}
}
