using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Violin.Texter.Core.RenderBrushes
{
	/// <summary>
	/// 包含已被命名颜色的颜色刷
	/// </summary>
	public class ColoredBrushes
	{
		/// <summary>
		/// 可将 ARGB 转换为 <see cref="Brush"/> 类型的转换器
		/// </summary>
		private static BrushConverter _converter = new BrushConverter();

		/// <summary>
		///	表示已被命名的刷子集合
		/// </summary>
		static public Dictionary<string, Brush> Brushes { get; set; }
		
		static ColoredBrushes()
		{
			InitailizeBrushes();
		}

		/// <summary>
		/// 初始化颜色刷子
		/// </summary>
		private static void InitailizeBrushes()
		{
			Brushes = new Dictionary<string, Brush>()
			{
				{"Red", _converter.ConvertFrom("#FFE51400") as Brush },
				{"Grey", _converter.ConvertFrom("#FFCFCFCF") as Brush },
				{"Green", _converter.ConvertFrom("#FF60A917") as Brush },
				{"Amber", _converter.ConvertFrom("#FFF0A30A") as Brush },
				{"Mauve", _converter.ConvertFrom("#CC76608A") as Brush },
				{"Olive", _converter.ConvertFrom("#CC6D8764") as Brush }
			};
		}
	}
}
