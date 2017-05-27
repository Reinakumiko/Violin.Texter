using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Logger
{
	public class ExceptionInfo
	{
		public DateTime HappenIn { get; set; }
		public bool IsCrash { get; set; }
		public Exception Exception { get; set; }
	}
}
