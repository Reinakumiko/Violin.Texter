using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Violin.Texter.Classes
{
	public class TranslationItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public string Key { get; set; }
		public string OriginTranslate { get; set; }

		private Translation _value;
		public Translation Value
		{
			get { return _value; }
			set
			{
				_value = value;

				OriginTranslate = value.Translated;
				State = HasTranslate ? TranslateState.Changed : TranslateState.Empty;
			}
		}

		[JsonIgnore]
		private TranslateState _state;

		[JsonIgnore]
		public TranslateState State
		{
			get { return _state; }
			set
			{
				_state = value;
				SetBrush();

				//通知事件更改
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Brush)));
			}
		}

		[JsonIgnore]
		public bool IsChanged
		{
			get
			{
				return Value.Translated != OriginTranslate;
			}

			set
			{
				if (value)
					OriginTranslate = Value.Translated;
			}
		}

		[JsonIgnore]
		public bool HasTranslate
		{
			get
			{
				return Value?.IsTranslate ?? false;
			}
		}

		[JsonIgnore]
		public Brush Brush { get; set; }

		public TranslationItem()
		{
			SetBrush();
		}

		private void SetBrush()
		{
			Brush = new TranslateStateBrush() { State = _state }.Brush;
		}

		public string GetTranslateRendered()
		{
			return $"\"{Value.Translated.Replace("\"", "\\\"")}\"";
		}

		public override string ToString()
		{
			return $"{Key}:{Value.Level} \"{Value.Text.Replace("\"", "\\\"")}\"";
		}
	}
}
