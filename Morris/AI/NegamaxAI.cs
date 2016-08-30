/*
 * NegamaxAI.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris.AI
{
	/// <summary>
	/// Eine einfache Version des Negamax-Algorithmus mit einer primitiven Heuristik
	/// </summary>
	[SelectorName("Einfacher Negamax")]
	class NegamaxAI : IMoveProvider
	{
		// Alle gültigen Züge, die basierend auf move einen Spielstein entfernen
		private IEnumerable<GameMove> validRemoves(GameMove move, IReadOnlyGameState state)
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

			return Enumerable.Range(0, GameState.FIELD_SIZE)
					.Where(point => state.Board[point].IsOccupiedBy(state.NextToMove.Opponent()) && filter(point))
					.Select(move.WithRemove);
		}

		// Alle gültigen Züge
		private IEnumerable<GameMove> allMoves(IReadOnlyGameState state)
		{
			return state.BasicMoves()
				.SelectMany(move => state.IsValidMove(move) == MoveValidity.ClosesMill ? validRemoves(move, state) : new[] { move });
		}

		// Primitive Negamax-Implementation nach https://en.wikipedia.org/wiki/Negamax
		private Tuple<int, GameMove> negamax(GameState state, int depth, int color)
		{
			if (state.Result != GameResult.Running)
			{
				switch (state.Result)
				{
					case GameResult.WhiteVictory:
						return Tuple.Create(10 * color, (GameMove)null);

					case GameResult.BlackVictory:
						return Tuple.Create(-10 * color, (GameMove)null);

					case GameResult.Draw:
						return Tuple.Create(0, (GameMove)null);
				}
			}
			// Die Heuristik für den Base Case ist auch hier wieder sehr primitiv und lautet: Differenz in der Zahl der Spielsteine
			if (depth == 0)
				return Tuple.Create((state.GetCurrentStones(Player.White) - state.GetCurrentStones(Player.Black)) * color, (GameMove)null);

			// Ab hier ist alles Standard, siehe Wikipedia
			int bestValue;
			GameMove goodMove = allMoves(state).AllMaxBy(next =>
			{
				// Was-wäre-wenn Analyse findet anhand von Arbeitskopien des Zustands statt
				var newState = new GameState(state);
				if (newState.TryApplyMove(next) != MoveResult.OK)
					return int.MinValue;

				return -negamax(newState, depth - 1, -color).Item1;
			}, out bestValue).ToList().ChooseRandom();

			return Tuple.Create(bestValue, goodMove);
		}

		public GameMove GetNextMove(IReadOnlyGameState state)
		{
			return negamax(new GameState(state), 4, state.NextToMove == Player.White ? 1 : -1).Item2;
		}
	}
}
