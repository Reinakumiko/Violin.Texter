﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Component
{
	public class FileListItem
	{
		public string FullName { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
