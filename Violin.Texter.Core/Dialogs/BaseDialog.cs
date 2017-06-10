using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using Violin.Texter.Core.EventArguments;

namespace Violin.Texter.Core.Dialogs
{
	/// <summary>
	/// 可调用对话框的基础类
	/// </summary>
	/// <typeparam name="T">匹配的对话框</typeparam>
	public abstract class BaseDialog<T> where T : CommonFileDialog
	{
		public T Dialog { get; set; }

		/// <summary>
		/// 当对话框执行完毕后执行结果时调用的委托
		/// </summary>
		/// <param name="dialog">发起的对话框</param>
		/// <param name="args">所传递的参数</param>
		public delegate void OnDialogCompleteEventHandle(T dialog, SelectResultEventArgs<DialogResult> args);

		/// <summary>
		/// 当对话框执行完毕后返回结果时触发
		/// </summary>
		public event OnDialogCompleteEventHandle OnDialogComplete;

		~BaseDialog()
		{
			Dialog.Dispose();
		}
	}
}
