using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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
			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}
}