using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Translations
{
	public class TranslationStateComparer
	{
		public static List<TranslationState> _stateSort = new List<TranslationState>()
		{
			TranslationState.TranslateUpdated,
			TranslationState.OriginUpdated,
			TranslationState.SameWithOrigin,
			TranslationState.New,
			TranslationState.Empty,
			TranslationState.ChangedNotSave,
			TranslationState.Changed
		};

		/// <summary>
		/// 以正序比较两个状态的大小
		/// </summary>
		/// <param name="x">第一个要比较的对象</param>
		/// <param name="y">第二个要比较的对象</param>
		/// <returns>小于 0 则 x 小于 y， 等于 0 则 x 等于 y， 大于 0 则 x 大于 y。</returns>
		public static int Compare(TranslationState x, TranslationState y)
		{
			return _stateSort.IndexOf(x) - _stateSort.IndexOf(y);
		}

		/// <summary>
		/// 以倒叙比较两个状态的大小
		/// </summary>
		/// <param name="x">第一个要比较的对象</param>
		/// <param name="y">第二个要比较的对象</param>
		/// <returns>小于 0 则 y 小于 x， 等于 0 则 y 等于 x， 大于 0 则 y 大于 x。</returns>
		public static int CompareDesc(TranslationState x, TranslationState y)
		{
			return _stateSort.IndexOf(y) - _stateSort.IndexOf(x);
		}
	}
}
