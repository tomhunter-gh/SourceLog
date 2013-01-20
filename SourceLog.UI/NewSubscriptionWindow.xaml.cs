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
using System.Windows.Shapes;
using SourceLog.Interface;
using SourceLog.ViewModel;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for NewSubscriptionWindow.xaml
	/// </summary>
	public partial class NewSubscriptionWindow : Window
	{
		private readonly NewSubscriptionWindowViewModel _vm;

		public NewSubscriptionWindow()
		{
			_vm = new NewSubscriptionWindowViewModel();
			DataContext = _vm;
			

			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var settingsXml = ((ISubscriptionSettings) grpSettings.Content).SettingsXml;
			_vm.AddSubscription(
				LogSubscriptionNameTextBox.Text, 
				(string)LogProviderPluginDropDown.SelectedValue, 
				settingsXml);
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void LogProviderPluginDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var ui = _vm.GetSubscriptionSettingsUiForPlugin((string) LogProviderPluginDropDown.SelectedValue);
			grpSettings.Content = ui ?? new SubscriptionSettings();
		}
	}
}
