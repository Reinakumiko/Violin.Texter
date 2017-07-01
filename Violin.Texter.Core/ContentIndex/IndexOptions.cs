using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.ContentIndex
{
	public struct IndexOptions
	{
		public IndexDirection Direction { get; set; }
		public IndexZones Zone { get; set; }
		public bool IsUseRegex { get; set; }
		public bool IsSensitiveCase { get; set; }
	}
}
