using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Notify
{
	/// <summary>
	/// 表示一个可供元素修改通知的通知列表
	/// </summary>
	/// <typeparam name="T">表示一个包含属性通知的对象</typeparam>
	public class NotifyList<T> : List<T>, IList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, INotifyCollectionChanged
		where T : INotifyPropertyChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public NotifyList() : base () { }
		public NotifyList(int capacity) : base(capacity) { }
		public NotifyList(IEnumerable<T> list) : base(list) { }

		public new void Add(T item)
		{
			if (item == null)
				return;

			//添加进集合
			base.Add(item);

			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
		}

		public new bool Remove(T item)
		{
			if (item == null)
				return false;

			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

			//从集合中移除对象
			return base.Remove(item);
		}
	}
}
