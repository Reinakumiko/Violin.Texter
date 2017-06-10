using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Violin.Texter.Core.Translations;

namespace Violin.Texter.Core.RenderBrushes
{
	public class TranslateStateBrush
	{
		/// <summary>
		/// 可将 ARGB 转换为 <see cref="Brush"/> 类型的转换器
		/// </summary>
		private static BrushConverter _converter = new BrushConverter();

		/// <summary>
		///	表示已与翻译状态关联的颜色刷
		/// </summary>
		static public Dictionary<TranslationState, Brush> Brushes { get; set; }

		static TranslateStateBrush()
		{
			InitailizeBrushes();
		}

		/// <summary>
		/// 初始化颜色刷子
		/// </summary>
		private static void InitailizeBrushes()
		{
			Brushes = new Dictionary<TranslationState, Brush>()
			{
				// 更新/新增条目
				{ TranslationState.New, ColoredBrushes.Brushes["Red"] },
				{ TranslationState.OriginUpdated, ColoredBrushes.Brushes["Red"] },
				{ TranslationState.TranslateUpdated, ColoredBrushes.Brushes["Red"] },

				// 原始/修改条目
				{ TranslationState.Changed, ColoredBrushes.Brushes["Green"] },
				{ TranslationState.ChangedNotSave, ColoredBrushes.Brushes["Amber"] },
				{ TranslationState.Empty, ColoredBrushes.Brushes["Grey"] },
			};
		}
	}
}
