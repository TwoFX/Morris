/*
 * GameResult.cs
 * Copyright (c) 2015 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Beschreibt, ob ein Spiel beendet ist und wie es ausgegangen ist
	/// </summary>
	public enum GameResult
	{
		Running,
		Draw,
		WhiteVictory = Player.White,
		BlackVictory = Player.Black
	}
}
