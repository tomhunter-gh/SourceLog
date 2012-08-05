using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace SourceLog.Model
{
	public class SideBySideFlowDocumentDiffGenerator
	{
		private double _tabWidth;
		private double _characterWidth;
		private double _lineHeight;

		public FlowDocument LeftDocument { get; private set; }
		public FlowDocument RightDocument { get; private set; }

		public double FirstModifiedLineVerticalOffset { get; private set; }

		public SideBySideFlowDocumentDiffGenerator(string leftText, string rightText)
		{
			Initialise();

			var differ = new SideBySideDiffBuilder(new Differ());
			var diffRes = differ.BuildDiffModel(leftText, rightText);

			LeftDocument = GenerateFlowDocument(diffRes.OldText);
			RightDocument = GenerateFlowDocument(diffRes.NewText);

			FirstModifiedLineVerticalOffset = diffRes.OldText.Lines.FindIndex(l => l.Type != ChangeType.Unchanged)*_lineHeight;
		}

		private void Initialise()
		{
			_tabWidth = new FormattedText(
				"\t",
				Thread.CurrentThread.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(_fontFamily.Source),
				FontSize,
				Brushes.Black
				).WidthIncludingTrailingWhitespace;
			
			var character = new FormattedText(
				"X",
				Thread.CurrentThread.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(_fontFamily.Source),
				FontSize,
				Brushes.Black
				);
			
			_characterWidth = character.WidthIncludingTrailingWhitespace;
			_lineHeight = character.Height;

		}

		readonly FontFamily _fontFamily = new FontFamily("Courier New");
		private const double FontSize = 12;
		readonly Thickness _zeroThickness = new Thickness(0);

		readonly SolidColorBrush _deletedFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100));
		readonly SolidColorBrush _insertedFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
		readonly SolidColorBrush _modifiedFillColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 255));
		readonly SolidColorBrush _imaginaryFillColor = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
		readonly SolidColorBrush _deletedWordFillColor = new SolidColorBrush(Color.FromArgb(255, 200, 100, 100));
		readonly SolidColorBrush _insertedWordFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 150));

		private FlowDocument GenerateFlowDocument(DiffPaneModel diffPaneModel)
		{
			var flowDocument = new FlowDocument();
			var lineWidths = new List<double>();
			foreach (var line in diffPaneModel.Lines)
			{
				Run run;
				Paragraph paragraph = null;

				switch (line.Type)
				{
					case ChangeType.Unchanged:
						run = new Run { Text = line.Text };
						paragraph = new Paragraph(run);
						break;
					case ChangeType.Modified:
						paragraph = RenderDiffWords(line);
						paragraph.Background = _modifiedFillColor;
						break;
					case ChangeType.Deleted:
						run = new Run { Text = line.Text };
						paragraph = new Paragraph(run) { Background = _deletedFillColor };
						break;
					case ChangeType.Inserted:
						run = new Run { Text = line.Text };
						paragraph = new Paragraph(run) { Background = _insertedFillColor };
						break;
					case ChangeType.Imaginary:
						run = new Run { Text = line.Text ?? String.Empty };
						paragraph = new Paragraph(run) { Background = _imaginaryFillColor };
						break;
				}

				if (paragraph == null) continue;
				paragraph.FontFamily = _fontFamily;
				paragraph.FontSize = FontSize;
				paragraph.Margin = _zeroThickness;

				lineWidths.Add(CalculateParagraphWidth(paragraph));

				flowDocument.Blocks.Add(paragraph);
			}

			flowDocument.PagePadding = _zeroThickness;
			flowDocument.PageWidth = lineWidths.DefaultIfEmpty(0).Max();
			return flowDocument;
		}

		private double CalculateParagraphWidth(Paragraph paragraph)
		{
			return paragraph.Inlines.OfType<Run>().Sum(r =>
					r.Text.Where(c => c == '\t').Count()*_tabWidth
					+ r.Text.Where(c => c != '\t').Count()*_characterWidth
				);
		}

		private Paragraph RenderDiffWords(DiffPiece line)
		{
			var paragraph = new Paragraph();
			foreach (var word in line.SubPieces)
			{
				var run = new Run(word.Text);
				switch (word.Type)
				{
					case ChangeType.Deleted:
						run.Background = _deletedWordFillColor;
						break;
					case ChangeType.Inserted:
						run.Background = _insertedWordFillColor;
						break;
				}
				paragraph.Inlines.Add(run);
			}
			return paragraph;
		}
	}
}