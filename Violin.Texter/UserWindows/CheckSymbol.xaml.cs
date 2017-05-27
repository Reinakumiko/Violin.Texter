using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Violin.Texter.Classes;

namespace Violin.Texter.UserWindows
{
	/// <summary>
	/// CheckSymbol.xaml 的交互逻辑
	/// </summary>
	public partial class CheckSymbol : MetroWindow
	{
		public CheckSymbol()
		{
			InitializeComponent();
		}

		public void OnFocus(TextBox box)
		{
			ContentBind(box.Text);
			//box.TextChanged += OnContentChanged;
		}

		public void OnLost(TextBox box)
		{
			ContentClear();
			//box.TextChanged -= OnContentChanged;
		}

		public List<SymbolCount> SymbolCounts { get; set; }
		private Regex regSymbol = new Regex(@"[^\d\w\s]");
		private void ContentBind(string text)
		{
			var symbols = regSymbol.Matches(text).Cast<Match>().Select(m => m.Value);

			var symbolCounts = symbols.GroupBy(g => g).Select(g => new SymbolCount()
			{
				Symbol = g.Key.FirstOrDefault(),
				Count = g.Count()
			});

			SymbolCounts = symbolCounts.ToList();
			
			_symbolBox.ItemsSource = SymbolCounts;
		}

		private void ContentClear()
		{
			SymbolCounts?.Clear();
		}

		private void OnContentChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = sender as TextBox;

			ContentBind(textBox.Text);
		}

		private void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();

			Owner = null;
		}
	}
}
