using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var a = new ConsoleInteraction();
			var b = new RandomBot();
			var w = new GameWindow();
			var g = new Game(b, b);
			g.AddObserver(a);
			g.AddObserver(w);
			Task.Run(() => g.Run());
			new Application().Run(w);
		}
	}
}
