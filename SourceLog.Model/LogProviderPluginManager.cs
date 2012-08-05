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

		private static Dictionary<string, Type> LoadLogProviderPluginTypeList()
		{
			var logProviderPluginTypeList = new Dictionary<string, Type>();

			var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory);
			foreach(FileInfo fileInfo in directoryInfo.GetFiles("*.dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
					foreach (Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILogProvider<ChangedFile>))))
					{
						logProviderPluginTypeList.Add(assembly.FullName, type);
					}
				}
				catch (BadImageFormatException)
				{ }
				catch (FileLoadException)
				{ }
			}

			return logProviderPluginTypeList;
		}

		public static void Refresh()
		{
			_logProviderPluginTypes = LoadLogProviderPluginTypeList();
		}
	}
}
