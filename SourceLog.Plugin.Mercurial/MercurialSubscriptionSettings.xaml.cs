using System;
using System.Xml.Linq;
using SourceLog.Interface;

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
	}
}
