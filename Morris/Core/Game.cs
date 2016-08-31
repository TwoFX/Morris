/*
 * Game.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;

namespace Morris
{
	/// <summary>
	/// Repräsentiert ein einzelnes Mühlespiel
	/// </summary>
	internal class Game
	{
		// Alle Anzeigen
		private List<IGameStateObserver> observers = new List<IGameStateObserver>();

		// Der Spielzustand
		private GameState state;

		// Die Spieler/KIs
		private Dictionary<Player, IMoveProvider> providers;

		// Alle bisherigen Züge
		private ObservableCollection<GameMove> moves = new ObservableCollection<GameMove>();
		// Alle bisherigen Züge (Lesezugriff)
		private ReadOnlyObservableCollection<GameMove> movesReadOnly;

		// Der Thread, auf dem die Spiellogik und die Provider laufen
		private Thread gameThread;

		public ReadOnlyObservableCollection<GameMove> Moves
		{
			get
			{
				return movesReadOnly;
			}
		}
	
		public IMoveProvider White
		{
			get
			{
				return providers[Player.White];
			}
			set
			{
				providers[Player.White] = value;
				if (state.NextToMove == Player.White)
					KickOver();
			}
		}

		public IMoveProvider Black
		{
			get
			{
				return providers[Player.Black];
			}
			set
			{
				providers[Player.Black] = value;
				if (state.NextToMove == Player.Black)
					KickOver();
			}
		}

		// Verzögerung, damit man einen Zug betrachten kann
		public int Delay
		{
			get;
			set;
		}

		// Wird gesperrt, wenn moves bzw. movesReadOnly verändert wird
		private static object movesLock = new object();

		public Game(IMoveProvider white, IMoveProvider black, int delay)
		{
			state = new GameState();

			// moves bzw. movesReadOnly ist eine ObservableCollection. Das bedeutet, dass jemand,
			// der die Referenz zu diesem Objekt hat, sich notifizieren lassen kann, wenn sich diese
			// geändert hat. Das ist sehr praktisch, da wir die Referenz zu movesReadOnly an die UI
			// geben können. Dann setzen wir einfach die ItemsSource der ListBox für die Züge, und es
			// werden automatisch immer die richtigen Züge angezeigt. Aufwand: ca. 2 Zeilen Code.
			// Die kryptische Zeile mit EnableCollectionSynchronization ermöglicht, dass die UI
			// auf die Änderung an movesReadOnly auch dann reagieren kann, wenn diese Änderung nicht
			// durch den UI-Thread ausgelöst wird.
			movesReadOnly = new ReadOnlyObservableCollection<GameMove>(moves);
			BindingOperations.EnableCollectionSynchronization(movesReadOnly, movesLock);

			providers = new Dictionary<Player, IMoveProvider>()
			{
				[Player.White] = white,
				[Player.Black] = black
			};
			Delay = delay;
		}

		// Startet den Spielthread
		public void Start()
		{
			if (gameThread != null)
				return;

			gameThread = new Thread(doRun);
			gameThread.Start();
		}

		// "Have you tried turning it off and on again?"
		// Tatsächlich ist diese Methode da, damit, falls sich der Spielthread gerade
		// im Code eines Spielers befindet, während dieser geändert wird, die aktuelle
		// Anfrage abgebrochen und eine neue Anfrage beim neuen Spieler gestartet wird.
		private void KickOver()
		{
			Stop();
			Start();
		}

		// Stoppt den Spielthread
		public void Stop()
		{
			if (gameThread == null)
				return;

			gameThread.Abort();
			gameThread = null;
		}

		/// <summary>
		/// Spielt eine gesamte Runde Mühle
		/// </summary>
		/// <returns>Das Spielergebnis</returns>
		private void doRun()
		{
			notifyOberservers();
			MoveResult res;

			// Äußere Schleife läuft einmal pro tatsächlichem Zug
			while (state.Result == GameResult.Running)
			{
				// Innere Schleife läuft einmal pro Zugversuch
				GameMove lastMove;
				do
				{
					res = state.TryApplyMove(lastMove = providers[state.NextToMove].GetNextMove(state));
				} while (res == MoveResult.InvalidMove);

				notifyOberservers();
				moves.Add(lastMove);

				Thread.Sleep(Delay);
			}
		}

		/// <summary>
		/// Registriert einen <see cref="IGameStateObserver"/> für kommende Spielereignisse 
		/// </summary>
		/// <param name="observer">Das zu notifizierende Objekt</param>
		public void AddObserver(IGameStateObserver observer)
		{
			observers.Add(observer);
			observer.Notify(state);
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

		/// <summary>
		/// Setzt das Spiel zurück auf den Zustand direkt nach move.
		/// </summary>
		public void RewindTo(GameMove to)
		{
			if (!moves.Contains(to))
				throw new ArgumentException("Kann nur auf einen Zug zurückspulen, der Teil des Spiels ist.");

			Stop();

			// Entgegenden dem Namen der Methode spulen wir nicht zurück,
			// sondern simulieren den Anfang des Spiels erneut
			GameState newState = new GameState();
			int numReplayed = 0;
			foreach (var move in moves)
			{
				if (newState.TryApplyMove(move) != MoveResult.OK)
					throw new InvalidOperationException("Vorheriger Zug konnte nicht nachvollzogen werden");

				numReplayed++;

				if (move == to)
					break;
			}
			state = newState;

			// Alle Züge, die jetzt nicht mehr existieren, werden gelöscht.
			// Rückwärts, um Aufrücken zu verhindern. O(n^2) -> O(n). Nicht dass die Verbesserung messbar wäre.
			int oldCount = moves.Count;
			for (int i = oldCount - 1; i >= numReplayed; i--)
				moves.RemoveAt(i);

			Start();
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
