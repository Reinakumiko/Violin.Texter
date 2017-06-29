using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Violin.Texter.Core.IndexEnumeration;
using Violin.Texter.Core.Progresses;

namespace Violin.Texter.UserWindows
{
	/// <summary>
	/// ContentIndex.xaml 的交互逻辑
	/// </summary>
	public partial class ContentIndex : MetroWindow
	{
		private static ContentIndex _instance;

		public static ContentIndex Instance
		{
			get
			{
				return _instance ?? (_instance = new ContentIndex());
			}
		}

		public IndexZones IndexZone
		{
			get
			{
				return GetCurrentIndexZone();
			}
		}

		public IndexDirection IndexDirection
		{
			get
			{
				return GetCurrentDirection();
			}
		}

		public int LastIndex { get; set; }
		public EditProgress EditProgress { get; set; }

		private ContentIndex()
		{
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			InitializeComponent();
		}

		public new void Show()
		{
			throw new NotSupportedException("该窗体不支持此方式展示");
		}

		public void Show(EditProgress progress)
		{
			EditProgress = progress;

			base.Show();
		}

		public int NextIndex()
		{
			return 0;
		}

		public void ResetIndex()
		{
			LastIndex = 0;
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
	}
}
