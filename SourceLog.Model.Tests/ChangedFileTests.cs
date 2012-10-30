﻿using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
			var mockContext = new Mock<ISourceLogContext>();
			
			var logSubscription = new LogSubscription (() => mockContext.Object)
				{
					LogSubscriptionId = 1,
					Log = new TrulyObservableCollection<LogEntry>()
				};

			var fakeLogSubscriptionDbSet = new FakeLogSubscriptionDbSet { logSubscription };
			mockContext.Setup(m => m.LogSubscriptions).Returns(fakeLogSubscriptionDbSet);

			var changedFile = new ChangedFile();

			using (var reader = new StreamReader("jquery.signalR.core.js.oldversion"))
			{
				changedFile.OldVersion = reader.ReadToEnd();
			}

			using (var reader = new StreamReader("jquery.signalR.core.js.newversion"))
			{
				changedFile.NewVersion = reader.ReadToEnd();
			}

			var logEntry = new LogEntry
			{
				ChangedFiles = new List<ChangedFile> { changedFile }
			};

			logSubscription.AddNewLogEntry(this, new NewLogEntryEventArgs<ChangedFile> { LogEntry = logEntry });


			Assert.IsTrue(changedFile.LeftFlowDocumentData.Length <= 5219,
				"changedFile.LeftFlowDocumentData.Length: " + changedFile.LeftFlowDocumentData.Length);

			Assert.IsTrue(changedFile.RightFlowDocumentData.Length <= 5224,
				"changedFile.RightFlowDocumentData.Length: " + changedFile.RightFlowDocumentData.Length);
		}
	}
}
