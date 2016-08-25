/*
 * GameMove.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Repräsentiert einen potentiell ungültigen Spielzug
	/// </summary>
	public class GameMove
	{
		/// <summary>
		/// Falls in dem Zug ein Stein bewegt wird, die Position des Steines vor dem Zug
		/// </summary>
		public int? From { get; private set; }

		/// <summary>
		/// Die Position eines neuen oder bewegten Steines nach dem Zug
		/// </summary>
		public int To { get; private set; }

		/// <summary>
		/// Falls der Zug eine Mühle schließt, die Position des infolgedessen entfernten gegnerischen Steines
		/// </summary>
		public int? Remove { get; private set; }

		private GameMove(int? from, int to, int? remove)
		{
			From = from;
			To = to;
			Remove = remove;
		}

		/// <summary>
		/// Erstellt einen neuen Zug, der das Setzen eines neuen Steines repräsentiert
		/// </summary>
		/// <param name="position">Wo der neue Stein platziert werden soll</param>
		/// <returns>Einen nicht zwangsläufig gütligen Spielzug</returns>
		public static GameMove Place(int position)
		{
			return new GameMove(null, position, null);
		}

		/// <summary>
		/// Erstellt einen neuen Zug, der das Setzen eines neuen Steines und das Schließen einer Mühle repräsentiert
		/// </summary>
		/// <param name="position">Wo der neue Stein platziert werden soll</param>
		/// <param name="remove">Welcher gegnerische Stein entfernt werden soll</param>
		/// <returns>Einen nicht zwangsläufig gültigen Spielzug</returns>
		public static GameMove PlaceRemove(int position, int remove)
		{
			return new GameMove(null, position, remove);
		}

		/// <summary>
		/// Erstellt einen neuen Zug, der das Bewegen eines Steines repräsentiert
		/// </summary>
		/// <param name="from">Wo sich der Stein vor dem Zug befindet</param>
		/// <param name="to">Wo sich der Stein nach dem Zug befinden soll</param>
		/// <returns>Einen nicht zwangsläufig gültigen Spielzug</returns>
		public static GameMove Move(int from, int to)
		{
			return new GameMove(from, to, null);
		}

		/// <summary>
		/// Erstellt einen neuen Zug, der das Bewegen eines Steines und das Schließen einer Mühle repräsentiert
		/// </summary>
		/// <param name="from">Wo sich der Stein vor dem Zug befindet</param>
		/// <param name="to">Wo sich der Stein nach dem Zug befindet</param>
		/// <param name="remove">Welcher gegnerische Stein entfernt werden soll</param>
		/// <returns>Einen nicht zwangsläufig gültigen Spielzug</returns>
		public static GameMove MoveRemove(int from, int to, int remove)
		{
			return new GameMove(from, to, remove);
		}

		// Die nachfolgenden beiden Methoden existieren, weil GameMove immutable sein soll, damit es keine lustigen
		// Aliasing-Bugs gibt, wenn MoveProviders komische Dinge mit den Zügen machen, die sie von GameState.BasicMoves
		// zurückbekommen

		/// <summary>
		/// Gibt eine Kopie des GameMove ohne Informationen zur Entfernung zurück
		/// </summary>
		/// <returns>Eine neuen Spielzug</returns>
		public GameMove WithoutRemove()
		{
			return new GameMove(From, To, null);
		}

		/// <summary>
		/// Erstellt eine Kopie des GameMove mit Zusatzinformation zur Entfernung eines gegnerischen Steins zurück
		/// </summary>
		/// <param name="remove">Welcher gegnerische Stein entfernt werden soll</param>
		/// <returns>Einen neuen Spielzug</returns>
		public GameMove WithRemove(int remove)
		{
			return new GameMove(From, To, remove);
		}
	}
}
