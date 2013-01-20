using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;

namespace SourceLog.Model
{
	public static class LogProviderPluginManager
	{
		private static Dictionary<string, Type> _logProviderPluginTypes;
		public static Dictionary<string, Type> LogProviderPluginTypes
		{
			get
			{
				if (_logProviderPluginTypes == null)
					_logProviderPluginTypes = LoadLogProviderPluginTypeList();
				return _logProviderPluginTypes;
			}
		}

		public static DirectoryInfo PluginsDirectory
		{
			get
			{
				return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Plugins");
			}
		}

		private static Dictionary<string, Type> LoadLogProviderPluginTypeList()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			
			var logProviderPluginTypeList = new Dictionary<string, Type>();

			foreach (var pluginDirectory in PluginsDirectory.GetDirectories())
			{
				foreach (FileInfo fileInfo in pluginDirectory.GetFiles("*.dll"))
				{
					try
					{
						Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
						foreach (Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILogProvider))))
						{
							logProviderPluginTypeList.Add(assembly.GetName().Name, type);
						}
					}
					//catch (BadImageFormatException)
					//{ }
					//catch (FileLoadException)
					//{ }
					catch (Exception ex)
					{
						Logger.Write(new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
							{
								Message = "Exception in LoadLogProviderPluginTypeList: " + Environment.NewLine
								+ " " + ex + Environment.NewLine
								+ "\tpluginDirectory: " + pluginDirectory + Environment.NewLine
								+ "\tfileInfo.FullName: " + fileInfo.FullName + Environment.NewLine,
								Severity = TraceEventType.Error
							});
					}
				}
			}

			return logProviderPluginTypeList;
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			foreach (var pluginDirectory in PluginsDirectory.GetDirectories())
			{
				string assemblyPath = Path.Combine(pluginDirectory.FullName, new AssemblyName(args.Name).Name + ".dll");
				if (File.Exists(assemblyPath))
					return Assembly.LoadFrom(assemblyPath);
			}

			return null;
		}

		public static void Refresh()
		{
			_logProviderPluginTypes = LoadLogProviderPluginTypeList();
		}

		public static UserControl GetSubscriptionSettingsUiForPlugin(string pluginName)
		{
			var pluginDirectory = PluginsDirectory.GetDirectories().Where(d => d.Name == pluginName).FirstOrDefault();
			if(pluginDirectory != null)
			{
				foreach (FileInfo fileInfo in pluginDirectory.GetFiles("*.dll"))
				{
					try
					{
						Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
						var type =
							assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof (ISubscriptionSettings))).FirstOrDefault();
						if (type != null)
						{
							return Activator.CreateInstance(type) as UserControl;
						}
					}
					catch (Exception ex)
					{
						Logger.Write(new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
						{
							Message = "Exception in GetSubscriptionSettingsUiForPlugin: " + Environment.NewLine
							+ " " + ex + Environment.NewLine
							+ "\tpluginDirectory: " + pluginDirectory + Environment.NewLine
							+ "\tfileInfo.FullName: " + fileInfo.FullName + Environment.NewLine,
							Severity = TraceEventType.Error
						});
					}
				}
			}
			return null;
		}
	}
}
