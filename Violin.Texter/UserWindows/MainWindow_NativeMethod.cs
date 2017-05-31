using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Violin.Texter.Classes;
using Violin.Texter.Logger;
using Violin.Texter.UserWindows;

namespace Violin.Texter
{
	public partial class MainWindow
	{

		/// <summary>
		/// 显示未处理的程序异常
		/// </summary>
		/// <param name="ex">未处理的程序异常</param>
		/// <param name="isTerminating">是否引起程序崩溃</param>
		private void DisplayUnhandleExceptionDialog(Exception ex, bool isTerminating = false)
		{
			new UnhandledException(ex, isTerminating) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
		}

		/// <summary>
		/// 记录程序发生的异常
		/// </summary>
		/// <param name="ex">程序发生的异常</param>
		/// <param name="crash">异常是否引起程序崩溃</param>
		private void UnhandledExceptionLog(Exception ex, bool crash)
		{
			ExceptionLogger.Log(new LogInfo<ExceptionInfo>()
			{
				RunningInfo = new SystemRunningInfo(),
				Data = new ExceptionInfo()
				{
					Exception = ex,
					HappenIn = DateTime.Now,
					IsCrash = crash
				}
			});
		}

		/// <summary>
		/// 重置编辑区
		/// </summary>
		private void EditorReset()
		{
			_transOrigin.Clear();
			_transTranslated.Clear();
		}

		/// <summary>
		/// 检查文件是否已创建
		/// </summary>
		/// <param name="fileInfo"></param>
		private void CheckExists(FileInfo fileInfo)
		{
			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			if (!fileInfo.Exists)
				fileInfo.Create().Dispose();
		}

		/// <summary>
		/// 保存进度
		/// </summary>
		/// <returns>是否成功写入硬盘</returns>
		private bool ProgressSave()
		{
			IsProgressChanged = false;

			//如果文件未保存过则转为另存为执行
			return string.IsNullOrWhiteSpace(EditProgress.OpenPath)
						 ? ProgressSaveAs()
						 : SaveProgress(EditProgress.OpenPath);
		}

		/// <summary>
		/// 进度另存为
		/// </summary>
		/// <returns></returns>
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
		
		private void SetListItems<T>(IEnumerable<T> list = null)
		{
			if (list != null && list.Count() > 0)
				keyList.ItemsSource = new ObservableCollection<T>(list);
			else
			{
				var fillString = string.Empty;

				if (EditProgress == null)
					fillString = "当前未打开任何进度文件。";
				else
					fillString = "当前列表内容为空。";

				keyList.ItemsSource = new string[] { fillString };
			}
		}

		private void UpdateEditList()
		{
			var editProgress = EditProgress;

			EditProgress = null;
			EditProgress = editProgress;
		}

		/// <summary>
		/// 导入文本内容
		/// </summary>
		/// <param name="file">要导入的文本内容</param>
		/// <param name="isTranslate">是否是翻译过的文本</param>
		private void ImportContent(string file, bool isTranslate)
		{
			var fileInfo = new FileInfo(file);
			using (var reader = fileInfo.OpenText())
			{
				//获取文本内容
				var fileContent = reader.ReadToEnd();

				//匹配规则
				var regMatchKey = new Regex(@"[\d\w\._]+");
				var regMatchValue = new Regex("\".*\"");
				var regMatchLevel = new Regex(@"(?<=:)\d");

				//匹配符合规则的文本段落
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
				}).ToList();

				//查找已存在的相同key并且文本内容不同的段落
				var updateResult = from import in matchedResult
								   from progress in EditProgress.Translations
								   where import.Key == progress.Key && (isTranslate ? (progress.Value.Translated != progress.Value.Translated) : (progress.Value.Text != import.Value.Text))
								   select import;

				//更新已存在的翻译段落(有更新的)
				updateResult.ToList().ForEach(t =>
				{
					var updateKey = EditProgress.Translations.Where(k => k.Key == t.Key).FirstOrDefault();

					if (isTranslate)
						updateKey.Value.Translated = t.Value.Translated;
					else
						updateKey.Value.Text = t.Value.Text;

					updateKey.State = isTranslate ? TranslateState.TranslateUpdated : TranslateState.OriginUpdated;
				});


				//查询已存在与导入的key差异
				var importKey = matchedResult.Select(t => t.Key).Distinct();
				var progressKey = EditProgress.Translations.Select(t => t.Key).Distinct();

				//添加新增的文本段落
				importKey.Except(progressKey).ToList().ForEach((k) =>
				{
					var importTranslate = matchedResult.Where(t => t.Key == k).FirstOrDefault();

					importTranslate.State = TranslateState.New;
					EditProgress.Translations.Add(importTranslate);
				});

				EditProgress.OriginName = fileInfo.Name;
				EditProgress.OriginContent = fileContent;
			}
		}

		private async void ImportPath(bool isTranslate)
		{
			var dialogController = await this.ShowProgressAsync("正在导入", "正在将文本导入到当前进度中...");
			dialogController.SetIndeterminate();

			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = false;
				_fileDialog.Title = "选择需要导入的文本";


				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					await Task.Run(() => ImportContent(_fileDialog.FileName, isTranslate));
				}
			}

			await dialogController.CloseAsync();
		}
	}
}
