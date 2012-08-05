using System.Windows.Controls;

namespace SourceLog
{
	public class CustomFlowDocumentScrollViewer : FlowDocumentScrollViewer
	{
		private ScrollViewer _scrollViewer;
		public ScrollViewer ScrollViewer
		{
			get
			{
				if (_scrollViewer == null)
				{
					_scrollViewer = Template.FindName("PART_ContentHost", this) as ScrollViewer;
				}
				return _scrollViewer;
			}
		}
	}
}
