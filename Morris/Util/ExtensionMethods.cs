using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	public static class ExtensionMethods
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

		/// <summary>
		/// Gibt an, ob ein Feld von einem bestimmten Spieler besetzt ist
		/// </summary>
		public static bool IsOccupiedBy(this Occupation o, Player p)
		{
			return o == (Occupation)p;
		}

		private static Random rng = new Random();

		/// <summary>
		/// Gibt ein zufälliges Element der IList zurück
		/// </summary>
		public static T ChooseRandom<T>(this IList<T> it)
		{
			if (it == null)
				throw new ArgumentNullException(nameof(it));

			return it[rng.Next(it.Count)];
		}

		/// <summary>
		/// Gibt alle Element in input zurück, für die der Wert, der selector zurückgibt, laut comparer maximal ist
		/// </summary>
		public static IEnumerable<T> AllMaxBy<T, TCompare>(this IEnumerable<T> input, Func<T, TCompare> selector, out TCompare finalMax, IComparer<TCompare> comparer = null)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));

			if (selector == null)
				throw new ArgumentNullException(nameof(input));

			comparer = comparer ?? Comparer<TCompare>.Default;

			List<T> collector = new List<T>(); // Enthält alle Elemente, die den höchsten gesehenen Wert von selector(element) aufweisen
			bool hasMax = false; // Ob wir bereits überhaupt ein Element gesehen haben und daher den Wert von max verwenden können
			TCompare max = default(TCompare); // Der höchste gesehene Wert von selector(element)

			foreach (T element in input)
			{
				TCompare current = selector(element);
				int comparisonResult = comparer.Compare(current, max);
				if (!hasMax || comparisonResult > 0)
				{
					// Es gibt einen neuen maximalen Wert von selector(element)
					hasMax = true;
					max = current;
					collector.Clear();
					collector.Add(element);
				}
				else if (comparisonResult == 0)
					collector.Add(element);
			}
			finalMax = max;
			return collector;
		}
	}
}
