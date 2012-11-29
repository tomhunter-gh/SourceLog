using System.Windows;
using System.Windows.Controls;

namespace SourceLog.Plugin.CrashTestDummy
{
	/// <summary>
	/// Interaction logic for CrashTestDummyWindow.xaml
	/// </summary>
	public partial class CrashTestDummyWindow : UserControl
	{
		private readonly CrashTestDummyPlugin _plugin;

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
