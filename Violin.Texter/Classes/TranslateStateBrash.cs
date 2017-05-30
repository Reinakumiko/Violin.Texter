using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Violin.Texter.Classes
{
	public class TranslateStateBrush
	{
		public string BrushName { get; set; }
		public TranslateState State { get; set; }
		public Brush Brush { get { return GetStateBrush(); } }

		private Brush GetStateBrush()
		{
			var brushString = "#00000000";

			switch (State)
			{
                case TranslateState.New:
                case TranslateState.OriginUpdated:
                case TranslateState.TranslateUpdated:
                    BrushName = "Red";
                    brushString = "#FFE51400";
                    break;
				case TranslateState.Empty:
					BrushName = "Grey";
					brushString = "#FFCFCFCF";
					break;
				case TranslateState.Changed:
					BrushName = "Green";
					brushString = "#FF60A917";
					break;
				case TranslateState.ChangedNotSave:
					BrushName = "Amber";
					brushString = "#FFF0A30A";
					break;
				default:
					throw new ArgumentOutOfRangeException("指定的状态不在预料的范围内。");
			}

			return new BrushConverter().ConvertFromString(brushString) as Brush;
		}
	}
}
