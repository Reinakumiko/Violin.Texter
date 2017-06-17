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

namespace Violin.Texter
{
	/// <summary>
	/// 对于主窗体的简单事件绑定部分
	/// </summary>
	public partial class MainWindow
	{
		private async void ImportOrigin_Click(object sender, RoutedEventArgs e)
		{
			await ImportPath(false);
			UpdateEditList();
		}

		private async void ImportTranslate_Click(object sender, RoutedEventArgs e)
		{
			await ImportPath(true);
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
			if (EditProgress == null)
			{
				this.ShowMessageAsync("无法保存", "当前没有已打开任务进度。");
				return;
			}

			ProgressSave(EditProgress);
		}

		private void SaveAs_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				this.ShowMessageAsync("无法保存", "当前没有已打开任务进度。");
				return;
			}

			ProgressSaveAs(EditProgress);
		}

		private async void CloseProgress_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				await this.ShowMessageAsync("操作失败", "当前未打开任何进度。");
				return;
			}

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

		private async void ExportOrigin_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				await this.ShowMessageAsync("无效的操作", "当前未打开任何进度，无法导出文本。");
				return;
			}

			SaveContent(() => EditProgress.OriginContent);
		}

		private async void ExportTranslated_Click(object sender, RoutedEventArgs e)
		{
			if (EditProgress == null)
			{
				await this.ShowMessageAsync("无效的操作", "当前未打开任何进度，无法导出文本。");
				return;
			}


			SaveContent(() =>
			{
				var originContent = EditProgress.OriginContent;
				var changedList = EditProgress.Translations.Where(t => t.IsTranslated).ToList();

				changedList.ForEach(t =>
				{
					var regMatch = $@"(?<={t.Key}\:\t?\d\s?)" + "\".*\"";

					originContent = Regex.Replace(originContent, regMatch, t.RenderTranslate());
				});

				return originContent;
			});
		}
	}
}