﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Violin.Texter.Core.Dialogs
{
	public class DialogResult
	{
		public CommonFileDialogResult Result { get; set; }
		public string Data { get; set; }
	}
}