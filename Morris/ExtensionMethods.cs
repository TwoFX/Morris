using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	internal static class ExtensionMethods
	{
		public static Player Opponent(this Player p)
		{
			return ~p;
		}
	}
}
