using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Violin.Texter.Core.Notify;
using Violin.Texter.Core.Translations;

namespace Violin.Texter.Core.Progresses
{
	/// <summary>
	/// 编辑进度
	/// </summary>
	public class EditProgress
	{
		/// <summary>
		///	可翻译的文本对象
		/// </summary>
		public NotifyList<Translation> Translations { get; set; }

		/// <summary>
		/// 该文本的原文
		/// </summary>
		public string OriginContent { get; set; }

		/// <summary>
		/// 该文本的文件名
		/// </summary>
		public string OriginName { get; set; }

		/// <summary>
		/// 表示当前进度的状态
		/// </summary>
		[JsonIgnore]
		public EditProgressState State { get; set; }

		/// <summary>
		/// 进度所在的物理位置
		/// </summary>
		[JsonIgnore]
		public string OpenPath { get; set; }
	}
}
