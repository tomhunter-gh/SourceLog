using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SourceLog.Interface;
using System.Xml.Linq;

namespace SourceLog.Plugin.TeamFoundationServer2010
{
    public partial class TFSSubscriptionSettings : ISubscriptionSettings
    {
        public TFSSubscriptionSettings()
        {
            InitializeComponent();
        }

        public string SettingsXml
        {
            get
            {
                return new XDocument(
                    new XElement("Settings",
                        new XElement("CollectionURL", txtCollectionUrl.Text),
                        new XElement("SourceLocation", txtSourceLocation.Text))
                ).ToString();
            }
            set
            {
                var settingsXml = XDocument.Parse(value);
                txtCollectionUrl.Text = settingsXml.Root.Element("CollectionURL").Value;
                txtSourceLocation.Text = settingsXml.Root.Element("SourceLocation").Value;
            }
        }
    }
}
