using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Core.Exceptions
{
	public class MessageDialogException : Exception
	{
		public string Title { get; set; }
		public string Content { get; set; }
		public override string Message
		{
			get
			{
				return $"{Title}: {Content}";
			}
		}

		public MessageDialogException(string title, string content)
		{
			Title = title;
			Content = content;
		}
	}
}
