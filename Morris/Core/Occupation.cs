/*
 * Occupation.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Zustand, den eine Stelle auf dem Spielfeld einnehmen kann
	/// </summary>
	public enum Occupation
	{
		Free,
		// Die folgende Gleichsetzung spart einige Verzweigungen,
		// indem ein Wert von Occupation direkt mit einem Wert
		// von Player verglichen werden kann
		White = Player.White,
		Black = Player.Black
	}
}