using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Violin.Texter.Console
{
	class Program
	{
		static void Main(string[] args)
		{
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
			var str = "country_capitulated.0.d:0 \"[Root.GetName] has lost the war and has surrendered.There seems to be no way their military is able to continue the fight.\"";
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值

			//var ymlObject = new Deserializer().Deserialize<Dictionary<string, string>>(str);

			//System.Console.WriteLine(ymlObject);
		}
	}
}
