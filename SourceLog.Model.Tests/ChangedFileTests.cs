using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
using SourceLog.Interface;

namespace SourceLog.Model.Tests
{
	[TestClass]
	public class ChangedFileTests
	{
		[TestMethod]
		[DeploymentItem(@"TestData\jquery.signalR.core.js.oldversion")]
		[DeploymentItem(@"TestData\jquery.signalR.core.js.newversion")]
		public void OldNewVersionTextStorageSizeThreshold()
		{
			//var mockContext = new Mock<ISourceLogContext>();
			
			//var logSubscription = new LogSubscription (() => mockContext.Object)
			//    {
			//        LogSubscriptionId = 1,
			//        Log = new TrulyObservableCollection<LogEntry>()
			//    };

			//var fakeLogSubscriptionDbSet = new FakeLogSubscriptionDbSet { logSubscription };
			//mockContext.Setup(m => m.LogSubscriptions).Returns(fakeLogSubscriptionDbSet);

			//var logEntriesDbSet = new FakeDbSet<LogEntry>();
			//mockContext.Setup(m => m.LogEntries).Returns(logEntriesDbSet);

			var changedFileDto = new ChangedFileDto();

			using (var reader = new FileStream("jquery.signalR.core.js.oldversion",FileMode.Open))
			{
				using (var memoryStream = new MemoryStream())
				{
					reader.CopyTo(memoryStream);
					changedFileDto.OldVersion = memoryStream.ToArray();
				}
			}

			using (var reader = new FileStream("jquery.signalR.core.js.newversion", FileMode.Open))
			{
				using (var memoryStream = new MemoryStream())
				{
					reader.CopyTo(memoryStream);
					changedFileDto.NewVersion = memoryStream.ToArray();
				}
			}

			var logEntryDto = new LogEntryDto
			{
				ChangedFiles = new List<ChangedFileDto> { changedFileDto }
			};

			//logSubscription.AddNewLogEntry(this, new NewLogEntryEventArgs { LogEntry = logEntryDto });

			var logEntry = new LogEntry(logEntryDto);

			logEntry.GenerateFlowDocuments();

			var changedFile = logEntry.ChangedFiles.First();

			Assert.IsTrue(changedFile.LeftFlowDocumentData.Length <= 5388,
				"changedFile.LeftFlowDocumentData.Length: " + changedFile.LeftFlowDocumentData.Length);

			Assert.IsTrue(changedFile.RightFlowDocumentData.Length <= 5383,
				"changedFile.RightFlowDocumentData.Length: " + changedFile.RightFlowDocumentData.Length);
		}
	}
}
