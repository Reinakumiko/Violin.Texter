using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Progresses
{
	/// <summary>
	/// 表示一个进度目前的状态
	/// </summary>
	public enum EditProgressState
	{
		/// <summary>
		/// 未打开进度
		/// </summary>
		None,

		/// <summary>
		/// 进度为空
		/// </summary>
		Empty,

		/// <summary>
		/// 进度已保存
		/// </summary>
		Saved,

		/// <summary>
		/// 进度未保存
		/// </summary>
		NotSave,
	}
}
