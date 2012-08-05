using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace SourceLog
{
	public class TextBoxDiffRenderer
	{
		private readonly Grid _leftGrid;
		private readonly string _leftText;
		private readonly string _rightText;
		private readonly Grid _rightGrid;

		private SideBySideDiffBuilder differ;

		private readonly object mutex = new object();
		private bool inDiff;

		private readonly FontInfo _currentFont;

		public TextBoxDiffRenderer(string leftText, string rightText, Grid leftGrid, Grid rightGrid, FontInfo fontInfo)
		{
			_leftGrid = leftGrid;
			_leftText = leftText;
			_rightGrid = rightGrid;
			_rightText = rightText;
			_currentFont = fontInfo;
		}

		public bool ShowVisualAids { private get; set; }
		public double? CharacterWidthOverride { private get; set; }
		public double? LeftOffsetOverride { private get; set; }
		public double? LinePaddingOverride { private get; set; }

		public int FirstModifiedLine { get; set; }

		public void GenerateDiffView()
		{
			if (inDiff) return;
			lock (mutex)
			{
				if (inDiff) return;
				inDiff = true;
			}

			differ = new SideBySideDiffBuilder(new Differ());


			var diffRes = differ.BuildDiffModel(_leftText, _rightText);

			FirstModifiedLine = diffRes.NewText.Lines.FindIndex(l => l.Type != ChangeType.Unchanged);
			GenerateDiffPanes(diffRes.OldText, diffRes.NewText);

			inDiff = false;
		}

		private void GenerateDiffPanes(DiffPaneModel leftDiff, DiffPaneModel rightDiff)
		{
			RenderDiffLines(_leftGrid, leftDiff);
			RenderDiffLines(_rightGrid, rightDiff);
		}

		private static void ClearDiffLines(Panel grid)
		{
			var rectangles = grid.Children.OfType<Rectangle>().ToList();
			foreach (var rect in rectangles)
			{
				grid.Children.Remove(rect);
			}
		}

		private void RenderDiffLines(Grid grid, DiffPaneModel diffModel)
		{
			ClearDiffLines(grid);

			var transparentFillColor = new SolidColorBrush(Colors.Transparent);
			var deletedFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100));
			var insertedFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
			var unchangedFillColor = new SolidColorBrush(Colors.White);
			var modifiedFillColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 255));
			var imaginaryFillColor = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));

			var lineNumber = 0;
			foreach (var line in diffModel.Lines)
			{
				var fillColor = transparentFillColor;
				
				switch (line.Type)
				{
					case ChangeType.Deleted:
						fillColor = deletedFillColor;
						break;
					case ChangeType.Inserted:
						fillColor = insertedFillColor;
						break;
					case ChangeType.Unchanged:
						fillColor = unchangedFillColor;
						break;
					case ChangeType.Modified:
						fillColor = modifiedFillColor;
						break;
					case ChangeType.Imaginary:
						fillColor = imaginaryFillColor;
						break;
				}

				if (line.Type != ChangeType.Unchanged)
				{
					PlaceRectangleInGrid(grid, lineNumber, fillColor, 0, null);
				}
				
				if (line.Type == ChangeType.Modified)
					RenderDiffWords(grid, line, lineNumber);
				
				lineNumber++;
			}
		}

		private void RenderDiffWords(Grid grid, DiffPiece line, int lineNumber)
		{
			var charPos = 0;
			var characterWidth = CharacterWidthOverride ?? _currentFont.CharacterWidth;
			var leftOffset = LeftOffsetOverride ?? _currentFont.LeftOffset;
			foreach (var word in line.SubPieces)
			{
				SolidColorBrush fillColor;
				if (word.Type == ChangeType.Deleted)
					fillColor = new SolidColorBrush(Color.FromArgb(255, 200, 100, 100));
				else if (word.Type == ChangeType.Inserted)
					fillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 150));
				else if (word.Type == ChangeType.Imaginary)
					continue;
				else
					fillColor = new SolidColorBrush(Colors.Transparent);

				var left = characterWidth * charPos + leftOffset;
				var wordWidth = characterWidth * word.Text.Length;
				PlaceRectangleInGrid(grid, lineNumber, fillColor, left, wordWidth);

				charPos += word.Text.Length;
			}
		}

		private void PlaceRectangleInGrid(Grid grid, int lineNumber, SolidColorBrush fillColor, double left, double? width)
		{
			var rectLineHeight = _currentFont.LineHeight;
			const double rectTopOffset = 0;

			var offset = rectLineHeight * lineNumber + rectTopOffset;
			var floor = Math.Floor(offset);

			var rectangle = new Rectangle
			{
				Fill = fillColor,
				Width = width ?? Double.NaN,
				Height = rectLineHeight,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = width.HasValue ? HorizontalAlignment.Left : HorizontalAlignment.Stretch,
				Margin = new Thickness(left, floor, 0, 0)
			};

			grid.Children.Insert(grid.Children.Count - 1, rectangle);
			//grid.Children.Add(rectangle);
		}
	}
}
