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
using Violin.Texter.Core.Checker;
using Violin.Texter.Core.StreamWorker;
using Violin.Texter.Core.Translations;
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
		/// 保存进度
		/// </summary>
		/// <returns>是否成功写入硬盘</returns>
		private bool ProgressSave(EditProgress progress)
		{
			IsProgressChanged = false;

			//如果文件未保存过则转为另存为执行
			return string.IsNullOrWhiteSpace(progress.OpenPath)
						 ? ProgressSaveAs(progress)
						 : SaveProgress(progress, progress.OpenPath);
		}

		/// <summary>
		/// 进度另存为
		/// </summary>
		/// <returns></returns>
		private bool ProgressSaveAs(EditProgress progress)
		{
			IsProgressChanged = false;

			using (var dialog = new CommonSaveFileDialog())
			{
				var file = new FileInfo(progress.OriginName);
				var fileName = file.Name.Replace(file.Extension, "");

				dialog.DefaultExtension = ".edp";
				dialog.DefaultFileName = $"{fileName}_progress";
				dialog.Filters.Add(new CommonFileDialogFilter("编辑进度", "*.edp"));

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					return SaveProgress(progress, dialog.FileName);
				}
			}

			return false;
		}

		private bool SaveProgress(EditProgress progress, string path)
		{
			var fileInfo = new FileInfo(path);
			fileInfo.Check();

			try
			{
				var jsonString = JsonConvert.SerializeObject(progress);
				var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

				GZipExtension.GZipSave(fileInfo, encoded);
			}
			catch (Exception)
			{
				return false;
			}

			progress.Translations.Where(t => t.State != TranslationState.Changed).ForEach(t => t.State = TranslationState.Changed);
			return true;
		}

		private void SaveContent(Func<string> action)
		{
			using (var dialog = new CommonSaveFileDialog())
			{
				dialog.DefaultExtension = ".yml";
				dialog.Filters.Add(new CommonFileDialogFilter("Yaml 源文件", "*.yml"));

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					var file = new FileInfo(dialog.FileName);
					file.Check();

					using (var fileStream = file.Open(FileMode.Truncate, FileAccess.Write))
					using (var writer = new StreamWriter(fileStream))
					{
						writer.WriteLine(action?.Invoke());
					}
				}
			}
		}

		private async Task CloseCurrentProgress()
		{
			if (EditProgress != null)
				await CloseEditProgress(EditProgress);
		}

		private async Task CloseEditProgress(EditProgress progress)
		{
			var result = default(MessageDialogResult);

			if (IsProgressChanged) //不这么写这个消息框Task就算不在条件内也会触发
			{
				var taskResult = this.ShowMessageAsync("保存进度", "当前进度未保存，是否要保存当前进度。", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
				{
					AffirmativeButtonText = "是",
					NegativeButtonText = "否"
				});

				//将异步等待结果返回
				result = await taskResult;
			}

			switch (result)
			{
				case MessageDialogResult.Affirmative:
					if (!ProgressSave(progress))
						break;
					return;
				case MessageDialogResult.Negative:
				default:
					break;
			}
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
					var translate = new Translation()
					{
						Key = regMatchKey.Match(value).Value,
						Level = Convert.ToInt32(regMatchLevel.Match(value).Value),
						Text = regMatchValue.Match(value).Value,
						State = TranslationState.Empty
					};

					return translate;
				}).ToList();

				//查找已存在的相同key并且文本内容不同的段落
				var updateResult = from import in matchedResult
								   from progress in EditProgress.Translations
								   where import.Key == progress.Key && (isTranslate ? (progress.Translated != import.Text) : (progress.Text != import.Text))
								   select import;

				//更新已存在的翻译段落(有更新的)
				updateResult.ToList().ForEach(t =>
				{
					var updateKey = EditProgress.Translations.Where(k => k.Key == t.Key).FirstOrDefault();

					if (isTranslate)
						updateKey.Translated = t.Text;
					else
						updateKey.Text = t.Text;

					updateKey.State = isTranslate ? TranslationState.TranslateUpdated : TranslationState.OriginUpdated;
				});


				//查询已存在与导入的key差异
				var importKey = matchedResult.Select(t => t.Key).Distinct();
				var progressKey = EditProgress.Translations.Select(t => t.Key).Distinct();

				//添加新增的文本段落
				importKey.Except(progressKey).ToList().ForEach((k) =>
				{
					var importTranslate = matchedResult.Where(t => t.Key == k).FirstOrDefault();

					importTranslate.State = TranslationState.New;
					EditProgress.Translations.Add(importTranslate);
				});

				if (isTranslate)
					return;

				EditProgress.OriginName = fileInfo.Name;
				EditProgress.OriginContent = fileContent;
			}
		}

		private async Task ImportPath(bool isTranslate)
		{
			var dialogController = await this.ShowProgressAsync("正在导入", "正在将文本导入到当前进度中...");
			dialogController.SetIndeterminate();

			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = false;
				_fileDialog.Title = "选择需要导入的文本";


				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					ImportContent(_fileDialog.FileName, isTranslate);
				}
			}

			await dialogController.CloseAsync();
		}
	}
}
