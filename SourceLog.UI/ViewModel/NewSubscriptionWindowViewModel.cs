using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceLog.Model;

namespace SourceLog.ViewModel
{
	class NewSubscriptionWindowViewModel
	{
		public List<string> LogProviderPluginNames
		{
			get { return LogProviderPluginManager.LogProviderPluginTypes.Select(p => p.Key).ToList(); }
		}

		public void AddSubscription(string name, string logProviderTypeName, string url)
		{
			LogSubscriptionManager.AddLogSubscription(name, logProviderTypeName, url);
		}
	}
}
