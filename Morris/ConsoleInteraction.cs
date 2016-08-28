/*
 * ConsoleInteraction.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license.
 */

using System;
using System.Linq;
using System.Text;

namespace Morris
{
	/// <summary>
	/// Ermöglicht Eingabe und Ausgabe der Spielsituation auf der Konsole.
	/// </summary>
	class ConsoleInteraction : IGameStateObserver, IMoveProvider
	{
		public ConsoleInteraction()
		{
			// Konsolenparameter setzen
			Console.OutputEncoding = Encoding.Unicode;
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Clear();
		}

		public void Notify(IReadOnlyGameState state)
		{
			Console.Clear();

			// Ein mit Leerzeichen initialisiertes 13*13 Jagged Array
			char[][] field = Enumerable.Repeat(0, 13).Select(_ => Enumerable.Repeat(' ', 13).ToArray()).ToArray();

			// Spielpositionen mit Belegung
			for (int i = 0; i < GameState.FIELD_SIZE; i++)
			{
				var point = CoordinateTranslator.CoordinatesFromID(i).Select(x => 2 * x).ToArray();
				switch (state.Board[i])
				{
					case Occupation.Free:
						field[point[0]][point[1]] = '▫';
						break;

					case Occupation.Black:
						field[point[0]][point[1]] = '●';
						break;

					case Occupation.White:
						field[point[0]][point[1]] = '○';
						break;

				}
			}

			// Linien auf dem Spielfeld zeichnen. Wo diese hingehören, wird aus den Verbindungsdaten, die GameState
			// bereithält, "on the fly" bestimmt. Diesen Schritt des Zeichnens des Spielfelds könnte man cachen,
			// das wäre hier aber mit Blick auf die Größe des Spielfelds eine premature optimization.
			for (int i = 0; i < GameState.FIELD_SIZE; i++)
			{
				var pointI = CoordinateTranslator.CoordinatesFromID(i).Select(x => 2 * x).ToArray();
				foreach (int j in GameState.GetConnected(i).Where(j => j < i))
				{
					var pointJ = CoordinateTranslator.CoordinatesFromID(j).Select(x => 2 * x).ToArray();

					if (pointI[0] == pointJ[0])
					{
						// Horizontale Linien
						for (int k = Math.Min(pointI[1], pointJ[1]) + 1; k <= Math.Max(pointI[1], pointJ[1]) - 1; k++)
						{
							field[pointI[0]][k] = '-';
						}
					}
					else
					{
						// Vertikale Linien
						for (int k = Math.Min(pointI[0], pointJ[0]) + 1; k <= Math.Max(pointI[0], pointJ[0]) - 1; k++)
						{
							field[k][pointI[1]] = '|';
						}
					}
				}
			}

			// Spielfeld mit Koordinaten ausgeben
			for (int i = 0; i < 13; i++)
			{
				Console.Write(i % 2 == 0 ? (char)('0' + 7 - i / 2) : ' ');
				Console.Write(' ');
				Console.WriteLine(new string(field[i]));
			}
			Console.Write("  ");
			Console.WriteLine(string.Join(" ", Enumerable.Range('a', 7).Select(x => (char)x)));

			// Spielstatus mitteilen, falls das Spiel nicht mehr läuft.
			switch (state.Result)
			{
				case GameResult.BlackVictory:
					Console.WriteLine("Schwarz hat gewonnen.");
					break;
				case GameResult.WhiteVictory:
					Console.WriteLine("Weiß hat gewonnnen.");
					break;
				case GameResult.Draw:
					Console.WriteLine("Unentschieden.");
					break;
			}
		}

		public GameMove GetNextMove(IReadOnlyGameState state)
		{
			// So lange wieder fragen, bis ein Input eingegeben wird, der geparst werden kann
			// Ob dieser Input dann einen gültigen Zug repräsentiert, ist wieder eine andere Frage
			while (true)
			{
				try
				{
					string phase;
					switch (state.GetPhase(state.NextToMove))
					{
						case Phase.Placing:
							phase = "Platziert";
							break;
						case Phase.Moving:
							phase = "Bewegt";
							break;
						default:
							phase = "Fliegt";
							break;
					}
					Console.Write($"{(state.NextToMove == Player.Black ? "Schwarz" : "Weiß")} am Zug ({phase}): ");

					// Eingabe parsen
					// Format {a1-}b2{,c3}
					// Bedeutet setze von a1 nach b2 und schlage c3
					// Teile in {} sind optional
					var rawInput = Console.ReadLine().ToLower();
					var input = rawInput.Split(new[] { ',', '-' }).Select(pos => CoordinateTranslator.IDFromHumanReadable(pos)).ToArray();
					switch (input.Length)
					{
						case 1:
							return GameMove.Place(input[0]);

						case 2:
							if (rawInput[2] == '-')
								return GameMove.Move(input[0], input[1]);
							if (rawInput[2] == ',')
								return GameMove.PlaceRemove(input[0], input[1]);
							throw new InvalidOperationException();

						case 3:
							if (rawInput[2] != '-' || rawInput[5] != ',')
								throw new InvalidOperationException();
							return GameMove.MoveRemove(input[0], input[1], input[2]);
					}

				}
				catch
				{
					// Einfach nocheinmal fragen, wenn der Input nicht geparst werden konnte
				}
			}
		}
	}
}
