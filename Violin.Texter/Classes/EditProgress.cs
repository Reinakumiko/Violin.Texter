using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Violin.Texter.Classes
{
	/// <summary>
	/// 编辑进度
	/// </summary>
	public class EditProgress
	{
		/// <summary>
		///	可翻译的文本对象
		/// </summary>
		public List<TranslationItem> Translations { get; set; }

		/// <summary>
		/// 该文本的原文
		/// </summary>
		public string OriginContent { get; set; }

		/// <summary>
		/// 该文本的文件名
		/// </summary>
		public string OriginName { get; set; }

		/// <summary>
		/// 进度所在的位置
		/// </summary>
		[JsonIgnore]
		public string OpenPath { get; set; }
	}
}
