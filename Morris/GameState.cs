/*
 * GameState.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	/// <summary>
	/// Repräsentiert eine Mühle-Spielsituation
	/// </summary>
	public class GameState : IReadOnlyGameState
	{
		public Occupation[] Board { get; private set; }
		public Player NextToMove { get; private set; }
		public GameResult Result { get; private set; }

		private List<Occupation[]> history = new List<Occupation[]>();
			 
		private Dictionary<Player, Phase> playerPhase;
		private Dictionary<Player, int> stonesPlaced;
		private Dictionary<Player, int> currentStones;

		public const int FIELD_SIZE = 24;
		public const int STONES_MAX = 9;
		public const int FLYING_MAX = 3;

		// Jeder Eintrag repräsentiert eine mögliche Mühle
		public static readonly ReadOnlyCollection<ReadOnlyCollection<int>> Mills = Array.AsReadOnly(new[]
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
			new[] { 5, 13, 20 },
			new[] { 2, 14, 23 }
		}.Select(mill => Array.AsReadOnly(mill)).ToArray());

		// Gibt an, ob zwei Felder verbunden sind.
		// Wird aus den Daten in mills im statischen Konstruktor generiert
		private static bool[,] connections;

		static GameState()
		{
			connections = new bool[FIELD_SIZE, FIELD_SIZE];
			foreach (var mill in Mills)
			{
				for (int i = 0; i < mill.Count - 1; i++)
				{
					connections[mill[i], mill[i + 1]] = true;
					connections[mill[i + 1], mill[i]] = true;
				}
			}
		}

		/// <summary>
		/// Gibt alle Felder zurück, die mit einem Feld verbunden sind
		/// </summary>
		/// <param name="ID">Das zu untersuchende Feld</param>
		public static IEnumerable<int> GetConnected(int ID)
		{
			return Enumerable.Range(0, FIELD_SIZE).Where(id => connections[ID, id]);
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

			stonesPlaced = new Dictionary<Player, int>()
			{
				[Player.Black] = 0,
				[Player.White] = 0
			};

			currentStones = new Dictionary<Player, int>()
			{
				[Player.Black] = 0,
				[Player.White] = 0
			};
		}

		ReadOnlyCollection<Occupation> IReadOnlyGameState.Board
		{
			get
			{
				return Array.AsReadOnly(Board);
			}
		}

		// Die folgenden drei Methoden existieren, weil selbst eine Setter-Only Property,
		// die die zugrundeliegenden Dictionaries zurückgibt, modifizierbar wäre.
		// IReadOnlyDictionary ist keine Lösung, weil es Nutzer dieser Klasse eigentlich
		// nicht interessiert, wie diese Daten gespeichtert sind. In einer späteren Version
		// könnte sich das auch ändern (weil Dictionaries ziemlich "overkill" zur Speicherung
		// zweier Ganzzahlen sind).

		/// <summary>
		/// Gibt die Phase, in der sich ein Spieler befindet, zurück
		/// </summary>
		/// <param name="player">Der Spieler, dessen Phase gesucht ist</param>
		/// <returns>Eine Phase</returns>
		public Phase GetPhase(Player player)
		{
			return playerPhase[player];
		}

		/// <summary>
		/// Gibt die von einem Spieler insgesamt gesetzten Steine zurück
		/// </summary>
		/// <param name="player">Der Spieler, dessen gesetzten Steine gesucht sind</param>
		/// <returns>Eine Zahl zwischen 0 und <see cref="STONES_MAX"/></returns>
		public int GetStonesPlaced(Player player)
		{
			return stonesPlaced[player];
		}

		/// <summary>
		/// Gibt die Zahl der Steine auf dem Spielfeld, die einem Spieler gehören, zurück
		/// </summary>
		/// <param name="player">Der Spieler, dessen aktuelle Steinzahl gesucht ist</param>
		/// <returns>Eine Zahl zwischen 0 und <see cref="STONES_MAX"/></returns>
		public int GetCurrentStones(Player player)
		{
			return currentStones[player];
		}

		// Gibt alle Paare von Spielfeldpositionen (p, p2) zurück, sodass
		// p vom aktuellen Spieler belegt ist und p2 frei ist und
		// zusätzlich pred(p, p2) erfüllt ist.
		// Hilfsmethode für BasicMoves; in C# 7 könnte man da eine coole
		// lokale Funktion draus machen, das hier ist aber ein
		// C# 6-Projekt...
		private IEnumerable<GameMove> pairs(Func<int, int, bool> pred)
		{
			return Enumerable.Range(0, FIELD_SIZE)
				.Where(p => (int)Board[p] == (int)NextToMove)
				.SelectMany(p => Enumerable.Range(0, FIELD_SIZE)
					.Where(p2 => Board[p2] == Occupation.Free && pred(p, p2))
					.Select(p2 => GameMove.Move(p, p2)));
		}

		/// <summary>
		/// Gibt alle möglichen Spielzüge für den Spieler, der aktuell am Zug ist,
		/// ohne Informationen über zu entfernende gegnerische Steine zurück.
		/// 
		/// Für von dieser Methode zurückgegebene Züge kann mithilfe von
		/// <see cref="IsValidMove(GameMove)"/> bestimmt werden, ob ein Stein
		/// entfernt werden darf.
		/// </summary>
		public IEnumerable<GameMove> BasicMoves()
		{
			switch (playerPhase[NextToMove])
			{
				case Phase.Placing:
					// Ein neuer Zug für alle freien Felder
					return Enumerable.Range(0, FIELD_SIZE)
						.Where(p => Board[p] == Occupation.Free)
						.Select(p => GameMove.Place(p));

				case Phase.Moving:
					// Ein neuer Zug für jedes Paar von Positionen (p, p2), bei dem
					// p vom aktuellen Spieler belegt ist und p2 frei und mit p
					// verbunden ist
					return pairs((p, p2) => connections[p, p2]);

				case Phase.Flying:
					// Ein neuer Zug für jedes Paar von Positionen (p, p2), bei dem
					// p vom aktuellen Spieler belegt ist und p2 frei ist
					return pairs((p, p2) => true);

				default:
					throw new InvalidOperationException("Sollte nie erreicht werden");
			}
		}

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
			bool millClosed = Mills.Any(mill => // Es muss eine potentielle Mühle geben, die
				mill.Contains(move.To) && // den neu gesetzten Stein enthält und
				mill.All(point =>  // bei der alle Punkte
					(!move.From.HasValue || point != move.From) && // nicht der Ursprungspunkt der aktuellen Steinbewegung sind und
					(int)Board[point] == (int)NextToMove || point == move.To)); // entweder schon vom Spieler bestzt sind oder Ziel der aktuellen Steinbewegung sind.

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
					.All(point => Mills.Any(mill => mill.Contains(point) && mill.All(mp => (int)Board[point] == (int)NextToMove.Opponent())));

				if (!allInMill && Mills.Any(mill => mill.Contains(move.Remove.Value) && mill.All(point => (int)Board[point] == (int)NextToMove.Opponent())))
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
			if (Result != GameResult.Running)
				return MoveResult.GameNotRunning;

			if (IsValidMove(move) != MoveValidity.Valid)
				return MoveResult.InvalidMove;

			// Weiteres Error Checking ist nicht notwendig, da dieses in IsValidMove vorgenommen wurde

			history.Add((Occupation[])Board.Clone());

			// ggf. wegbewegter Stein
			if (move.From.HasValue)
				Board[move.From.Value] = Occupation.Free;
			else
			{
				currentStones[NextToMove]++;
				if (++stonesPlaced[NextToMove] == STONES_MAX)
					playerPhase[NextToMove] = Phase.Moving;
			}

			// Hinbewegter Stein
			Board[move.To] = (Occupation)NextToMove;

			// Wiederholte Stellung
			if (!playerPhase.Values.Contains(Phase.Placing) && history.Any(pastBoard => Board.SequenceEqual(pastBoard)))
				Result = GameResult.Draw;

			// ggf. entfernter Stein
			if (move.Remove.HasValue)
			{
				Board[move.Remove.Value] = Occupation.Free;
				// Hier darf kein short-circuiting verwendet werden
				if (playerPhase[NextToMove.Opponent()] == Phase.Moving & --currentStones[NextToMove.Opponent()] == FLYING_MAX)
					playerPhase[NextToMove.Opponent()] = Phase.Flying;
			}

			// Gegner hat nur noch zwei Steine
			if (playerPhase[NextToMove.Opponent()] != Phase.Placing && currentStones[NextToMove.Opponent()] == 2)
				Result = (GameResult)NextToMove;

			// Gegner ist jetzt dran
			NextToMove = NextToMove.Opponent();

			// Wenn der (jetzt) aktuelle Spieler keine gültigen Züge hat,
			// hat er verloren
			if (!BasicMoves().Any())
				Result = (GameResult)NextToMove.Opponent();

			return MoveResult.OK;
		}
	}
}
