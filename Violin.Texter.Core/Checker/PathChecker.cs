using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Checker
{
	public static class PathChecker
	{
		public static void Check(this FileInfo fileInfo)
		{
			fileInfo.Directory.CheckDirectory();

			if (!fileInfo.Exists)
				fileInfo.Create().Dispose();
		}

		public static void Check(string path)
		{
			var fileInfo = new FileInfo(path);

			fileInfo.Check();
		}

		public static void CheckDirectory(this DirectoryInfo dirInfo)
		{
			if (!dirInfo.Exists)
				dirInfo.Create();
		}

		public static void CheckDirectory(string path)
		{
			var dirInfo = new DirectoryInfo(path);
			dirInfo.CheckDirectory();
		}
	}
}
