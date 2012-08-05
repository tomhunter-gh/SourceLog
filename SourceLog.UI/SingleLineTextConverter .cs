using System;
using System.Windows.Data;

namespace SourceLog
{
	public class SingleLineTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var s = (string)value;
			s = s.Replace(Environment.NewLine, " ");
			return s;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
