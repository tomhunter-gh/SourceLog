using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Documents;
using Moq;
using SourceLog.Interface;
using System.Diagnostics;

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

			var logSubscription = new LogSubscription
			{
				LogSubscriptionId = 1,
				Log = new TrulyObservableCollection<LogEntry>()
			};
			var fakeLogSubscriptionDbSet = new FakeDbSet<LogSubscription> { logSubscription };

			var mockContext = new Mock<ISourceLogContext>();
			mockContext.Setup(m => m.LogSubscriptions).Returns(fakeLogSubscriptionDbSet);
			SourceLogContext.ThreadStaticContext = mockContext.Object;

			logSubscription.AddNewLogEntry(this, new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry });

			var textRange = new TextRange(changedFile.LeftFlowDocument.ContentStart, changedFile.LeftFlowDocument.ContentEnd);
			Assert.IsTrue(textRange.Text.StartsWith("\t"));
		}
	}
}
