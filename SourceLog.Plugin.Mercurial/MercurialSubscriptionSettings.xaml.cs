using System;
using System.Xml.Linq;
using SourceLog.Interface;
using WinForms = System.Windows.Forms;

namespace SourceLog.Plugin.Git
{
	/// <summary>
	/// Interaction logic for GitSubscriptionSettings.xaml
	/// </summary>
	public partial class GitSubscriptionSettings : ISubscriptionSettings
	{
		public GitSubscriptionSettings()
		{
			InitializeComponent();
		}

		public string SettingsXml
		{
			get
			{
				return new XDocument(
					new XElement("Settings",
						new XElement("Directory", txtDirectory.Text))
				).ToString();
			}
			set
			{
				var settingsXml = XDocument.Parse(value);
				txtDirectory.Text = settingsXml.Root.Element("Directory").Value;				
			}
		}

		private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var dlg = new WinForms.FolderBrowserDialog();
			WinForms.DialogResult result = dlg.ShowDialog(this.GetIWin32Window());

			if (result.ToString() == "OK")
				txtDirectory.Text = dlg.SelectedPath;
		}
	}
}
