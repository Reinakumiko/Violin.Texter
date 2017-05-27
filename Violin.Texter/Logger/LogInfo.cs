using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Logger
{
	public class LogInfo<T>
	{
		public T Data { get; set; }
		public SystemRunningInfo RunningInfo { get; set; }
	}
}
