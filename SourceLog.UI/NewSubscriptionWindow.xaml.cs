using System.Windows;
using System.Windows.Controls;
using SourceLog.Interface;
using SourceLog.Model;
using SourceLog.ViewModel;

namespace SourceLog
{
	public partial class NewSubscriptionWindow
	{
		private readonly NewSubscriptionWindowViewModel _vm;
		private readonly LogSubscription _logSubscription;

		// used for adding a subscription
		public NewSubscriptionWindow(MainWindowViewModel mainWindowViewModel)
		{
			_vm = new NewSubscriptionWindowViewModel(mainWindowViewModel);
			DataContext = _vm;

			InitializeComponent(); 
		}

		// used for editing a subscription
		public NewSubscriptionWindow(LogSubscription logSubscription)
		{
			_vm = new NewSubscriptionWindowViewModel();
			DataContext = _vm;

			InitializeComponent(); 

			txtName.Text = logSubscription.Name;
			ddlPlugin.SelectedValue = logSubscription.PluginTypeName;
			LogProviderPluginDropDownSelectionChanged(this, null);
			((ISubscriptionSettings) grpSettings.Content).SettingsXml = logSubscription.Url;

			btnAdd.Visibility = Visibility.Hidden;
			btnUpdate.Visibility = Visibility.Visible;

			_logSubscription = logSubscription;
		}

		private void LogProviderPluginDropDownSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var ui = _vm.GetSubscriptionSettingsUiForPlugin((string) ddlPlugin.SelectedValue);
			grpSettings.Content = ui ?? new SubscriptionSettings();
		}

		private void BtnAddClick(object sender, RoutedEventArgs e)
		{
			var settingsXml = ((ISubscriptionSettings) grpSettings.Content).SettingsXml;
			_vm.AddSubscription(
				txtName.Text, 
				(string)ddlPlugin.SelectedValue, 
				settingsXml);
			Close();
		}

		private void BtnUpdateClick(object sender, RoutedEventArgs e)
		{
			var settingsXml = ((ISubscriptionSettings) grpSettings.Content).SettingsXml;
			_logSubscription.Update(txtName.Text, (string) ddlPlugin.SelectedValue, settingsXml);
			Close();
		}

		private void BtnCancelClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
