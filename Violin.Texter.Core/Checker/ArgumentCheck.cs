using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Checker
{
	public static class ArgumentCheck
	{
		public static void CheckNull(params object[] args)
		{
			var nullArgument = args.Where(arg => arg.IsNull()).Select(arg => arg);

			foreach (var arg in nullArgument)
			{
				throw new ArgumentNullException();
			}
		}

		public static bool IsNull<T>(this T obj)
		{
			return ReferenceEquals(obj, null);
		}
	}
}
