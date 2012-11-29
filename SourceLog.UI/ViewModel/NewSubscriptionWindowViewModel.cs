using System.Collections.Generic;
using System.Linq;
using SourceLog.Model;

namespace SourceLog.ViewModel
{
	public class NewSubscriptionWindowViewModel
	{
		public List<string> LogProviderPluginNames
		{
			get { return LogProviderPluginManager.LogProviderPluginTypes.Select(p => p.Key).ToList(); }
		}

		public void AddSubscription(string name, string logProviderTypeName, string url)
		{
			MainWindowViewModel.LogSubscriptionManager.AddLogSubscription(name, logProviderTypeName, url);
		}
	}
}
