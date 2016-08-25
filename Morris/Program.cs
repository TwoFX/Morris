using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	class Program
	{
		static void Main(string[] args)
		{
			var a = new ConsoleInteraction();
			var g = new Game(a, a);
			g.AddObserver(a);
			g.Run();
		}
	}
}
