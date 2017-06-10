using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Exceptions
{
	[Serializable]
	public class CrashException : Exception
	{
		public bool Crash { get; set; }
		public CrashException(bool crash) { Crash = crash; }
		public CrashException(string message, bool crash) : base(message) { Crash = crash; }
		public CrashException(string message, bool crash, Exception inner) : base(message, inner) { Crash = crash; }
		protected CrashException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
