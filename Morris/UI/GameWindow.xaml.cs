/*
 * GameWindow.xaml.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Morris
{
	/// <summary>
	/// Eine WPF-gestütze Mühle-GUI
	/// </summary>
	[SelectorName("GUI"), SingleInstance]
	internal partial class GameWindow : Window, IGameStateObserver, IMoveProvider
	{
		// Diese konstanten Steuern das Aussehen des Spielfelds.
		private const int BLOCK_SIZE = 100; // Sollte durch 2 teilbar sein
		private const int OFFSET_LEFT = 50;
		private const int OFFSET_TOP = 50;
		private const int OFFSET_BOTTOM = 90;
		private const int OFFSET_RIGHT = 10;
		private const int LINE_THICKNESS = 6; // Sollte durch 2 teilbar sein
		private const int PIECE_RADIUS = 30;
		private const int LEGEND_OFFSET = 30;
		private const int LABEL_BUFFER_SIZE = 40;
		private const int LEGEND_SIZE = 20;
		private const int STATUS_SIZE = 20;
		private const int STATUS_OFFSET_TOP = 10;

		private Ellipse[] pieces;
		private SolidColorBrush primaryColor = Brushes.Black;
		private Label status;

		public GameWindow()
		{
			// Dieser Konstruktor ist nicht sonderlich spannend zu lesen, da er einfach einen Haufen WPF-Objekte initialisiert.

			InitializeComponent();

			// Spielfield zeichnen
			for (int i = 0; i < GameState.FIELD_SIZE; i++)
			{
				var pointI = CoordinateTranslator.CoordinatesFromID(i);
				foreach (int j in GameState.GetConnected(i).Where(j => j < i))
				{
					var pointJ = CoordinateTranslator.CoordinatesFromID(j);

					// "Fire and Forget": Sobald wir die Objekte zum Grid hinzugefügt haben,
					// brauchen wir keine Referenzen mehr zu speichern.
					var h = new Rectangle();
					h.Fill = primaryColor;
					h.Width = BLOCK_SIZE * Math.Abs(pointI[1] - pointJ[1]) + LINE_THICKNESS;
					h.Height = BLOCK_SIZE * Math.Abs(pointI[0] - pointJ[0]) + LINE_THICKNESS;
					h.HorizontalAlignment = HorizontalAlignment.Left;
					h.VerticalAlignment = VerticalAlignment.Top;

					h.Margin = new Thickness(
						BLOCK_SIZE * Math.Min(pointI[1], pointJ[1]) + BLOCK_SIZE / 2 - LINE_THICKNESS / 2 + OFFSET_LEFT,
						BLOCK_SIZE * Math.Min(pointI[0], pointJ[0]) + BLOCK_SIZE / 2 - LINE_THICKNESS / 2 + OFFSET_TOP,
						0, 0);

					grid.Children.Add(h);
				}
			}

			// Beschriftung links
			for (int i = 0; i < 7; i++)
			{
				Label l = new Label();
				l.VerticalContentAlignment = VerticalAlignment.Center;
				l.HorizontalAlignment = HorizontalAlignment.Left;
				l.VerticalAlignment = VerticalAlignment.Top;
				l.Content = (7 - i).ToString();
				l.Height = LABEL_BUFFER_SIZE;

				l.Margin = new Thickness(OFFSET_LEFT - LEGEND_OFFSET, OFFSET_TOP + i * BLOCK_SIZE + BLOCK_SIZE / 2 - LABEL_BUFFER_SIZE / 2, 0, 0);
				l.FontSize = LEGEND_SIZE;
				l.Foreground = primaryColor;
				grid.Children.Add(l);
			}

			// Beschriftung unten
			for (int i = 0; i < 7; i++)
			{
				Label l = new Label();
				l.HorizontalContentAlignment = HorizontalAlignment.Center;
				l.HorizontalAlignment = HorizontalAlignment.Left;
				l.VerticalAlignment = VerticalAlignment.Top;
				l.Content = (char)('a' + i);
				l.Width = LABEL_BUFFER_SIZE;
				l.Margin = new Thickness(OFFSET_LEFT + i * BLOCK_SIZE + BLOCK_SIZE / 2 - LABEL_BUFFER_SIZE / 2, OFFSET_TOP + 7 * BLOCK_SIZE, 0, 0);
				l.FontSize = LEGEND_SIZE;
				l.Foreground = primaryColor;
				grid.Children.Add(l);
			}

			// Fenstergröße
			Height = OFFSET_TOP + 7 * BLOCK_SIZE + OFFSET_BOTTOM;
			Width = OFFSET_LEFT + 7 * BLOCK_SIZE + OFFSET_RIGHT;


			// Es gibt nicht für jeden tatsächlichen Spielstein eine Ellipse, die sich bewegt. Stattdessen gibt es eine
			// Ellipse auf jedem der 24 Spielfeldpunkte, die je nach Belegung Schwarz, weiß oder transparent ist
			pieces = new Ellipse[GameState.FIELD_SIZE];
			for (int i = 0; i < GameState.FIELD_SIZE; i++)
			{
				var point = CoordinateTranslator.CoordinatesFromID(i);
				// e ist hier lediglich dazu da, kürzer als pieces[i] zu sein.
				var e = pieces[i] = new Ellipse();
				e.Fill = Brushes.Transparent;
				e.Width = e.Height = 2 * PIECE_RADIUS;
				e.HorizontalAlignment = HorizontalAlignment.Left;
				e.VerticalAlignment = VerticalAlignment.Top;
				e.MouseDown += ellipseMouseDown;
				e.MouseMove += ellipseMouseMove;
				e.MouseUp += ellipseMouseUp;
				resetPositon(i);
				grid.Children.Add(e);
			}

			// Statusanzeige
			status = new Label();
			status.HorizontalContentAlignment = HorizontalAlignment.Center;
			status.HorizontalAlignment = HorizontalAlignment.Left;
			status.VerticalAlignment = VerticalAlignment.Top;
			status.Width = 7 * BLOCK_SIZE + LINE_THICKNESS;
			status.Margin = new Thickness(OFFSET_LEFT, STATUS_OFFSET_TOP, 0, 0);
			status.FontSize = STATUS_SIZE;
			grid.Children.Add(status);

			Show();
		}

		public void Notify(IReadOnlyGameState state)
		{
			Dispatcher.Invoke(() =>
			{
				// Ellipsen einfärben
				for (int i = 0; i < GameState.FIELD_SIZE; i++)
				{
					switch (state.Board[i])
					{
						case Occupation.Free:
							pieces[i].Fill = Brushes.Transparent;
							break;

						case Occupation.Black:
							pieces[i].Fill = Brushes.Black;
							break;

						case Occupation.White:
							pieces[i].Fill = Brushes.White;
							break;
					}
				}
				
				// Statusanzeige, falls das Spiel vorbei ist
				switch (state.Result)
				{
					case GameResult.BlackVictory:
						status.Content = "Schwarz hat gewonnen";
						break;

					case GameResult.WhiteVictory:
						status.Content = "Weiß hat gewonnen";
						break;

					case GameResult.Draw:
						status.Content = "Unentschieden";
						break;

					case GameResult.Running:
						status.Content = null;
						break;
				}
			});
		}

		private void resetPositon(int index)
		{
			var point = CoordinateTranslator.CoordinatesFromID(index);
			pieces[index].Margin = new Thickness(
				OFFSET_LEFT + BLOCK_SIZE * point[1] + BLOCK_SIZE / 2 - PIECE_RADIUS,
				OFFSET_TOP + BLOCK_SIZE * point[0] + BLOCK_SIZE / 2 - PIECE_RADIUS,
				0, 0);
		}


		// Überblick über die Benutzerinteraktion in der GUI:

		// Wenn ein Zug angefordert wird, wird die Methode GetNextMove vom Spielthread
		// aus aufgerufen. Diese bestimmt, ob je nach Spielzustand ein Klick oder ein
		// Drag/Drop erforderlich ist und setzt mode entsprechend. Außerdem wird, basierend
		// auf dem Spielfeld in validSources abgespeichert, welche Felder angeklickt bzw. gezogen
		// werden dürfen. Dieser Thread muss
		// nun blockieren, bis ein Zug durch den Benutzer asusgeführt wurde. Dazu wartet
		// er auf das AutoResetEvent sync.
		// Wenn ein erlaubtes Feld angeklickt wurde, wird die ellipseMouseDown-Methode aufgerufen
		// (Event Handler). Wenn lediglich ein Klick notwendig war, speichert die Methode die ID
		// des Felds in der Instanzvariable source und löst das AutoResetEvent aus.
		// Falls ein Drag/Drop stattfinden soll, wird die Modusvariable entsprechend modifiziert,
		// die Maus eingefangen und die relative Position des Mauszeigers zum Spielstein im Feld
		// Offset gespeichert. Immer wenn die Maus bewegt wird, wird der Spielstein nun so bewegt,
		// dass die relative Position zur Maus gleich bleibt. Wird die Maus losgelassen, wird der
		// Mauszeiger freigelassen und das Feld, auf dem der Spielstein fallengelassen wurde, berechnet.
		// Dieses wird dann in der Instanz gespeichert und sync ausgelöst.

		// Aus den gewonnen Daten wird dann wieder im Spielthread ein GameMove gebaut und geprüft, ob
		// dieser einen gegnerischen Stein schlägt. Wenn ja, wird ein weiterer Klick eingeholt.
		// *Nachdem* dies stattgefunden hat wird dann gegebenenfalls ein bewegter Spielstein zurück
		// auf seine Ursprungsposition gebracht (wie bereits erwähnt werden die Ellipsen nicht wirklich
		// bewegt, sondern die unsichtbare Ellipse am Zielort wird beim nächsten Aufruf von Notify
		// entsprechend eingefärbt).

		private enum Mode
		{
			Normal,
			ExpectingClick,
			ExpectingDrag,
			Dragging
		}

		private Mode mode = Mode.Normal;
		private Point offset;// Relative Position des bewegten Spielsteins zum Mauszeiger
		private bool error = false; // Ob der Stein auf ein nonexistentes Feld gezogen wurde
		private bool[] validSources = new bool[GameState.FIELD_SIZE]; // Felder, die angeklickt/gezogen werden dürfen
		private int source = -1; // Welcher Stein angeklickt/gezogen wurde
		private int destination = -1; // Wo der Stein hingezogen wurde
		AutoResetEvent sync = new AutoResetEvent(false); // Damit der Spielthread weiß, dass auf dem UI-Thread ein Zug gemacht wurde

		private void ellipseMouseDown(object sender, MouseEventArgs e)
		{
			Ellipse ellipse = sender as Ellipse;
			int index = Array.IndexOf(pieces, ellipse); // Welches Feld wurde angegklickt?

			if (index == -1)
				return;

			switch (mode)
			{
				case Mode.ExpectingClick:
					//  Wir wollten einen Klick haben
					if (!validSources[index])
						break;

					source = index;
					sync.Set(); // Zurück zum Spielthread
					break;

				case Mode.ExpectingDrag:
					// Dieser Klick repräsentiert den Anfang eines "Ziehprozesses"
					if (!validSources[index])
						break;

					source = index; // Wo Drag&Drop begonnen hat
					mode = Mode.Dragging; // Wir sind jetzt am ziehen
					offset = e.GetPosition(ellipse); // Wo befindet sich der Mausweiger im Spielstein?
					Mouse.Capture(ellipse); // Alle Mausevents werden jetzt an diesen Spielstein gesendet
					break;
			}
		}

		private void ellipseMouseMove(object sender, MouseEventArgs e)
		{
			Ellipse ellipse = sender as Ellipse;

			if (mode != Mode.Dragging || e.LeftButton != MouseButtonState.Pressed)
				return;

			// Den Spielstein, der gerade gezogen wird, auf die richtige Position bewegen
			Point pos = e.GetPosition(grid);
			ellipse.Margin = new Thickness(pos.X - offset.X, pos.Y - offset.Y, 0, 0);
		}

		private void ellipseMouseUp(object sender, MouseEventArgs e)
		{
			Ellipse ellipse = sender as Ellipse;

			if (mode != Mode.Dragging)
				return;

			// Das Spielfeld wird hier imaginär in ein Raster der Größe BLOCK_SIZE * BLOCK_SIZE aufgeteilt.
			// Im Zentrum jedes Blocks liegt ein Feld
			int left = (int)Math.Floor((ellipse.Margin.Left + PIECE_RADIUS - OFFSET_LEFT) / BLOCK_SIZE);
			// 6 - x, weil WPF von oben, Mühle aber von unten zählt
			int top = 6 - (int)Math.Floor((ellipse.Margin.Top + PIECE_RADIUS - OFFSET_TOP) / BLOCK_SIZE);

			try
			{
				destination = CoordinateTranslator.IDFromCoordinates(new[] { top, left });
			}
			catch (ArgumentException)
			{
				// IDFromCoordinates schmeißt hier, wenn der Spielstein auf ein Stelle im Spielfeld, die kein
				// Feld enthält oder aus dem Feld heraus gezogen wurde
				error = true;
			}

			// Maus freigeben (Mausevents werden jetzt wieder an das Element gesendet, das sich unter dem Mauszeiger befindet)
			Mouse.Capture(null);
			sync.Set(); // Zurück zum Spielthread
		}

		public GameMove GetNextMove(IReadOnlyGameState state)
		{
			// Status
			string phase;
			switch (state.GetPhase(state.NextToMove))
			{
				case Phase.Placing:
					phase = "platziert";
					break;
				case Phase.Moving:
					phase = "bewegt";
					break;
				default:
					phase = "fliegt";
					break;
			}
			Dispatcher.Invoke(() => status.Content = $"{(state.NextToMove == Player.Black ? "Schwarz" : "Weiß")} {phase}.");

			GameMove move = null;

			int oldsource = -1; // Enthält ggf. den Spielstein, der bewegt wurde

			if (state.GetPhase(state.NextToMove) == Phase.Placing)
			{
				// Wir brauchen nur einen Klick in ein freies Feld
				for (int i = 0; i < GameState.FIELD_SIZE; i++)
				{
					validSources[i] = state.Board[i] == Occupation.Free;
				}
				mode = Mode.ExpectingClick;

				sync.WaitOne(); // Warten...

				// source enthält jetzt den angeklickten Punkt
				mode = Mode.Normal;
				move = GameMove.Place(source);
			}
			else
			{
				// Wir brauchen Drag&Drop von einem vom aktuellen Spieler besetzten Feld
				for (int i = 0; i < GameState.FIELD_SIZE; i++)
				{
					validSources[i] = state.Board[i] == (Occupation)state.NextToMove;
				}
				mode = Mode.ExpectingDrag;
				error = false;

				sync.WaitOne(); // Warten

				// Ungültiges Feld, einfach einen ungültigen Zug zurückgeben, damit GetNextMove
				// erneut aufgerufen wird
				if (error)
				{
					Dispatcher.Invoke(() => resetPositon(source));
					return null;
				}

				// Source und Destination erhalten Anfang und Ende des Drag-Events
				mode = Mode.Normal;
				move = GameMove.Move(source, destination);
				oldsource = source;
			}

			if (state.IsValidMove(move) == MoveValidity.ClosesMill)
			{
				// Selbes Spiel wie oben.
				Dispatcher.Invoke(() => status.Content = "Bitte gegnerischen Stein zum Entfernen wählen.");
				for (int i = 0; i < GameState.FIELD_SIZE; i++)
				{
					validSources[i] = state.Board[i] == (Occupation)state.NextToMove.Opponent();
				}
				mode = Mode.ExpectingClick;
				sync.WaitOne();
				mode = Mode.Normal;
				move = move.WithRemove(source);
			}

			// Erst jetzt setzen wir den Stein zurück. Grund dafür ist, dass
			// so der Stein nicht hin- und herspringt. Wenn wir ihn direkt zurücksetzen würden,
			// kann es passieren, dass der Stein zurückgestzt wird und dann aber noch ein zu
			// entfernender Stein abgefragt wird. Erst nachdem dieser abgefragt wurde, wird dann
			// der Zielstein richtig eingefärbt. Wenn man aber den Stein erst ganz am Ende zurücksetzt,
			// gibt es keine derarten Sprünge.
			if (oldsource >= 0)
				Dispatcher.Invoke(() => {
					// Kleiner Hack, es sieht schöner aus, wenn der Stein schon hier ausgeblendet wird. Wir wissen, dass das der
					// Fall sein wird, solange der Zug gültig ist, deshalb prüfen wir das hier schonmal.
					if (state.IsValidMove(move) == MoveValidity.Valid) pieces[oldsource].Fill = Brushes.Transparent;
					resetPositon(oldsource);
					});

			Dispatcher.Invoke(() => status.Content = null);
			return move;
		}
	}
}
