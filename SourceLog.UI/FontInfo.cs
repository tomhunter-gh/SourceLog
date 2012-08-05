using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SourceLog
{
	public class FontInfo
	{
		public FontFamily FontFamily { get; set; }
		public double Size { get; set; }
		public double LineHeight { get; set; }
		public double CharacterWidth { get; set; }
		public double TabWidth { get; set; }
		public double LeftOffset { get; set; }

		public FontInfo(FontFamily fontFamily, double fontSize)
		{
			FontFamily = fontFamily;
			Size = fontSize;
			LineHeight = fontSize * FontFamily.LineSpacing;
			
			var fixedWidthCharacter = new FormattedText("X",
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(FontFamily.Source),
				fontSize,
				Brushes.Black);

			CharacterWidth = fixedWidthCharacter.Width;

			var tabCharacter = new FormattedText("\t",
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(FontFamily.Source),
				fontSize,
				Brushes.Black);

			TabWidth = tabCharacter.WidthIncludingTrailingWhitespace;

			LeftOffset = 0;
		}

		
	}
}
