using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Logger
{
	public class SystemRunningInfo
	{
		/// <summary>
		/// 当前程序运行目录
		/// </summary>
		public string CurrentDirectory
		{
			get { return Environment.CurrentDirectory; }
		}

		/// <summary>
		/// 当前系统版本
		/// </summary>
		public string SystemVersion
		{
			get
			{
				var version = Environment.OSVersion;

				return $"{version}({version.Version} {version.Platform} {version.ServicePack})";
			}
		}

		/// <summary>
		/// 系统已运行的时间(秒)
		/// </summary>
		public string SystemStartingTime
		{
			get { return Environment.TickCount.ToString(); }
		}
	}
}
