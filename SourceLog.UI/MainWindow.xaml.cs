using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
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

			//FixDataGridSorting();
		}

		private void LstSubscriptionsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.SelectedLogSubscription = e.AddedItems.Cast<LogSubscription>().First();

			CollectionViewSource.GetDefaultView(ViewModel.SelectedLogSubscription.Log)
				.SortDescriptions.Add(new SortDescription("CommittedDate", ListSortDirection.Descending));
			CollectionViewSource.GetDefaultView(ViewModel.SelectedLogSubscription.Log).Refresh();
			//CollectionViewSource.GetDefaultView(ViewModel.SelectedLogSubscription.Log).Refresh();
			//CollectionViewSource.GetDefaultView(ViewModel.SelectedLogSubscription.Log).CollectionChanged += (o, ev) =>
			//    {
			//        if (ev.Action == NotifyCollectionChangedAction.Add)
			//        {
			//            ((ICollectionView)o).SortDescriptions.Add(new SortDescription("LogEntryId", ListSortDirection.Descending));
			//            ((ICollectionView)o).Refresh();
			//        }
			//    };
			//FixDataGridSorting();
		}

		//private void FixDataGridSorting()
		//{
		//    // fix for datagrid clearing the sort order each time the ItemsSource is rebound
		//    ViewModel.LogSubscriptions.ForEach(s =>
		//        {
		//            CollectionViewSource.GetDefaultView(s.Log).SortDescriptions.Add(
		//                new SortDescription("LogEntryId", ListSortDirection.Descending));
		//            CollectionViewSource.GetDefaultView(s.Log).CollectionChanged += (o, e) =>
		//            {
		//                if (e.Action == NotifyCollectionChangedAction.Add)
		//                {
		//                    ((ICollectionView)o).Refresh();
		//                }

		//            };
		//        });
		//}

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
			var selectedLogEntry = e.AddedItems.Cast<LogEntry>().Last();
			ViewModel.SelectedLogEntry = selectedLogEntry;
			lstChangedFiles.SelectedItem = selectedLogEntry.ChangedFiles.FirstOrDefault();
		}

		private void LstChangedFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var changedFile = e.AddedItems.Cast<ChangedFile>().Last();

				//var left = changedFile.OldVersion;
				//var right = changedFile.NewVersion;

				////var stopWatch = new Stopwatch();
				////stopWatch.Start();
				//AddImaginaryLines(ref left, ref right);
				////stopWatch.Stop();
				////Debug.WriteLine("AddImaginaryLines");

				//var diffRenderer = new TextBoxDiffRenderer(left, right, LeftDiffGrid, RightDiffGrid, CurrentFont);

				//LeftBox.Document = StringToFlowDocument(left);
				//RightBox.Document = StringToFlowDocument(right);
				LeftBox.Document = changedFile.LeftFlowDocument;
				RightBox.Document = changedFile.RightFlowDocument;

				//diffRenderer.GenerateDiffView();
				ScrollFirstChangeIntoView(changedFile.FirstModifiedLineVerticalOffset);
			}
			else
			{
				LeftBox.Document = new FlowDocument();
				RightBox.Document = new FlowDocument();
			}
		}

		//static readonly FontInfo CurrentFont = new FontInfo(new FontFamily("Courier New"), 12);

		//private static FlowDocument StringToFlowDocument(string text)
		//{
		//    var flowDocument = new FlowDocument
		//        {
		//            PageWidth = GetMaxLineLength(text) + 50, 
		//            PagePadding = new Thickness(0)
		//        };
		//    var run = new Run
		//        {
		//            Text = text,
		//            FontFamily = CurrentFont.FontFamily,
		//            FontSize = CurrentFont.Size
		//    };
		//    flowDocument.Blocks.Add(new Paragraph(run));
		//    return flowDocument;
		//}

		//private static double GetMaxLineLength(string text)
		//{
		//    var formattedSpace = new FormattedText(
		//        " ",
		//        Thread.CurrentThread.CurrentCulture,
		//        FlowDirection.LeftToRight,
		//        new Typeface(CurrentFont.FontFamily.Source),
		//        CurrentFont.Size,
		//        Brushes.Black
		//    );

		//    var formattedTab = new FormattedText(
		//        "\t",
		//        Thread.CurrentThread.CurrentCulture,
		//        FlowDirection.LeftToRight,
		//        new Typeface(CurrentFont.FontFamily.Source),
		//        CurrentFont.Size,
		//        Brushes.Black
		//    );

		//    return text.Split('\n').Max(
		//        l =>
		//            l.Where(c => c == '\t').Count() * formattedTab.WidthIncludingTrailingWhitespace
		//            + l.Where(c => c != '\t').Count() * formattedSpace.WidthIncludingTrailingWhitespace
		//    );
		//}

		//private static void AddImaginaryLines(ref string left, ref string right)
		//{
		//    left = StripImaginaryLinesAndCharacters(left);
		//    right = StripImaginaryLinesAndCharacters(right);

		//    var differ = new SideBySideDiffBuilder(new Differ());
		//    var diffRes = differ.BuildDiffModel(left, right);

		//    left = AddImaginaryLinesToText(diffRes.OldText, left);
		//    right = AddImaginaryLinesToText(diffRes.NewText, right);
		//}

		//private const char ImaginaryLineCharacter = '\u202B';

		//private static string StripImaginaryLinesAndCharacters(string text)
		//{
		//    var lines = text.Split('\r').Where(x => !x.Equals(ImaginaryLineCharacter.ToString()));
		//    var aggregatedLines = lines.Count() == 0 ? String.Empty : String.Join("\r", lines);

		//    return aggregatedLines.Replace(ImaginaryLineCharacter.ToString(), String.Empty);
		//}

		//private static string AddImaginaryLinesToText(DiffPaneModel diffModel, string text)
		//{
		//    var lines = new List<string>();
		//    if (!String.IsNullOrEmpty(text))
		//    {
		//        lines = text.Split('\r').ToList();
		//    }

		//    var lineNumber = 0;
		//    foreach (var line in diffModel.Lines)
		//    {
		//        if (line.Type == ChangeType.Imaginary)
		//        {
		//            lines.Insert(lineNumber, ImaginaryLineCharacter.ToString());
		//        }
		//        lineNumber++;
		//    }

		//    if (lines.Count > 1)
		//    {
		//        return String.Join("\r", lines);
		//    }
		//    return lines.DefaultIfEmpty(String.Empty).First();
		//}


		private void ScrollFirstChangeIntoView(double firstModifiedLineVerticalOffset)
		{
			LeftScroller.UpdateLayout();
			LeftScroller.ScrollToVerticalOffset(firstModifiedLineVerticalOffset - 30);
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
	}
}
