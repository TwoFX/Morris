/*
 * GameState.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	/// <summary>
	/// Repräsentiert eine Mühle-Spielsituation
	/// </summary>
	public class GameState
	{
		public Occupation[] Board { get; private set; }
		public Player NextToMove { get; private set; }
		public GameResult Result { get; private set; }

		public GameState()
		{
			// Leeres Feld
			Board = Enumerable.Repeat(Occupation.Free, 24).ToArray();
			NextToMove = Player.White;
			Result = GameResult.Running;
		}

		public bool IsValidMove()


	}
}
