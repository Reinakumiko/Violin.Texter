using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Violin.Texter.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var str = "country_capitulated.0.d:0 \"[Root.GetName] has lost the war and has surrendered.There seems to be no way their military is able to continue the fight.\"";

			var ymlObject = new Deserializer().Deserialize<Dictionary<string, string>>(str);

			System.Console.WriteLine(ymlObject);
		}
	}
}
