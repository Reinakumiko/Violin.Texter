using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Violin.Texter.Core.ContentIndex;
using Violin.Texter.Core.Translations;

namespace Violin.Texter.Core.Enumeration
{
	public class TranslationEnumeration<T> where T : Translation
	{
		/// <summary>
		/// 保存在迭代器里的集合
		/// </summary>
		private IList<T> _list;

		/// <summary>
		/// 当前已循环到的位置
		/// </summary>
		private int _index = -1;

		/// <summary>
		/// 上一次循环到的位置 (这个有什么用我也不知道)
		/// </summary>
		private int _lastindex = -1;

		/// <summary>
		/// 公开可读取迭代器的List (这个List可以设置成只读不可修改吗?)
		/// </summary>
		public IList<T> List {
			get
			{
				return _list;
			}
		}

		/// <summary>
		/// 当前所循环到的项
		/// </summary>
		public T Current
		{
			get
			{
				return GetCurrent();
			}
		}

		/// <summary>
		/// 用一个 <see cref="IList{T}"/> 初始化翻译的可遍历迭代器
		/// </summary>
		/// <param name="list"></param>
		public TranslationEnumeration(IList<T> list)
		{
			this._list = list;
		}

		public KeyValuePair<int, T> GetNext(string pattern, IndexOptions options)
		{
			while (MoveNext(options))
			{
				var current = Current;

				if ((options.IsUseRegex && RegexMatchTranslation(pattern, current, options)) || (!options.IsUseRegex && StringMatchTranslation(pattern, current, options)))
				{
					//返回一个键值对
					return new KeyValuePair<int, T>(_index, _list[_index]);
				}
			}

			return new KeyValuePair<int, T>(-1, null);
		}

		public bool MoveNext(IndexOptions options)
		{
			//将上一次的索引保存
			_lastindex = _index;

			//判断下一位置的正反向
			_index += options.Direction == IndexDirection.Next ? 1 : -1;

			//判断是否超出索引
			return !IsOutOfIndex(_index);
		}

		public void Reset(bool top = true)
		{
			_index = top
				   ? -1
				   : _list.Count - 1;
		}

		public void SetIndex(uint index)
		{
			_index = (int)index;
		}

		private bool IsOutOfIndex(int index)
		{
			return _index < 0 || _index >= _list.Count;
		}

		private T GetCurrent()
		{
			if (IsOutOfIndex(_index))
				throw new IndexOutOfRangeException();

			return _list[_index];
		}

		private bool RegexMatchTranslation(string pattern, Translation translation, IndexOptions options)
		{
			//Initailize Regex
			var reg = new Regex(pattern, options.IsSensitiveCase ? RegexOptions.None : RegexOptions.IgnoreCase);

			//Match Result
			bool anyMatched = false;

			//Just match Key
			if (options.Zone == IndexZones.KeyOnly || options.Zone == IndexZones.All)
				anyMatched |= reg.IsMatch(translation.Key);

			//Just match Content
			if (!anyMatched && (options.Zone == IndexZones.ContentOnly || options.Zone == IndexZones.All))
				anyMatched |= (reg.IsMatch(translation.Text) || reg.IsMatch(translation.Translated ?? string.Empty)); //可能会出现没有翻译文本的情况, 这种时候给个空字符串匹配吧

			//The match result
			return anyMatched;
		}

		private bool StringMatchTranslation(string pattern, Translation translation, IndexOptions options)
		{
			bool anyMatched = false;

			var ignoreCase = options.IsSensitiveCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			if (options.Zone == IndexZones.KeyOnly || options.Zone == IndexZones.All)
				anyMatched |= (translation.Key.IndexOf(pattern, ignoreCase) != -1);

			if (!anyMatched && (options.Zone == IndexZones.ContentOnly || options.Zone == IndexZones.All))
				anyMatched |= ((!string.IsNullOrWhiteSpace(translation.Text) && translation.Text.IndexOf(pattern, ignoreCase) != -1) || 
							  (!string.IsNullOrWhiteSpace(translation.Translated) && translation.Translated.IndexOf(pattern, ignoreCase) != -1));

			return anyMatched;
		}
	}
}
