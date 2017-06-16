using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Enumeration
{
	public static class EnumerationExtension
	{
		/// <summary>
		/// 对每个 <see cref="IEnumerable{T}"/> 执行指定操作
		/// </summary>
		/// <typeparam name="T"><see cref="IEnumerable{T}"/> 中每个元素的类型</typeparam>
		/// <param name="collection">要遍历的 <see cref="IEnumerable{T}"/> 实例</param>
		/// <param name="callback">要对每个 <see cref="IEnumerable{T}"/> 元素执行的 <see cref="Action{T}"/> 委托</param>
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> callback)
		{
			foreach (var item in collection)
			{
				callback(item);
			}
		}

		/// <summary>
		/// 对每个 <see cref="IEnumerable"/> 执行指定操作
		/// </summary>
		/// <param name="collection">要遍历的 <see cref="IEnumerable"/> 实例</param>
		/// <param name="callback">要对每个 <see cref="IEnumerable"/> 元素执行的 <see cref="Action{T}"/> 委托</param>
		public static void ForEach(this IEnumerable collection, Action<object> callback)
		{
			foreach (var item in collection)
			{
				callback(item);
			}
		}
	}
}
