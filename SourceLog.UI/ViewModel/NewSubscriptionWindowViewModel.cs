using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SourceLog.Model;

namespace SourceLog.ViewModel
{
	public class NewSubscriptionWindowViewModel
	{
		private readonly MainWindowViewModel _mainWindowViewModel;

		public List<string> LogProviderPluginNames
		{
			get { return PluginManager.PluginTypes.Select(p => p.Key).ToList(); }
		}

		public NewSubscriptionWindowViewModel(MainWindowViewModel mainWindowViewModel)
		{
			_mainWindowViewModel = mainWindowViewModel;
		}

		public NewSubscriptionWindowViewModel()
		{
		}

		public void AddSubscription(string name, string logProviderTypeName, string url)
		{
			var logSubscription = MainWindowViewModel.LogSubscriptionManager.AddLogSubscription(name, logProviderTypeName, url);
			_mainWindowViewModel.SelectedLogSubscription = logSubscription;
		}

		public UserControl GetSubscriptionSettingsUiForPlugin(string pluginName)
		{
			return PluginManager.GetSubscriptionSettingsUiForPlugin(pluginName);
		}
	}
}
