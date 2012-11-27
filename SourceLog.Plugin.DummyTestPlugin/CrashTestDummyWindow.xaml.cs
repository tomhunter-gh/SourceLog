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

namespace SourceLog.Plugin.CrashTestDummy
{
	/// <summary>
	/// Interaction logic for CrashTestDummyWindow.xaml
	/// </summary>
	public partial class CrashTestDummyWindow : UserControl
	{
		private CrashTestDummyPlugin _plugin;

		public CrashTestDummyWindow(CrashTestDummyPlugin plugin)
		{
			InitializeComponent();
			_plugin = plugin;
		}

		private void Button1Click(object sender, RoutedEventArgs e)
		{
			_plugin.FireNewLogEntry();
		}
	}
}
