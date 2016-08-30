/*
 * CoordinateTranslator.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris
{
	/// <summary>
	/// Statische Klasse, die Methoden bereitstellt um Spielfeldpositionen zwischen verschiedenen Formaten zu überführen
	/// </summary>
	public static class CoordinateTranslator
	{
		private static Dictionary<string, int> humans = new Dictionary<string, int>()
		{
			["a1"] = 21,
			["a4"] = 9,
			["a7"] = 0,
			["b2"] = 18,
			["b4"] = 10,
			["b6"] = 3,
			["c3"] = 15,
			["c4"] = 11,
			["c5"] = 6,
			["d1"] = 22,
			["d2"] = 19,
			["d3"] = 16,
			["d5"] = 7,
			["d6"] = 4,
			["d7"] = 1,
			["e3"] = 17,
			["e4"] = 12,
			["e5"] = 8,
			["f2"] = 20,
			["f4"] = 13,
			["f6"] = 5,
			["g1"] = 23,
			["g4"] = 14,
			["g7"] = 2
		};

		private static Dictionary<int, string> ids = humans.ToDictionary(pair => pair.Value, pair => pair.Key);

		// Die XML-Kommentare sind hier ausgelassen, weil die Methodennamen ausreichend sprechend sein sollten

		public static int[] CoordinatesFromHumanReadable(string human)
		{
			if (!humans.Keys.Contains(human))
				throw new ArgumentException("Dies ist keine gültige Positionsangabe");

			return new[] { 6 - (human[1] - '1'), human[0] - 'a' };
		}

		public static string HumanReadableFromCoordinates(int[] coord)
		{
			string res = new string(new[] { 'a' + coord[1], '1' + coord[0] }.Select(x => (char)x).ToArray());

			if (humans.Keys.Contains(res))
				return res;

			throw new ArgumentException("Dies sind keine gültigen Koordinaten");
		}

		public static int[] CoordinatesFromID(int id)
		{
			return CoordinatesFromHumanReadable(HumanReadableFromID(id));
		}

		public static int IDFromCoordinates(int[] coord)
		{
			return IDFromHumanReadable(HumanReadableFromCoordinates(coord));
		}

		public static int IDFromHumanReadable(string human)
		{
			int result;
			if (humans.TryGetValue(human, out result))
				return result;

			throw new ArgumentException("Dies ist keine gültige Positionsangabe");
		}

		public static string HumanReadableFromID(int ID)
		{
			string result;
			if (ids.TryGetValue(ID, out result))
				return result;

			throw new ArgumentException("Dies ist eine gültige ID");
		}
	}
}
