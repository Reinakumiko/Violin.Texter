using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.EventArguments
{
	public class SelectResultEventArgs<T> : EventArgs
	{
		public T Result { get; set; }
	}
}
