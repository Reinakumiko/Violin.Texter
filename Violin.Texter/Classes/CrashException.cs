using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Classes
{
	[Serializable]
	public class CrashException : Exception
	{
		public CrashException(bool crash) { Crash = crash; }
		public CrashException(string message, bool crash) : base(message) { Crash = crash; }
		public CrashException(string message, bool crash, Exception inner) : base(message, inner) { Crash = crash; }

		public bool Crash { get; set; }

		protected CrashException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
