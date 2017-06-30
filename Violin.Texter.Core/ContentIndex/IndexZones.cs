using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.ContentIndex
{
	/// <summary>
	/// 表示定位时的范围
	/// </summary>
	public enum IndexZones
	{
		/// <summary>
		/// 包含键与内容
		/// </summary>
		All,

		/// <summary>
		/// 只包含键
		/// </summary>
		KeyOnly,

		/// <summary>
		/// 只包含内容
		/// </summary>
		ContentOnly
	}
}
