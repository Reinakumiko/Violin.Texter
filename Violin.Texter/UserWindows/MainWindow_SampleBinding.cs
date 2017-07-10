using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Violin.Texter.Classes;
using Violin.Texter.UserWindows;
using Violin.Texter.Core.Translations;
using Violin.Texter.Core.Exceptions;
using System.ComponentModel;

namespace Violin.Texter
{
	/// <summary>
	/// 对于主窗体的简单事件绑定部分
	/// </summary>
	public partial class MainWindow
	{
		private async void ImportOrigin_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
				CreateCurrentProgress();

			await ImportPath(EditProgress, false);
			UpdateEditList();
		}

		private async void ImportTranslate_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
				CreateCurrentProgress();

			await ImportPath(EditProgress, true);
			UpdateEditList();
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

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			CheckValidProgress(EditProgress);
			ProgressSave(EditProgress);
		}

		private void SaveAs_Click(object sender, RoutedEventArgs e)
		{
			CheckValidProgress(EditProgress);
			ProgressSaveAs(EditProgress);
		}

		private async void CloseProgress_Click(object sender, RoutedEventArgs e)
		{
			CheckValidProgress(EditProgress);
			await CloseCurrentProgress();
		}

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

		/// <summary>
		/// 导出原文
		/// </summary>
		private void ExportOrigin_Click(object sender, RoutedEventArgs e)
		{
			CheckValidProgress(EditProgress, "无效的操作", "当前未打开任何进度，无法导出文本。");
			SaveContent(() => EditProgress.OriginContent);
		}


		/// <summary>
		/// 导出译文
		/// </summary>
		private void ExportTranslated_Click(object sender, RoutedEventArgs e)
		{
			CheckValidProgress(EditProgress, "无效的操作", "当前未打开任何进度，无法导出文本。");
			SaveContent(() =>
			{
				var originContent = EditProgress.OriginContent;
				var changedList = EditProgress.Translations.Where(t => t.IsTranslated).ToList();

				changedList.ForEach(t =>
				{
					var regMatch = $@"(?<={t.Key}\:\t?\d\s?)" + "\".*\"";

					originContent = Regex.Replace(originContent, regMatch, t.RenderTranslateText(true));
				});

				return originContent;
			});
		}

		/// <summary>
		/// 复制键项目
		/// </summary>
		private void KeyCopy_Click(object sender, RoutedEventArgs e)
		{
			var translateContent = $"{CurrentItem.Key}:{CurrentItem.Level} \r\n\tOrigin: {CurrentItem.Text} \r\n\tTranslated: {CurrentItem.Translated}";

			Clipboard.SetText(translateContent);
		}

		/// <summary>
		/// 移除键项目
		/// </summary>
		private void RemoveKey_Click(object sender, RoutedEventArgs e)
		{
			EditProgress.Translations.Remove(CurrentItem);
		}

		/// <summary>
		/// 程序关闭时询问是否保存进度
		/// </summary>
		private async void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;

			await CloseCurrentProgress();

			Application.Current.Shutdown();
		}
	}
}