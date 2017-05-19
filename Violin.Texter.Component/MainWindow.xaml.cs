using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Violin.Texter.Component
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			ShowMinButton = false;
			ShowMaxRestoreButton = false;
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var control = sender as TextBox;
			control.Text = Regex.Match(control.Text, @"\d+").Value;
		}

		private void _selectFile_Click(object sender, RoutedEventArgs e)
		{
			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = false;
				_fileDialog.Title = "选择需要拆分的文件";

				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					_targetPath.Text = _fileDialog.FileName;
				}
			}
		}

		private void _selectDir_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new CommonOpenFileDialog())
			{
				dialog.IsFolderPicker = true;
				dialog.Title = "选择保存的目录";

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					_savePath.Text = dialog.FileName;
				}
			}
		}

		private async void _splitAction_Click(object sender, RoutedEventArgs e)
		{
			var mySettings = new MetroDialogSettings()
			{
				AnimateShow = false,
				AnimateHide = false
			};

			var controller = await this.ShowProgressAsync("处理中", "正在拆分文件", settings: mySettings);
			controller.SetIndeterminate();

			try
			{
				var filePath = _targetPath.Text;
				var fileInfo = new FileInfo(filePath);
				var dirInfo = new DirectoryInfo(_savePath.Text);

				if (string.IsNullOrWhiteSpace(filePath) || !fileInfo.Exists)
					throw new ArgumentException("选择的文件目录不正确。");

				if (!dirInfo.Exists)
					dirInfo.Create();

				if (_withLine.IsChecked ?? false)
					SplitWithLine(fileInfo, dirInfo, Convert.ToInt32(_lineNumber.Text));
				else if (_withCount.IsChecked ?? false)
					SplitWithCount(fileInfo, dirInfo, Convert.ToInt32(_fileNumber.Text));
				else
					throw new ArgumentOutOfRangeException("可选的拆分方式超出预料之外。");

				await Task.Delay(2000);
			}
			catch (Exception ex)
			{
				await this.ShowMessageAsync("发生异常", ex.ToString(), settings: new MetroDialogSettings()
				{
					MaximumBodyHeight = 100,
					DialogMessageFontSize = 12,
					AffirmativeButtonText = "确定",
					ColorScheme = MetroDialogOptions.ColorScheme
				});
			}
			finally
			{
				await controller.CloseAsync();
			}
		}

		private void SplitWithLine(FileInfo fileInfo, DirectoryInfo dirInfo, int lineCount)
		{
			var lineText = GetTextContent(fileInfo);

			var fileCount = Math.Ceiling((double)lineText.Count / lineCount);

			for (int i = 0; i < fileCount; i++)
			{
				var savePath = $"{dirInfo}/{fileInfo.Name.Replace(fileInfo.Extension, "")}_part{i}{fileInfo.Extension}";
				var fileTexter = lineText.Where(l => LineInCurrentPageZone(l.Key, i, lineCount)).OrderBy(l => l.Key);

				using (var file = File.Open(savePath, FileMode.OpenOrCreate, FileAccess.Write))
				using (var writer = new StreamWriter(file))
				{
					foreach (var line in fileTexter)
					{
						writer.WriteLine(line.Value);
					}
				}
			}
		}

		private void SplitWithCount(FileInfo fileInfo, DirectoryInfo dirInfo, int count)
		{
			var lineText = GetTextContent(fileInfo);

			var lineCount = lineText.Count / count;

			for (int i = 0; i < count; i++)
			{
				var savePath = $"{dirInfo}/{fileInfo.Name.Replace(fileInfo.Extension, "")}_part{i}{fileInfo.Extension}";
				var fileTexter = lineText.Where(l => LineInCurrentPageZone(l.Key, i, lineCount)).OrderBy(l => l.Key);

				using (var file = File.Open(savePath, FileMode.OpenOrCreate, FileAccess.Write))
				using (var writer = new StreamWriter(file))
				{
					foreach (var line in fileTexter)
					{
						writer.WriteLine(line.Value);
					}
				}
			}
		}

		private bool LineInCurrentPageZone(int currentLine, int currentPage, int pageElement)
		{
			var startPoint = currentPage * pageElement;
			return currentLine >= startPoint && currentLine < startPoint + pageElement;
		}

		private Dictionary<int, string> GetTextContent(FileInfo fileInfo)
		{
			var lineText = new Dictionary<int, string>();

			using (var reader = File.OpenText(fileInfo.FullName))
			{
				while (!reader.EndOfStream)
				{
					lineText.Add(lineText.Count + 1, reader.ReadLine());
				}
			}

			return lineText;
		}

		private void _componentSelect_Click(object sender, RoutedEventArgs e)
		{
			using (var _fileDialog = new CommonOpenFileDialog())
			{
				_fileDialog.Multiselect = true;
				_fileDialog.Title = "选择需要组合的文件(多选)";

				if (_fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					foreach (var file in _fileDialog.FileNames)
					{
						//不添加重复文件
						if (_componentTarget.Items.Cast<FileListItem>().Where(i => i.FullName == file).Count() > 0)
							continue;

						var fileInfo = new FileInfo(file);
						_componentTarget.Items.Add(new FileListItem()
						{
							FullName = fileInfo.FullName,
							Name = fileInfo.Name
						});
					}
				}
			}
		}

		private void _componentRemove_Click(object sender, RoutedEventArgs e)
		{
			var selectedItem = _componentTarget.Items.Cast<FileListItem>().Where(item => _componentTarget.SelectedItems.Cast<FileListItem>().Contains(item)).ToList();

			foreach (var item in selectedItem)
			{
				_componentTarget.Items.Remove(item);
			}
		}

		private void _componentDir_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new CommonOpenFileDialog())
			{
				dialog.IsFolderPicker = true;
				dialog.Title = "选择保存的目录";

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					_componentSave.Text = dialog.FileName;
				}
			}
		}

		private async void _componentAction_Click(object sender, RoutedEventArgs e)
		{
			var mySettings = new MetroDialogSettings()
			{
				AnimateShow = false,
				AnimateHide = false
			};

			var controller = await this.ShowProgressAsync("处理中", "正在合并文件", settings: mySettings);
			controller.SetIndeterminate();

			try
			{
				var dirInfo = new DirectoryInfo(_componentSave.Text);

				if (!dirInfo.Exists)
					dirInfo.Create();

				var fileList = _componentTarget.Items.Cast<FileListItem>();
				var fileSaveName = fileList.FirstOrDefault()?.Name ?? "Untitle.txt";
				fileSaveName = Regex.Replace(fileSaveName, @"_part\d+", "");

				var savePath = $"{dirInfo.FullName}/{fileSaveName}";
				using (var file = File.Open(savePath, FileMode.OpenOrCreate, FileAccess.Write))
				using (var writer = new StreamWriter(file))
				{

					//读取并组合
					foreach (var fileItem in fileList)
					{
						using (var fileReader = File.OpenText(fileItem.FullName))
						{
							writer.Write(fileReader.ReadToEnd());
						}
					}
				}

				await Task.Delay(2000);
			}
			catch (Exception ex)
			{
				await this.ShowMessageAsync("发生异常", ex.ToString(), settings: new MetroDialogSettings()
				{
					MaximumBodyHeight = 100,
					DialogMessageFontSize = 12,
					AffirmativeButtonText = "确定",
					ColorScheme = MetroDialogOptions.ColorScheme
				});
			}
			finally
			{
				await controller.CloseAsync();
			}
		}
	}
}
