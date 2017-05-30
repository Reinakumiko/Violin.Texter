using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Classes
{
    /// <summary>
    /// 表示一个翻译段落当前的状态
    /// </summary>
	public enum TranslateState
	{
        /// <summary>
        /// 表示该文本是新加入的
        /// </summary>
        New,

        /// <summary>
        /// 表示该翻译文本未被翻译
        /// </summary>
		Empty,

        /// <summary>
        /// 表示该翻译文本已存在翻译译文
        /// </summary>
		Changed,

        /// <summary>
        /// 表示该翻译文本已存在翻译译文, 但未保存到进度中
        /// </summary>
		ChangedNotSave,

        /// <summary>
        /// 表示该翻译文本的原文已被更新
        /// </summary>
        OriginUpdated,

        /// <summary>
        /// 表示该翻译文本的译文已被更新
        /// </summary>
        TranslateUpdated
	}
}
