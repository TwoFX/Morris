using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	internal static class ExtensionMethods
	{
		/// <summary>
		/// Gibt den Gegner des Spielers zurück
		/// </summary>
		public static Player Opponent(this Player p)
		{
			// Es ist in der Regel vermutlich einfacher ~player anstatt player.Opponent() zu
			// schreiben, Änderungen am Schema von Player sind so jedoch von dem Code, der
			// Player verwendet, wegabstrahiert und die semantische Bedeutung von Code,
			// der .Opponent verwendet, ist einfacher zu erkennen (Kapselung).
			return ~p;
		}

		private static Random rng = new Random();

		/// <summary>
		/// Gibt ein zufälliges Element der IList zurück
		/// </summary>
		public static T ChooseRandom<T>(this IList<T> it)
		{
			return it[rng.Next(it.Count)];
		}
	}
}
