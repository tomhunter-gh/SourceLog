using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SourceLog.Interface;

namespace SourceLog.Model.Tests
{
	[TestClass]
	public class FlowDocumentTests
	{
		[TestMethod]
		public void LeadingTabsAreDisplayed()
		{
			var changedFile = new ChangedFile();
			changedFile.OldVersion = "\t@ ,";
			changedFile.NewVersion = "\t @ ";

			var logEntry = new LogEntry { ChangedFiles = new List<ChangedFile> { changedFile } };

			var mockContext = new Mock<ISourceLogContext>();
			var logSubscription = new LogSubscription(() => mockContext.Object)
			{
				LogSubscriptionId = 1,
				Log = new TrulyObservableCollection<LogEntry>()
			};

			var fakeLogSubscriptionDbSet = new FakeLogSubscriptionDbSet { logSubscription };
			mockContext.Setup(m => m.LogSubscriptions).Returns(fakeLogSubscriptionDbSet);

			logSubscription.AddNewLogEntry(this, new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry });

			var textRange = new TextRange(changedFile.LeftFlowDocument.ContentStart, changedFile.LeftFlowDocument.ContentEnd);
			Assert.IsTrue(textRange.Text.StartsWith("\t"));
		}
	}

	public class FakeLogSubscriptionDbSet : FakeDbSet<LogSubscription>
	{
		public override LogSubscription Find(params object[] keyValues)
		{
			return this.Single(ls => ls.LogSubscriptionId == (int)(keyValues[0]));
		}
	}
}
