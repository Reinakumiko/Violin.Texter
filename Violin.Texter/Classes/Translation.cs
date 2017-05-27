using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Violin.Texter.Classes
{
	public class Translation
	{
		public int Level { get; set; }
		public string Translated { get; set; }

		private string _text;

		public string Text
		{
			get { return _text; }
			set { _text = value.Trim('"').Replace("\\\"", "\""); }
		}
		
		[JsonIgnore]
		public bool IsTranslate { get { return !string.IsNullOrWhiteSpace(Translated); } }
	}
}
