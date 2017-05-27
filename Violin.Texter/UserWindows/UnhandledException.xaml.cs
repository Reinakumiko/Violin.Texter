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

namespace Violin.Texter.UserWindows
{
	/// <summary>
	/// UnhandledException.xaml 的交互逻辑
	/// </summary>
	public partial class UnhandledException : MetroWindow
	{
		public UnhandledException(Exception ex, bool isTerminating)
		{
			InitializeComponent();

			Title = "发成程序异常";
			_dialogMessage.Text = ex.ToString();

			if(isTerminating)
			{
				Title = "未能处理程序异常";

				_dialogClose.Content = "退出程序";
			}


			MaxHeight = SystemParameters.PrimaryScreenHeight - 100;
			MaxWidth = SystemParameters.PrimaryScreenWidth - 200;
		}

		private void _dialogClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
