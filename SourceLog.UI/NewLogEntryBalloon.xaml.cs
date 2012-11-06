using System.Windows.Controls;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for NewLogEntryBalloon.xaml
	/// </summary>
	public partial class NewLogEntryBalloon : UserControl
	{
		public NewLogEntryBalloon(string logSubscription, string author, string message)
		{
			InitializeComponent();
			LogSubscription.Text = logSubscription;
			Author.Text = author;
			Message.Text = message;
		}
	}
}
