using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace SourceLog.Model
{
	public sealed class TrulyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
	{
		public TrulyObservableCollection()
		{
			CollectionChanged += TrulyObservableCollectionChanged;
		}

		void TrulyObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					((INotifyPropertyChanged) item).PropertyChanged += ItemPropertyChanged;
				}
			}
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					((INotifyPropertyChanged) item).PropertyChanged -= ItemPropertyChanged;
				}
			}
		}

		void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

			// TODO: remove reference to System.Windows.Threading.Dispatcher?
			if (Thread.CurrentThread.ManagedThreadId == Dispatcher.CurrentDispatcher.Thread.ManagedThreadId)
			{
				OnCollectionChanged(a);
			}
		}
	}
}