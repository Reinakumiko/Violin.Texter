using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Classes
{
	public class SymbolCount
	{
		public char Symbol { get; set; }
		public int Count { get; set; }

		public int IntSymbol { get { return (int)Symbol; } }

		public override string ToString()
		{
			return $"{Symbol}(0x{(int)Symbol:X}): {Count}";
		}
	}
}