using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Violin.Texter.Classes;
using Violin.Texter.Logger;
using Violin.Texter.Progress;
using Violin.Texter.UserWindows;

namespace Violin.Texter
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private readonly string _mainTitle = "Wawwawa";

		/// <summary>
		/// 当进度发生变动时执行的委托
		/// </summary>
		/// <param name="edirProgress">发生变动的进度</param>
		public delegate void OnEditProgressChanged(EditProgress edirProgress);

		/// <summary>
		/// 当进度发生更改时触发的事件
		/// </summary>
		public event OnEditProgressChanged OnEditProgressChangedEventHandle;
		
		/// <summary>
		/// 获取或设置当前进度的修改状态
		/// </summary>
		public bool IsProgressChanged
		{
			get
			{
				return EditProgress.State == ProgressState.Saved;
			}
			set
			{
				if (!value)
					EditProgress.State = ProgressState.Saved;
			}
		}


		/// <summary>
		/// 当前被选中的翻译文本
		/// </summary>
		public TranslationItem CurrentItem { get; set; }

		/// <summary>
		/// 当前激活进度的私有成员
		/// </summary>
		private EditProgress _editProgress = null;

		/// <summary>
		/// 获取或设置编辑进度
		/// </summary>
		public EditProgress EditProgress
		{
			get { return _editProgress; }
			set
			{
				_editProgress = value;
				OnEditProgressChangedEventHandle?.Invoke(_editProgress);
			}
		}

		/// <summary>
		/// 初始化应用程序主窗体
		/// </summary>
		public MainWindow()
		{
			//初始化窗体控件
			InitializeComponent();

			//设置为主窗体关闭后结束程序
			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

			//将本窗体设置为主窗体 (为什么CheckSymbol会变成主窗体???)
			Application.Current.MainWindow = this;

			//捕捉应用程序域未能处理的异常(将会引起程序崩溃)
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				var exception = e.ExceptionObject as Exception;
				UnhandledExceptionLog(exception, true); //true -> crash on dialog close

				DisplayUnhandleExceptionDialog(exception, e.IsTerminating);
			};

			//捕捉应用程序未拦截的异常(可被拦截)
			Application.Current.DispatcherUnhandledException += (sender, e) =>
			{
				var exception = e.Exception;
				if (exception is CrashException)
					e.Handled = !(e.Exception as CrashException).Crash;
				else
					e.Handled = true;

				if (e.Handled)
				{
					UnhandledExceptionLog(exception, !e.Handled); //false -> not crash
					DisplayUnhandleExceptionDialog(exception);
				}
			};

			OnEditProgressChangedEventHandle += editprogress =>
			{
				//重新绑定列表
				SetListItems(EditProgress?.Translations);

				//重置编辑框
				EditorReset();
			};
		}

		private void keyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CurrentItem = keyList.SelectedItem as TranslationItem;

			//获取已翻译的字符串
			_transTranslated.Text = CurrentItem?.Value.Translated;

			//编辑框焦点
			_transTranslated.Focus();

			//光标设置到末尾
			_transTranslated.SelectionStart = _transTranslated.Text.Length;
		}

		private void _transTranslated_TextChanged(object sender, TextChangedEventArgs e)
		{
			var currentText = (sender as TextBox).Text;
			var transItem = CurrentItem?.Value;

			if (transItem == null || transItem.Translated == currentText || keyList.Items.Count < 1)
				return;

			CurrentItem.State = TranslateState.ChangedNotSave;
			transItem.Translated = currentText;
			IsProgressChanged = true;
		}

		/// <summary>
		/// 创建新进度
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void CreateProgress_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress != null)
				await await CloseCurrentProgress();

			EditProgress = new EditProgress()
			{
				Translations = new List<TranslationItem>()
			};
		}

		/// <summary>
		/// 打开进度
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void OpenProgress_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress != null)
				await await CloseCurrentProgress();

			IsProgressChanged = false;
			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = false;
				_fileDialog.Title = "选择需要打开的文件";

				_fileDialog.Filters.Add(new CommonFileDialogFilter("编辑进度", "*.edp"));

				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					var file = new FileInfo(_fileDialog.FileName);
					using (var fileStream = file.OpenRead())
					using (var gzip = new GZipStream(fileStream, CompressionMode.Decompress))
					using (var reader = new StreamReader(gzip))
					{
						var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));

						//填充编辑进度
						JsonConvert.PopulateObject(decoded, EditProgress);

						if (EditProgress != null) //一般情况下都不会为空
							EditProgress.OpenPath = file.FullName;
					}
				}
			}
		}
	}
}
