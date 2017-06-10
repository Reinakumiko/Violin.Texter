using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Violin.Texter.Core.Checker;

namespace Violin.Texter.Core.StreamWorker
{
	public static class GZipExtension
	{
		/// <summary>
		/// 将二进制内容以 <see cref="CompressionMode.Compress"/> 的方式写入到 <see cref="GZipStream"/>
		/// </summary>
		/// <param name="path">文件的存储路径</param>
		/// <param name="content">写入的内容</param>
		public static void GZipSave(string path, IEnumerable<byte> content)
		{
			GZipSave(new FileInfo(path), content);
		}

		/// <summary>
		/// 将二进制内容以 <see cref="CompressionMode.Compress"/> 的方式写入到 <see cref="GZipStream"/>
		/// </summary>
		/// <param name="info">文件的存储路径信息</param>
		/// <param name="content">写入的内容</param>
		public static void GZipSave(FileInfo info, IEnumerable<byte> content)
		{
			info.Check();

			using (var file = info.Open(FileMode.Truncate, FileAccess.Write))
			{
				file.GZipSave(content);
			}
		}

		/// <summary>
		/// 将文本内容以 <see cref="CompressionMode.Compress"/> 的方式写入到 <see cref="GZipStream"/>
		/// </summary>
		/// <param name="path">文件的存储路径</param>
		/// <param name="content">写入的内容</param>
		public static void GZipSave(string path, string content)
		{
			GZipSave(new FileInfo(path), content);
		}

		/// <summary>
		/// 将文本内容以 <see cref="CompressionMode.Compress"/> 的方式写入到 <see cref="GZipStream"/>
		/// </summary>
		/// <param name="info">文件的存储路径信息</param>
		/// <param name="content">写入的内容</param>
		public static void GZipSave(FileInfo info, string content)
		{
			GZipSave(info, Encoding.UTF8.GetBytes(content));
		}

		/// <summary>
		/// 将二进制内容以 <see cref="CompressionMode.Compress"/> 的方式写入到 <see cref="Stream"/> 的 <see cref="GZipStream"/> 中
		/// </summary>
		/// <param name="stream">要写入的 <see cref="Stream"/></param>
		/// <param name="content">二进制数据源</param>
		public static void GZipSave(this Stream stream, IEnumerable<byte> content)
		{
			using (var gzipStream = new GZipStream(stream, CompressionMode.Compress))
			{
				var byteContent = content.ToArray();
				gzipStream.Write(byteContent, 0, byteContent.Length);
			}
		}

		/// <summary>
		/// 将文本内容以 <see cref="CompressionMode.Compress"/> 的方式写入到 <see cref="Stream"/> 的 <see cref="GZipStream"/> 中
		/// </summary>
		/// <param name="stream">要写入的 <see cref="Stream"/></param>
		/// <param name="content">文本数据源</param>
		public static void GZipSave(this Stream stream, string content)
		{
			stream.GZipSave(Encoding.UTF8.GetBytes(content));
		}

		/// <summary>
		/// 以 <see cref="CompressionMode.Decompress"/> 的方式从 <see cref="GZipStream"/> 中读取内容
		/// </summary>
		/// <param name="info">文件的路径</param>
		/// <returns>文件内容</returns>
		public static string GZipRead(this FileInfo info)
		{
			info.Check();

			using (var file = info.OpenRead())
			using (var gzip = new GZipStream(file, CompressionMode.Decompress))
			using (var reader = new StreamReader(gzip))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// 以 <see cref="CompressionMode.Decompress"/> 的方式从 <see cref="GZipStream"/> 中读取内容
		/// </summary>
		/// <param name="path">文件的路径</param>
		/// <returns>文件内容</returns>
		public static string GZipRead(string path)
		{
			return new FileInfo(path).GZipRead();
		}
	}
}
