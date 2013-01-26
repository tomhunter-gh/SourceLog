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
	public static class PluginManager
	{
		private static Dictionary<string, Type> _pluginTypes;
		public static Dictionary<string, Type> PluginTypes
		{
			get { return _pluginTypes ?? (_pluginTypes = LoadPluginTypeList()); }
		}

		public static DirectoryInfo PluginsDirectory
		{
			get
			{
				return new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Plugins");
			}
		}

		private static Dictionary<string, Type> LoadPluginTypeList()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
			
			var pluginTypeList = new Dictionary<string, Type>();

			foreach (var pluginDirectory in PluginsDirectory.GetDirectories())
			{
				foreach (FileInfo fileInfo in pluginDirectory.GetFiles("*.dll"))
				{
					try
					{
						Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
						foreach (Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPlugin))))
						{
							pluginTypeList.Add(assembly.GetName().Name, type);
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
								Message = "Exception in " + typeof(PluginManager).Name + ": " + Environment.NewLine
								+ " " + ex + Environment.NewLine
								+ "\tpluginDirectory: " + pluginDirectory + Environment.NewLine
								+ "\tfileInfo.FullName: " + fileInfo.FullName + Environment.NewLine,
								Severity = TraceEventType.Error
							});
					}
				}
			}

			return pluginTypeList;
		}

		static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
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
			_pluginTypes = LoadPluginTypeList();
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