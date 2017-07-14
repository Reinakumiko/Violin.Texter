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
using Violin.Texter.Core.Enumeration;
using Violin.Texter.Core.Exceptions;
using Violin.Texter.Core.Notify;
using Violin.Texter.Core.Progresses;
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
		/// 显示因为操作失误引起的异常
		/// </summary>
		/// <param name="ex">操作引发的失误异常</param>
		private async void DisplayMessages(MessageDialogException ex)
		{
			await this.ShowMessageAsync(ex.Title, ex.Content);
		}

		/// <summary>
		/// 重置编辑区
		/// </summary>
		private void EditorReset()
		{
			_transOrigin.Clear();
			_transTranslated.Clear();
		}

		private void CreateCurrentProgress()
		{
			EditProgress = new EditProgress()
			{
				State = EditProgressState.NotSave,
				Translations = new NotifyList<Translation>()
			};

			SetListItems(EditProgress);
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
				var fileName = string.Empty;
				var originName = progress?.OriginName;

				if (originName != null)
				{
					var file = new FileInfo(progress.OriginName);
					fileName = file.Name.Replace(file.Extension, "");
				}
				else
					fileName = "edit";

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

		private async void SaveContent(Func<string> action)
		{
			var dialogController = await this.ShowProgressAsync("导出文本", "正在将本文内容导出...");
			dialogController.SetIndeterminate();

			using (var dialog = new CommonSaveFileDialog())
			{
				dialog.DefaultExtension = ".yml";
				dialog.Filters.Add(new CommonFileDialogFilter("Yaml 源文件", "*.yml"));

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					var file = new FileInfo(dialog.FileName);
					file.Check();

					using (var fileStream = file.Open(FileMode.Truncate, FileAccess.Write))
					using (var writer = new StreamWriter(fileStream, new UTF8Encoding(true)))
					{
						writer.WriteLine(await Task.Run(action));
					}
				}
			}

			await dialogController.CloseAsync();
		}

		private async Task CloseCurrentProgress()
		{
			if (EditProgress != null)
				await CloseEditProgress(EditProgress);

			this.Invoke(() =>
			{
				EditProgress = null;
			});
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

		private void SetListItems(EditProgress progress)
		{
			var fillString = string.Empty;

			if (progress != null)
			{
				var translations = progress.Translations;

				if (translations != null && translations.Count > 0)
				{
					keyList.ItemsSource = translations;
					return;
				}
				else
					fillString = "当前列表内容为空。";
			}
			else
				fillString = "当前未打开任何进度文件。";

			keyList.ItemsSource = new string[] { fillString };
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
		private List<Translation> ImportContent(string fileContent)
		{
				//匹配规则
				var regMatchKey = new Regex(@"[\d\w\._]+", RegexOptions.Compiled);
				var regMatchValue = new Regex("\".*\"", RegexOptions.Compiled);
				var regMatchLevel = new Regex(@"(?<=:)\d", RegexOptions.Compiled);

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

				return matchedResult;
		}

		/// <summary>
		/// 导入译文到现有列表中
		/// </summary>
		private void ImportTranslation(IList<Translation> originText, List<Translation> translatedText)
		{
			translatedText.ForEach(t =>
			{
				var origin = originText.Where(o => o.Key == t.Key).FirstOrDefault();

				if (origin == null || origin.Translated == t.Text)
					return;

				//更新字段的译文
				origin.Translated = t.Text;

				//该字段是被更新译文的
				origin.State = TranslationState.TranslateUpdated;
			});
		}

		/// <summary>
		/// 更新原文文本到现有列表中
		/// </summary>
		private void UpdateTranslation(IList<Translation> originText, List<Translation> newText)
		{
			newText.ForEach(t =>
			{
				var origin = originText.Where(o => o.Key == t.Key).FirstOrDefault();

				if (origin == null)
				{
					//该字段是被新加入的
					t.State = TranslationState.New;
					originText.Add(t);
				}
				else if (origin.Text != t.Text)
				{
					//更新原文文本
					origin.Text = t.Text;

					//该字段是被更新原文的
					origin.State = TranslationState.OriginUpdated;
				}
			});
		}

		private async Task ImportPath(EditProgress progress, bool isTranslate)
		{
			var dialogController = await this.ShowProgressAsync("正在导入", "正在将文本导入到当前进度中...");
			dialogController.SetIndeterminate();

			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = false;
				_fileDialog.Title = "选择需要导入的文本";


				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					var fileInfo = new FileInfo(_fileDialog.FileName);

					//打开文件流
					using (var reader = fileInfo.OpenText())
					{
						//获取文本内容
						var fileContent = reader.ReadToEnd();

						//获取导入的文本中有哪些段落
						var importContents = ImportContent(fileContent);

						if (isTranslate)
							ImportTranslation(progress.Translations, importContents); //检查导入译文的更新
						else //如果是译文则不检查原文的更新
						{
							//检查导入文本的更新
							UpdateTranslation(progress.Translations, importContents);
						}

							//设置进度内导入的文件名
							progress.OriginName = fileInfo.Name;

							//设置进度内导入的文件内容
							progress.OriginContent = fileContent;
						}
					}
				}

			}

			await dialogController.CloseAsync();
		}

		private void CheckValidProgress(EditProgress progress)
		{
			CheckValidProgress(EditProgress, "无效的操作", "当前没有已打开的任务进度。");
		}

		private void CheckValidProgress(EditProgress progress, string title, string content)
		{
			if (progress != null)
				return;

			throw new MessageDialogException(title, content);
		}
	}
}
