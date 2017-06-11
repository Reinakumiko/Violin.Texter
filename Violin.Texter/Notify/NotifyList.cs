using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Notify
{
	public class NotifyList<T> : List<T>, INotifyCollectionChanged, INotifyPropertyChanged, IList<T>, IList, IEnumerable, IEnumerable<T>, ICollection, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
		where T : INotifyPropertyChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		public NotifyList() : base () { }
		public NotifyList(int capacity) : base(capacity) { }
		public NotifyList(IEnumerable<T> list) : base(list) { }

		public new void Add(T item)
		{
			base.Add(item);

			item.PropertyChanged += OnCollectionPropertyChange;
		}

		private void OnCollectionPropertyChange(object sender, PropertyChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, sender));
		}
	}
}
