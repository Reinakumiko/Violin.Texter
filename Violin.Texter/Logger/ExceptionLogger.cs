using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Violin.Texter.Logger
{
	public static class ExceptionLogger
	{
		static public string Name
		{
			get { return $"exception_{DateTime.Now:yyyyMMdd}.log"; }
		}

		static public string Path
		{
			get { return "./logs"; }
		}

		static public void Log(this Stream stream, LogInfo<ExceptionInfo> logInfo)
		{
			using (var gzip = new GZipStream(stream, CompressionMode.Compress))
			{
				var encoded = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(logInfo));
				gzip.Write(encoded, 0, encoded.Length);
				gzip.WriteByte(Convert.ToByte(','));
			}
		}

		static public void Log(LogInfo<ExceptionInfo> logInfo)
		{
			Log(Path, Name, logInfo);
		}

		static public void Log(string path, LogInfo<ExceptionInfo> logInfo)
		{
			Log(path, Name, logInfo);
		}

		static public void Log(string path, string fileName, LogInfo<ExceptionInfo> logInfo)
		{
			var filePath = $"{path}/{fileName}";
			var fileInfo = new FileInfo(filePath);

			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			if (!fileInfo.Exists)
				fileInfo.Create().Close();

			using (var file = fileInfo.Open(FileMode.Append, FileAccess.Write))
			{
				Log(file, logInfo);
			}
		}
	}
}
