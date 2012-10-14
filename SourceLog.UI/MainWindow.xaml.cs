using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using SourceLog.Model;
using SourceLog.ViewModel;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindowViewModel ViewModel;

		public MainWindow()
		{
			InitializeComponent();

			ViewModel = new MainWindowViewModel();
			DataContext = ViewModel;

			FixDataGridSorting();
		}

		private void LstSubscriptionsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.SelectedLogSubscription = e.AddedItems.Cast<LogSubscription>().First();
			FixDataGridSorting();
			
			//if (ViewModel.SelectedLogSubscription.Log.Count > 0)
			//{
			//    dgLog.SelectedItem = ViewModel.SelectedLogSubscription.Log.Last();
			//    lstChangedFiles.SelectedItem = ViewModel.SelectedLogEntry.ChangedFiles.FirstOrDefault();
			//}
		}

		private void FixDataGridSorting()
		{
			// fix for datagrid clearing the sort order each time the ItemsSource is rebound
			ViewModel.LogSubscriptions.ToList().ForEach(s =>
				CollectionViewSource.GetDefaultView(s.Log).SortDescriptions.Add(
					new SortDescription("LogEntryId", ListSortDirection.Descending)
				)
			);
		}

		private void ButtonClick(object sender, RoutedEventArgs e)
		{
			var newSubscriptionWindow = new NewSubscriptionWindow { Owner = this };
			newSubscriptionWindow.ShowDialog();
		}

		private void DgLogSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach (var readItem in e.RemovedItems.Cast<LogEntry>())
			{
				ViewModel.MarkEntryRead(readItem);
			}
			if (e.AddedItems.Count > 0)
			{
				var selectedLogEntry = e.AddedItems.Cast<LogEntry>().Last();
				ViewModel.SelectedLogEntry = selectedLogEntry;
				lstChangedFiles.SelectedItem = selectedLogEntry.ChangedFiles.FirstOrDefault();
			}
		}

		private void LstChangedFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var changedFile = e.AddedItems.Cast<ChangedFile>().Last();

				LeftBox.Document = changedFile.LeftFlowDocument;
				RightBox.Document = changedFile.RightFlowDocument;

				RightBox.Document.Loaded += (o, x) => ScrollFirstChangeIntoView(
					((ChangedFile)lstChangedFiles.SelectedItem).FirstModifiedLineVerticalOffset);
			}
			else
			{
				LeftBox.Document = new FlowDocument();
				RightBox.Document = new FlowDocument();
			}
		}

		private void ScrollFirstChangeIntoView(double firstModifiedLineVerticalOffset)
		{
			//LeftScroller.UpdateLayout();
			LeftScroller.ScrollToVerticalOffset(firstModifiedLineVerticalOffset - (LeftScroller.ViewportHeight / 2));
		}

		private void LeftScrollerScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var scrollViewer = sender as ScrollViewer;
			if (scrollViewer != null)
			{
				RightScroller.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
				RightScroller.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
			}
		}

		private void RightScrollerScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var scrollViewer = sender as ScrollViewer;
			if (scrollViewer != null)
			{
				LeftScroller.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
				LeftScroller.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
			}
		}

		private void PassDiffPaneMouseWheelToParent(object sender, MouseWheelEventArgs e)
		{
			if (!e.Handled)
			{
				e.Handled = true;
				var eventArg = new MouseWheelEventArgs(
					e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent, Source = sender};
				var parent = ((Control)sender).Parent as UIElement;
				if (parent != null) parent.RaiseEvent(eventArg);
			}
		}
	}
}
