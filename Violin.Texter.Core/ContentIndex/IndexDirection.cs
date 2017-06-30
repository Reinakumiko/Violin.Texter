using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.ContentIndex
{
	/// <summary>
	/// 表示定位时的搜索方向
	/// </summary>
	public enum IndexDirection
	{
		/// <summary>
		/// 往当前位置之前
		/// </summary>
		Previous,

		/// <summary>
		/// 往当前位置之后
		/// </summary>
		Next
	}
}
