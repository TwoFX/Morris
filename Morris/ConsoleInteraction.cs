using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morris
{
	class ConsoleInteraction : IGameStateObserver, IMoveProvider
	{
		public void Notify(IReadOnlyGameState state)
		{
			// Ein mit Leerzeichen initialisiertes 8*8 Jagged Array
			char[][] field = Enumerable.Repeat(0, 8).Select(_ => Enumerable.Repeat(' ', 8).ToArray()).ToArray();

			for (int i = 0; i < GameState.FIELD_SIZE; i++)
			{
				var point = CoordinateTranslator.CoordinatesFromID(i);
				switch (state.Board[i])
				{
					case Occupation.Free:
						field[point.Item1][point.Item2] = 'F';
						break;

					case Occupation.Black:
						field[point.Item1][point.Item2] = 'B';
						break;

					case Occupation.White:
						field[point.Item1][point.Item2] = 'W';
						break;

				}
			}

			//for (int i = 0; i < GameState.FIELD_SIZE; i++)
			//{
			//	foreach (int j in GameState.GetConnected(i))
			//	{

			//	}
			//}

			foreach (var row in field)
			{
				Console.WriteLine(new string(row));
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
					Console.Write("Bitte gib einen Zug ein: ");

					// Eingabe parsen
					var input = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					var inputPositions = input.Skip(1).Select(pos => CoordinateTranslator.IDFromHumanReadable(pos)).ToArray();

					switch (input[0])
					{
						case "p": // place
							return GameMove.Place(inputPositions[0]);

						case "pr": // place remove
							return GameMove.PlaceRemove(inputPositions[0], inputPositions[1]);

						case "m": // move
							return GameMove.Move(inputPositions[0], inputPositions[1]);

						case "mr": // move remove
							return GameMove.MoveRemove(inputPositions[0], inputPositions[1], inputPositions[2]);

						default:
							throw new InvalidOperationException();
					}

				}
				catch
				{
					// Einfach nocheinmal versuchen...
				}
			}
		}
	}
}
