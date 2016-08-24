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
		public bool IsGameRunning { get; private set; }

		private Dictionary<Player, Phase> playerPhase;

		private const int FIELD_SIZE = 24;

		static GameState()
		{

			connections = new bool[FIELD_SIZE, FIELD_SIZE];
			foreach (int[] mill in mills)
			{
				for (int i = 0; i < mill.Length - 1; i++)
				{
					connections[mill[i], mill[i + 1]] = true;
					connections[mill[i + 1], mill[i]] = true;
				}
			}
		}

		public GameState()
		{
			// Leeres Feld
			Board = Enumerable.Repeat(Occupation.Free, FIELD_SIZE).ToArray();
			NextToMove = Player.White;
			Result = GameResult.Running;
			playerPhase = new Dictionary<Player, Phase>()
			{
				[Player.Black] = Phase.Placing,
				[Player.White] = Phase.Placing
			};
		}

		// Jeder Eintrag repräsentiert eine mögliche Mühle
		private static readonly int[][] mills = new[]
		{
			// Horizontal
			new[] { 0, 1, 2 },
			new[] { 3, 4, 5 },
			new[] { 6, 7, 8 },
			new[] { 9, 10, 11 },
			new[] { 12, 13, 14 },
			new[] { 15, 16, 17 },
			new[] { 18, 19, 20 },
			new[] { 21, 22, 23 },

			//Vertikal
			new[] { 0, 9, 21 },
			new[] { 3, 10, 18 },
			new[] { 6, 11, 15 },
			new[] { 1, 4, 7 },
			new[] { 16, 19, 22 },
			new[] { 8, 12, 17 },
			new[] { 5, 12, 20 },
			new[] { 2, 14, 23 }
		};

		// Gibt an, ob zwei Felder verbunden sind.
		// Wird aus den Daten in mills im statischen Konstruktor generiert
		private static bool[,] connections;

		/// <summary>
		/// Bestimmt, ob ein Zug in der aktuellen Spielsituation gültig ist
		/// </summary>
		/// <param name="move">Der Zug, der überprüft werden soll</param>
		/// <returns>
		/// <para><see cref="MoveValidity.Valid"/>, wenn der Zug gültig ist.</para>
		/// <para><see cref="MoveValidity.ClosesMill"/>, wenn der Zug gültig ist, aber eine Mühle schließt, und kein zu entfernender Stein angegeben wurde.</para>
		/// <para><see cref="MoveValidity.DoesNotCloseMill"/>, wenn der Zug gültig ist, aber ein zu entfernender Stein angegeben wurde, obwohl der Zug keine Mühle schließt.</para>
		/// <para><see cref="MoveValidity.Invalid"/>, wenn der Zug ungültig ist.</para>
		/// </returns>
		public MoveValidity IsValidMove(GameMove move)
		{
			// Die Verifikation wurde aus Gründen der Lesbarkeit nicht in eine gigantische
			// bedingte Anweisung gepackt, auch wenn dies kompakter und eventuell marginal
			// schneller wäre

			// 1.: Ziel verifizieren
			if (move.To < 0 || move.To >= FIELD_SIZE)
				return MoveValidity.Invalid; // OOB

			if (Board[move.To] != Occupation.Free)
				return MoveValidity.Invalid; // Zielplatz belegt

			// 2.: "Steinherkunft" verifizieren
			if (move.From.HasValue) // Bewegung
			{
				if (playerPhase[NextToMove] == Phase.Placing)
					return MoveValidity.Invalid; // Darf noch keinen Stein bewegen

				if (move.From < 0 || move.From >= FIELD_SIZE)
					return MoveValidity.Invalid; // OOB

				if ((int)Board[move.From.Value] != (int)NextToMove) // In der Enum-Definition von Occupation gleichgesetzt
					return MoveValidity.Invalid; // Kein Stein zum Bewegen

				if (playerPhase[NextToMove] == Phase.Moving && !connections[move.From.Value, move.To])
					return MoveValidity.Invalid; // Darf noch nicht springen
			}
			else if (playerPhase[NextToMove] != Phase.Placing)
				return MoveValidity.Invalid; // Darf keinen Stein mehr platzieren

			// 3.: Wurde eine Mühle geschlossen?
			bool millClosed = mills.Any(mill => mill.All(point => (int)Board[point] == (int)NextToMove || point == move.To));

			// 4.: Verifikation des Mühlenparameters
			if (millClosed)
			{
				if (!move.Remove.HasValue)
					return MoveValidity.ClosesMill; // Kein Parameter gegeben

				if (move.Remove < 0 || move.Remove >= FIELD_SIZE)
					return MoveValidity.Invalid; // OOB

				if ((int)Board[move.Remove.Value] != (int)NextToMove.Opponent())
					return MoveValidity.Invalid; // Auf dem Feld liegt kein gegnerischer Stein

				// Es darf kein Stein aus einer geschlossenen Mühle entnommen werden, falls es Steine gibt, die in keiner
				// Mühle sind.
				// Die LINQ-Abfrage drückt folgendes aus:
				// "Für alle gegnerischen Steine gilt, dass eine Mühle existiert, die diesen Stein enthält und von der alle
				// Felder durch gegnerische Steine besetzt sind (die Mühle also geschlossen ist)"
				bool allInMill = Enumerable.Range(0, FIELD_SIZE)
					.Where(point => (int)Board[point] != (int)NextToMove.Opponent())
					.All(point => mills.Any(mill => mill.Contains(point) && mill.All(mp => (int)Board[point] == (int)NextToMove.Opponent())));

				if (!allInMill && mills.Any(mill => mill.Contains(move.Remove.Value) && mill.All(point => (int)Board[point] == (int)NextToMove.Opponent())))
					return MoveValidity.Invalid; // Versuch, einen Stein aus einer Mühle zu entfernen, obwohl Steine frei sind
			}
			else if (move.Remove.HasValue)
				return MoveValidity.DoesNotCloseMill; // Unnötiger Parameter gegeben

			return MoveValidity.Valid;
		}

		/// <summary>
		/// Versucht, einen Zug auszuführen
		/// </summary>
		/// <param name="move">Der auszuführende Spielzug</param>
		/// <returns>
		/// <para><see cref="MoveResult.GameNotRunning"/>, wenn das Spiel nicht läuft.</para>
		/// <para><see cref="MoveResult.InvalidMove"/>, wenn der Zug ungültig ist. <see cref="GameState.IsValidMove(GameMove)"/> kann helfen, zu bestimmen, warum der Zug nicht gültig ist.</para>
		/// <para><see cref="MoveResult.OK"/>, wenn der Zug erfolgreicht ausgeführt wurde.</para>
		/// </returns>
		public MoveResult TryApplyMove(GameMove move)
		{
			if (!IsGameRunning)
				return MoveResult.GameNotRunning;

			if (IsValidMove(move) != MoveValidity.Valid)
				return MoveResult.InvalidMove;

			// Weiteres Error Checking ist nicht notwendig, da dieses in IsValidMove vorgenommen wurde

			// ggf. wegbewegter Stein
			if (move.From.HasValue)
				Board[move.From.Value] = Occupation.Free;

			// Hinbewegter Stein
			Board[move.To] = (Occupation)NextToMove;

			// ggf. entfernter Stein
			if (move.Remove.HasValue)
				Board[move.Remove.Value] = Occupation.Free;

			// Gegner ist jetzt dran
			NextToMove = NextToMove.Opponent();

			return MoveResult.OK;
		}


	}
}
