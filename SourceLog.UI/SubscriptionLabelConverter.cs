using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SourceLog.Model;

namespace SourceLog
{
	public class SubscriptionLabelConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var name = (string)values[0];
			var unreadCount = ((ObservableCollection<LogEntry>) values[1]).Count(le => !le.Read);

			return name + (unreadCount > 0 ? " (" + unreadCount + ")" : String.Empty);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
