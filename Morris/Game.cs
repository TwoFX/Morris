/*
 * Game.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System.Collections.Generic;
using System.Threading;

namespace Morris
{
	/// <summary>
	/// Repräsentiert ein einzelnes Mühlespiel
	/// </summary>
	class Game
	{
		private List<IGameStateObserver> observers = new List<IGameStateObserver>();
		private GameState state;
		private Dictionary<Player, IMoveProvider> providers;

		public Game(IMoveProvider white, IMoveProvider black)
		{
			state = new GameState();
			providers = new Dictionary<Player, IMoveProvider>()
			{
				[Player.White] = white,
				[Player.Black] = black
			};
		}

		/// <summary>
		/// Spielt eine gesamte Runde Mühle
		/// </summary>
		/// <param name="moveDelay">Die Zeit, in Millisekunden, die gewartet wird, bevor nach einem
		/// erfolgreichem Zug der nächste Zug angefordert wird (damit KI vs. KI-Spiele in einem
		/// angemessenen Tempo angesehen werden können)</param>
		/// <returns>Das Spielergebnis</returns>
		public GameResult Run(int moveDelay = 0)
		{
			notifyOberservers();
			MoveResult res;

			// Äußere Schleife läuft einmal pro tatsächlichem Zug
			do
			{
				// Innere Schleife läuft einmal pro Zugversuch
				do
				{
					res = state.TryApplyMove(providers[state.NextToMove].GetNextMove(state));
				} while (res == MoveResult.InvalidMove);

				notifyOberservers();
				Thread.Sleep(moveDelay);
			} while (state.Result == GameResult.Running);

			return state.Result;
		}

		/// <summary>
		/// Registriert einen <see cref="IGameStateObserver"/> für kommende Spielereignisse 
		/// </summary>
		/// <param name="observer">Das zu notifizierende Objekt</param>
		public void AddObserver(IGameStateObserver observer)
		{
			observers.Add(observer);
		}

		/// <summary>
		/// Meldet einen <see cref="IGameStateObserver"/> von kommenden Spielereignissen ab
		/// </summary>
		/// <param name="observer">Das abzumeldende Objekt</param>
		/// <returns>Wahr, wenn das Objekt tatsächlich entfernt wurde.
		/// Falsch, wenn es nicht gefunden wurde.</returns>
		public bool RemoveObserver(IGameStateObserver observer)
		{
			return observers.Remove(observer);
		}

		// Meldet dem Spielzustand an alle Observer
		private void notifyOberservers()
		{
			foreach (var observer in observers)
			{
				observer.Notify(state);
			}
		}
	}
}
