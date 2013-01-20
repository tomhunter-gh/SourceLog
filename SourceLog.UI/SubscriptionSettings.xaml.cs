using SourceLog.Interface;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for SubscriptionSettings.xaml
	/// </summary>
	public partial class SubscriptionSettings : ISubscriptionSettings
	{
		public SubscriptionSettings()
		{
			InitializeComponent();
		}

		public string SettingsXml
		{
			get { return txtUrl.Text; }
			set { txtUrl.Text = value; }
		}
	}
}
