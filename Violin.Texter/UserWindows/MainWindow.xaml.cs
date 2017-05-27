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
using Violin.Texter.UserWindows;

namespace Violin.Texter
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			//设置为主窗体关闭后结束程序
			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

			//将本窗体设置为主窗体 (为什么CheckSymbol会变成主窗体???)
			Application.Current.MainWindow = this;


			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				var exception = e.ExceptionObject as Exception;
				UnhandledExceptionLog(exception, true); //true -> crash on dialog close

				DisplayUnhandleExceptionDialog(exception, e.IsTerminating);
			};

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
		}

		private EditProgress _editProgress = null;
		public EditProgress EditProgress
		{
			get { return _editProgress; }
			set
			{
				_editProgress = value;

				//重新绑定列表

				keyList.ItemsSource = new ObservableCollection<TranslationItem>(EditProgress?.Translations ?? new List<TranslationItem>());

				//重置编辑框
				EditorReset();
			}
		}

		private void Open_Click(object sender, RoutedEventArgs e)
		{
			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = false;
				_fileDialog.Title = "选择需要编辑的文件";

				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					EditProgress = GetContent(_fileDialog.FileName);
				}
			}
		}

		private void DisplayUnhandleExceptionDialog(Exception ex, bool isTerminating = false)
		{
			new UnhandledException(ex, isTerminating) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
		}

		private void UnhandledExceptionLog(Exception ex, bool crash)
		{
			ExceptionLogger.Log(new LogInfo<ExceptionInfo>() {
				RunningInfo = new SystemRunningInfo(),
				Data = new ExceptionInfo() {
					Exception = ex,
					HappenIn = DateTime.Now,
					IsCrash = crash
				}
			});
		}

		private void EditorReset()
		{
			_transOrigin.Clear();
			_transTranslated.Clear();
		}

		private EditProgress GetContent(string file)
		{
			var fileInfo = new FileInfo(file);
			using (var reader = fileInfo.OpenText())
			{

				var fileContent = reader.ReadToEnd();

				var regMatchKey = new Regex(@"[\d\w\._]+");
				var regMatchValue = new Regex("\".*\"");
				var regMatchLevel = new Regex(@"(?<=:)\d");

				var matchedResult = Regex.Matches(fileContent, @"[\d\w\._]+?\:\t?\d\s?" + "\".*\"").Cast<Match>().Select(m =>
				{
					var value = m.Value ?? string.Empty;
					var translate = new TranslationItem()
					{
						Key = regMatchKey.Match(value).Value,
						Value = new Translation()
						{
							Level = Convert.ToInt32(regMatchLevel.Match(value).Value),
							Text = regMatchValue.Match(value).Value
						}
					};

					return translate;
				});

				return new EditProgress()
				{
					OriginName = fileInfo.Name,
					OriginContent = fileContent,
					Translations = matchedResult.ToList()
				};
			}
		}

		public bool IsProgressChanged
		{
			get
			{
				return EditProgress.Translations.Any(t => t.IsChanged);
			}
			set
			{
				if (!value)
					EditProgress?.Translations.Where(t => t.IsChanged).ToList().ForEach(t => t.IsChanged = true);
			}
		}

		public TranslationItem CurrentItem { get; set; }
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

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void SplitComponent_Click(object sender, RoutedEventArgs e)
		{
			new Component.MainWindow() { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
		}

		private void Undo_Click(object sender, RoutedEventArgs e)
		{
			_transTranslated.Undo();
		}

		private void Redo_Click(object sender, RoutedEventArgs e)
		{
			_transTranslated.Redo();
		}

		private void Cut_Click(object sender, RoutedEventArgs e)
		{
			_transTranslated.Cut();
		}

		private void Copy_Click(object sender, RoutedEventArgs e)
		{
			_transTranslated.Copy();
		}

		private void Paste_Click(object sender, RoutedEventArgs e)
		{
			_transTranslated.Paste();
		}

		private void SelectAll_Click(object sender, RoutedEventArgs e)
		{
			_transTranslated.SelectAll();
		}

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

						if ((EditProgress = JsonConvert.DeserializeObject<EditProgress>(decoded)) != null)
							EditProgress.OpenPath = file.FullName;
					}
				}
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				this.ShowMessageAsync("无法保存", "当前没有已打开任务进度。");
				return;
			}

			ProgressSave();
		}

		private void SaveAs_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				this.ShowMessageAsync("无法保存", "当前没有已打开任务进度。");
				return;
			}

			ProgressSaveAs();
		}

		private bool ProgressSave()
		{
			IsProgressChanged = false;

			//如果文件未保存过则转为另存为执行
			return string.IsNullOrWhiteSpace(EditProgress.OpenPath)
						 ? ProgressSaveAs()
						 : SaveProgress(EditProgress.OpenPath);
		}

		private bool ProgressSaveAs()
		{
			IsProgressChanged = false;

			using (var dialog = new CommonSaveFileDialog())
			{
				var file = new FileInfo(EditProgress.OriginName);
				var fileName = file.Name.Replace(file.Extension, "");

				dialog.DefaultExtension = ".edp";
				dialog.DefaultFileName = $"{fileName}_progress";
				dialog.Filters.Add(new CommonFileDialogFilter("编辑进度", "*.edp"));

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					return SaveProgress(dialog.FileName);
				}
			}

			return false;
		}

		private void CheckExists(FileInfo fileInfo)
		{
			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			if (!fileInfo.Exists)
				fileInfo.Create().Dispose();
		}

		private bool SaveProgress(string path)
		{
			var fileInfo = new FileInfo(path);
			CheckExists(fileInfo);

			try
			{
				using (var file = fileInfo.Open(FileMode.Truncate, FileAccess.Write))
				using (var gzip = new GZipStream(file, CompressionMode.Compress))
				{
					var jsonString = JsonConvert.SerializeObject(EditProgress);
					var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

					var byteContent = Encoding.UTF8.GetBytes(encoded);
					gzip.Write(byteContent, 0, byteContent.Length);
				}
			}
			catch (Exception)
			{
				return false;
			}

			EditProgress.Translations.Where(t => t.State == TranslateState.ChangedNotSave).Any(t => (t.State = TranslateState.Changed) == TranslateState.Changed);
			return true;
		}

		private async Task<Task> CloseCurrentProgress()
		{
			Func<Task<MessageDialogResult>> resultAction = async () =>
			{
				if (IsProgressChanged) //不这么写这个消息框Task就算不在条件内也会触发
				{
					var taskResult = this.ShowMessageAsync("保存进度", "当前进度未保存，是否要保存当前进度。", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
					{
						AffirmativeButtonText = "是",
						NegativeButtonText = "否"
					});

					//将异步等待结果返回
					return await taskResult;
				}

				//返回默认
				return default(MessageDialogResult);
			};

			Action<object> taskAction = (result) =>
			{
				switch ((MessageDialogResult)result)
				{
					case MessageDialogResult.Affirmative:
						if (!ProgressSave())
							break;
						return;
					case MessageDialogResult.Negative:
					default:
						break;
				}

				this.Invoke(() =>
				{
					EditProgress = null;
				});
			};

			return Task.Factory.StartNew(taskAction, await resultAction());
		}

		private CheckSymbol SymbolDialog = null;
		private void CheckSymbol_Click(object sender, RoutedEventArgs e)
		{
			if (SymbolDialog == null)
			{
				SymbolDialog = new CheckSymbol()
				{
					WindowStartupLocation = WindowStartupLocation.CenterOwner
				};
			}

			SymbolDialog.Owner = this;
			SymbolDialog.Show();
		}

		private async void CloseProgress_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				await this.ShowMessageAsync("操作失败", "当前未打开任何进度。");
				return;
			}

			await await CloseCurrentProgress();
		}

		private void ExportOrigin_Click(object sender, RoutedEventArgs e)
		{
			SaveContent(() => EditProgress.OriginContent);
		}

		private void ExportTranslated_Click(object sender, RoutedEventArgs e)
		{
			SaveContent(() =>
			{
				var originContent = EditProgress.OriginContent;
				var changedList = EditProgress.Translations.Where(t => t.State == TranslateState.Changed).ToList();

				changedList.ForEach(t =>
				{
					var regMatch = $@"(?<={t.Key}\:\t?\d\s?)" + "\".*\"";

					originContent = Regex.Replace(originContent, regMatch, t.GetTranslateRendered());
				});

				return originContent;
			});
		}

		private async void SaveContent(Func<string> action)
		{
			if (EditProgress == null)
			{
				await this.ShowMessageAsync("无效的操作", "当前未打开任何进度，无法导出文本。");
				return;
			}

			using (var dialog = new CommonSaveFileDialog())
			{
				dialog.DefaultExtension = ".yml";
				dialog.Filters.Add(new CommonFileDialogFilter("Yaml 源文件", "*.yml"));

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					var file = new FileInfo(dialog.FileName);
					CheckExists(file);

					using (var fileStream = file.Open(FileMode.Truncate, FileAccess.Write))
					using (var writer = new StreamWriter(fileStream))
					{
						writer.WriteLine(action?.Invoke());
					}
				}
			}
		}

		private void BugReport_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://github.com/Reinakumiko/Violin.Texter/issues/new");
		}

		private async void About_Click(object sender, RoutedEventArgs e)
		{
			//await this.ShowMessageAsync("About", "Copyright © ViolinStudio. \r\nAll Rights Reserved.");
			await this.ShowMessageAsync("About", "喵喵喵喵喵????");
		}

		#region 符号检查绑定
		private void EditBox_GotFocus(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;

			SymbolDialog?.OnFocus(textBox);
		}

		private void EditBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;

			SymbolDialog?.OnLost(textBox);
		}

		#endregion
		
		#region 异常处理测试
		private void ThrowException_Click(object sender, RoutedEventArgs e)
		{
			throw new CrashException("异常测试", false);
		}

		private void ThrowCrashException_Click(object sender, RoutedEventArgs e)
		{
			throw new CrashException("异常测试", true);
		}

		private void ThrowDeepException_Click(object sender, RoutedEventArgs e)
		{
			var callbackDeepCount = 0;

			Action DeepCallbackLoop = null;

			//深层调用结构扩充(递归)
			DeepCallbackLoop = () =>
			{
				if (callbackDeepCount++ > 70)
				{
					this.Invoke(() =>
					{
						throw new CrashException("异常测试", false);
					});
				}
				else
					DeepCallbackLoop();
			};

			this.Invoke(DeepCallbackLoop);
		}
		#endregion
	}
}
