/*
 * Player.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Repräsentiert einen Spieler
	/// </summary>
	public enum Player : byte
	{
		White = 170, // (10101010) base 2
		Black = 85 // (01010101) base 2
	}
}
