using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

		public static DirectoryInfo pluginsDirectory
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

			foreach (var pluginDirectory in pluginsDirectory.GetDirectories())
			{
				foreach (FileInfo fileInfo in pluginDirectory.GetFiles("*.dll"))
				{
					try
					{
						Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
						foreach (Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILogProvider))))
						{
							logProviderPluginTypeList.Add(assembly.FullName, type);
						}
					}
					catch (BadImageFormatException)
					{ }
					catch (FileLoadException)
					{ }
				}
			}

			return logProviderPluginTypeList;
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			foreach (var pluginDirectory in pluginsDirectory.GetDirectories())
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
	}
}
