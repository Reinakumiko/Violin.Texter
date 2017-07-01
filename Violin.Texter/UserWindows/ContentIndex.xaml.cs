using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Violin.Texter.Core.ContentIndex;
using Violin.Texter.Core.Enumeration;
using Violin.Texter.Core.Progresses;
using Violin.Texter.Core.Translations;

namespace Violin.Texter.UserWindows
{
	/// <summary>
	/// ContentIndex.xaml 的交互逻辑
	/// </summary>
	public partial class ContentIndex : MetroWindow
	{
		private static ContentIndex _instance;

		/// <summary>
		/// 获得窗体的实例
		/// </summary>
		public static ContentIndex Instance
		{
			get
			{
				return _instance ?? (_instance = new ContentIndex());
			}
		}

		/// <summary>
		/// 表示搜索时的查询范围
		/// </summary>
		public IndexZones IndexZone
		{
			get
			{
				return GetCurrentIndexZone();
			}
		}

		/// <summary>
		/// 表示搜索时的查询方向
		/// </summary>
		public IndexDirection IndexDirection
		{
			get
			{
				return GetCurrentDirection();
			}
		}

		/// <summary>
		/// 搜索时是否区分大小写
		/// </summary>
		public bool IsSensitiveCase
		{
			get
			{
				return _sensitiveCase.IsChecked ?? false;
			}
		}

		/// <summary>
		/// 搜索时是否使用正则表达式
		/// </summary>
		public bool IsUseRegex
		{
			get
			{
				return _useRegex.IsChecked ?? false;
			}
		}

		/// <summary>
		/// 表示要搜索的内容
		/// </summary>
		public string SearchContent
		{
			get
			{
				return _searchContent.Text;
			}
		}

		/// <summary>
		/// 表示上一次搜索时的内容
		/// </summary>
		public string LastSearchContent { get; set; }

		/// <summary>
		/// 表示上一次搜索到结果时的索引
		/// </summary>
		public int LastIndex { get; set; }

		public EditProgress EditProgress { get; set; }
		public TranslationEnumeration<Translation> _enumeration { get; set; }

		private ContentIndex()
		{
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			InitializeComponent();

			SetDisplayMessage(string.Empty);
		}

		public int NextIndex()
		{
			var options = new IndexOptions()
			{
				Direction = IndexDirection,
				Zone = IndexZone,
				IsUseRegex = IsUseRegex,
				IsSensitiveCase = IsSensitiveCase
			};

			var result = _enumeration.GetNext(SearchContent, options);
			return result.Key;
		}

		public void ResetIndex()
		{
			_enumeration.Reset(IndexDirection == IndexDirection.Next);
		}

		public void SetIndex(uint index)
		{
			_enumeration.SetIndex(index);
		}

		public void SetDisplayMessage(string message)
		{
			SetDisplayMessage(message, Brushes.Black);
		}

		public void SetDisplayMessage(string message, Brush color)
		{
			_resultText.Text = message;
			_resultText.Foreground = color;
		}

		private IndexZones GetCurrentIndexZone()
		{
			if (indexZone_All.IsChecked ?? false)
				return IndexZones.All;
			else if (indexZone_KeyOnly.IsChecked ?? false)
				return IndexZones.KeyOnly;
			else if (indexZone_ContentOnly.IsChecked ?? false)
				return IndexZones.ContentOnly;
			else
				throw new IndexOutOfRangeException();
		}

		private IndexDirection GetCurrentDirection()
		{
			if (groupName_Prev.IsChecked ?? false)
				return IndexDirection.Previous;
			else if (groupName_Next.IsChecked ?? false)
				return IndexDirection.Next;
			else
				throw new IndexOutOfRangeException();
		}

		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//拦截窗体的关闭命令
			e.Cancel = true;

			//将窗体隐藏
			this.Hide();

			//将拥有者设置为空
			this.Owner = null;
		}

		private void _getNext_Click(object sender, RoutedEventArgs e)
		{
			if (Owner == null)
			{
				SetDisplayMessage("需要查找的内容源为空。", Brushes.Red);
				return;
			}

			if(string.IsNullOrWhiteSpace(SearchContent))
			{
				SetDisplayMessage("输入的查找内容为空。", Brushes.Red);
				return;
			}

			if (_enumeration == null || _enumeration.List != EditProgress.Translations || _enumeration.List.Count != EditProgress.Translations.Count)
				_enumeration = new TranslationEnumeration<Translation>(EditProgress.Translations);
			
			if (LastSearchContent != SearchContent)
			{
				ResetIndex();
				LastIndex = -1;
				LastSearchContent = SearchContent;
			}

			var nextIndex = NextIndex();

			if (nextIndex == -1)
			{
				if (LastIndex == -1)
					SetDisplayMessage("未找到匹配的文本项。");
				else
					SetDisplayMessage("搜索达到了尽头，下次搜索将会从头开始。", Brushes.Red);

				ResetIndex();
				return;
			}

			//重置消息(成功)
			SetDisplayMessage("");

			Owner.Invoke(() =>
			{
				LastIndex = nextIndex;
				var keyList = (Owner as MainWindow).keyList;
				keyList.SelectedIndex = LastIndex;

				//将列表中的位置定位到已查找到项的位置
				keyList.ScrollIntoView(keyList.SelectedItem);
				
				/* 这段感觉起来效果还不如 ScrollIntoView()
				var border = VisualTreeHelper.GetChild(keyList, 0) as Border;

				if (border != null)
				{
					var scrollViewer = border.Child as ScrollViewer;
					if (scrollViewer != null)
					{
						scrollViewer.ScrollToVerticalOffset(nextIndex);
					}
				}
				*/
			});

			//焦点设置回查找窗体
			//感觉没必要啊
			this.Focus();
		}
	}
}
