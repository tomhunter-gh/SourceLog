using System;
using System.Windows;
using System.Windows.Input;

namespace SourceLog
{
	public class ShowWindowCommand : ICommand
	{
		public void Execute(object parameter)
		{
			var window = (Window) parameter;
			if (window.WindowState == WindowState.Minimized)
				window.WindowState = WindowState.Normal;
			window.Activate();
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;
	}
}
