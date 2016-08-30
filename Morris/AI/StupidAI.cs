/*
 * StupidAI.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Linq;

namespace Morris
{
	/// <summary>
	/// Eine sehr primitive KI, die lediglich die direkten nächsten Züge nach simplen Kriterien untersucht
	/// </summary>
	[SelectorName("Dumme KI")]
	internal class StupidAI : IMoveProvider
	{
		private int scoreNonRemoving(GameMove move, IReadOnlyGameState state)
		{
			// Diese Veriablen enthalten genau das, was ihre Namen suggerieren
			bool closesMill = GameState.Mills.Any(mill => mill.All(point => (!move.From.HasValue || point != move.From.Value) && state.Board[point].IsOccupiedBy(state.NextToMove) || point == move.To));
			bool preventsMill = GameState.Mills.Any(mill => mill.All(point => state.Board[point].IsOccupiedBy(state.NextToMove.Opponent()) || point == move.To));
			bool opensMill = move.From.HasValue && GameState.Mills.Any(mill => mill.Contains(move.From.Value) && mill.All(point => state.Board[point].IsOccupiedBy(state.NextToMove)));

			// Als "Tiebraker" dient das Kriterium, wie viele andere eigene Spielsteine sich in den potenziellen Mühlen des Zielfeldes befinden.
			// Dieses Kriterium ist extrem schwach, weil es sehr leicht in lokalen Maxima stecken bleibt
			// In anderen Worten ist es sehr schwierig für diese KI, nach der Setzphase noch neue Mühlen zu bilden
			int inRange = GameState.Mills.Sum(mill => mill.Contains(move.To) ? mill.Count(point => state.Board[point].IsOccupiedBy(state.NextToMove)) : 0);

			return (closesMill ? 10 : 0) + (preventsMill ? 12 : 0) + (opensMill ? 9 : 0) + inRange;
		}

		private int scoreRemove(int remove, IReadOnlyGameState state)
		{
			return GameState.Mills.Sum(mill => mill.Contains(remove) ? mill.Count(point => state.Board[point].IsOccupiedBy(state.NextToMove.Opponent())) : 0);
		}

		public GameMove GetNextMove(IReadOnlyGameState state)
		{
			int ignored;
			// Simples Prinzip: Alle Züge werden nach den Bepunktungsfunktionen bewertet, von den besten Zügen wird ein zufälliger Zug ausgewählt
			GameMove move = state.BasicMoves().AllMaxBy(x => scoreNonRemoving(x, state), out ignored).ToList().ChooseRandom();

			if (state.IsValidMove(move) == MoveValidity.ClosesMill)
			{
				bool allInMill = Enumerable.Range(0, GameState.FIELD_SIZE)
					.Where(point => state.Board[point].IsOccupiedBy(state.NextToMove.Opponent()))
					.All(point => GameState.Mills.Any(mill => mill.Contains(point) && mill.All(mp => state.Board[mp].IsOccupiedBy(state.NextToMove.Opponent()))));

				Func<int, bool> filter;
				if (allInMill)
					filter = _ => true;
				else
					// Wenn es Steine gibt, die in keiner Mühle sind, müssen wir einen solchen Stein entfernen
					filter = point => GameState.Mills.All(mill => !mill.Contains(point) || mill.Any(mp => !state.Board[mp].IsOccupiedBy(state.NextToMove.Opponent())));

				return move.WithRemove(
					Enumerable.Range(0, GameState.FIELD_SIZE)
					.Where(point => state.Board[point].IsOccupiedBy(state.NextToMove.Opponent()) && filter(point))
					.AllMaxBy(x => scoreRemove(x, state), out ignored)
					.ToList().ChooseRandom()
					);
			}

			return move;
		}
	}
}
