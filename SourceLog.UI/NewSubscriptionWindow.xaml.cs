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
using System.Windows.Shapes;
using SourceLog.ViewModel;

namespace SourceLog
{
	/// <summary>
	/// Interaction logic for NewSubscriptionWindow.xaml
	/// </summary>
	public partial class NewSubscriptionWindow : Window
	{
		private readonly NewSubscriptionWindowViewModel _vm;

		public NewSubscriptionWindow()
		{
			_vm = new NewSubscriptionWindowViewModel();
			DataContext = _vm;
			

			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			_vm.AddSubscription(LogSubscriptionNameTextBox.Text, (string)LogProviderPluginDropDown.SelectedValue, UrlTextBox.Text);
			Close();
		}

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
	}
}
