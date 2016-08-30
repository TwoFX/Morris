/*
 * RandomBot.cs
 * Copyright (c) 2016 Markus Himmel
 * This fileis distributed under the terms of the MIT license
 */

using System;
using System.Linq;

namespace Morris
{
	/// <summary>
	/// Ein extrem einfacher KI-Spieler, der einen zufälligen gültigen Spielzug auswählt
	/// </summary>
	[SelectorName("Zufalls-KI")]
	internal class RandomBot : IMoveProvider
	{
		// Anhand dieser Klasse können wir sehen, wie einfach es ist, einen Computerspieler zu implementieren.
		// Es muss lediglich eine einzige, einfache Methode implementiert werden. Der Spielzustandparameter stellt
		// Methoden wie BasicMoves und IsValidMove bereit, mit denen viele verschiedene Strategien sehr einfach
		// implementiert werden können.

		private Random rng = new Random();

		public GameMove GetNextMove(IReadOnlyGameState state)
		{
			// Ein zufälliger Spielzug
			GameMove chosen = state.BasicMoves().ToList().ChooseRandom();

			// Wenn wir einen Stein entfernen dürfen, wählen wir einen zufälligen Punkt aus, auf dem sich ein gegnerischer Stein befindet
			// Anmerkung: Hier kann ein ungültiger Zug bestimmt werden, weil man z.T. nicht alle gegnerischen Steine nehmen darf, aber 
			// dann wird GetNextMove einfach noch einmal aufgerufen.
			if (state.IsValidMove(chosen) == MoveValidity.ClosesMill)
				return chosen.WithRemove(Enumerable
					.Range(0, GameState.FIELD_SIZE)
					.Where(d => (int)state.Board[d] == (int)state.NextToMove.Opponent())
					.ToList().ChooseRandom());

			return chosen;
		}
	}
}
