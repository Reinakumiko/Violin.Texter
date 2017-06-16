using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using Violin.Texter.Core.EventArguments;
using Violin.Texter.Core.RenderBrushes;

namespace Violin.Texter.Core.Translations
{
	/// <summary>
	/// 表示一个翻译文本的键值对
	/// </summary>
	public class Translation : INotifyPropertyChanged
	{
		/// <summary>
		/// 当翻译文本被修改时触发的委托
		/// </summary>
		/// <param name="translation">被修改的翻译文本</param>
		public delegate void OnTranslationChangedEventHandle(Translation translation);

		/// <summary>
		/// 当翻译文本修改状态时调用的委托
		/// </summary>
		/// <param name="translation">发生修改的翻译文本</param>
		/// <param name="args">被修改的新旧值</param>
		public delegate void OnTranslationStateChangedEventHandle(Translation translation, ValueChangedEventArgs<TranslationState> args);

		/// <summary>
		/// 当状态被修改时触发的事件
		/// </summary>
		public event OnTranslationStateChangedEventHandle OnTranslationStateChanged;

		/// <summary>
		/// 当翻译文本的译文被修改时触发的事件
		/// </summary>
		public event OnTranslationChangedEventHandle OnTranslationChanged;

		/// <summary>
		/// 当属性发生更改的时候触发的属性通知事件
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// 翻译文本原文的内部储存字段
		/// </summary>
		private string _text;

		/// <summary>
		/// 翻译文本译文的内部存储字段
		/// </summary>
		private string _translation;

		/// <summary>
		/// 翻译文本当前状态的内部字段
		/// </summary>
		private TranslationState _state;

		/// <summary>
		/// 翻译译文的优先等级
		/// </summary>
		public int Level { get; set; }

		/// <summary>
		/// 翻译文本的键
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// 翻译文本的翻译译文
		/// </summary>
		public string Translated
		{
			get { return _translation; }
			set
			{
				_translation = value;

				OnTranslationChanged?.Invoke(this);

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Brush)));
			}
		}

		/// <summary>
		/// 翻译文本的原文
		/// </summary>
		public string Text
		{
			get { return _text; }
			set { _text = value.Trim('"').Replace("\\\"", "\""); }
		}

		/// <summary>
		/// 翻译文本的状态
		/// </summary>
		[JsonIgnore]
		public TranslationState State
		{
			get { return _state; }
			set
			{
				OnTranslationStateChanged?.Invoke(this, new ValueChangedEventArgs<TranslationState>()
				{
					OldValue = _state,
					NewValue = value
				});

				_state = value;
			}
		}

		/// <summary>
		/// 表示翻译文本目前的刷子
		/// </summary>
		[JsonIgnore]
		public Brush Brush
		{
			get { return TranslateStateBrush.Brushes[State]; }
		}

		/// <summary>
		/// 表示当前翻译文本是否已经被翻译
		/// </summary>
		[JsonIgnore]
		public bool IsTranslated
		{
			get { return !string.IsNullOrWhiteSpace(Translated); }
		}

		/// <summary>
		/// 获取翻译的渲染文本
		/// </summary>
		/// <returns>翻译文本的字符串结果</returns>
		public string RenderTranslate()
		{
			return $"\"{Translated?.Replace("\"", "\\\"")}\"";
		}

		/// <summary>
		/// 将当前对象以字符串的表现形式呈现
		/// </summary>
		/// <returns>当前对象的字符串结果</returns>
		public override string ToString()
		{
			return ToString(false);
		}

		/// <summary>
		/// 将当前对象以字符串的表现形式呈现
		/// </summary>
		/// <param name="translated">以翻译文本来呈现结果</param>
		/// <returns>当前对象的字符串结果</returns>
		public string ToString(bool translated)
		{
			return $"{Key}:{Level} \"{(translated ? Translated : Text).Replace("\"", "\\\"")}\"";
		}
	}
}
