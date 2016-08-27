/*
 * GameWindow.xaml.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Morris
{

	/// <summary>
	/// Eine WPF-gestütze Mühle-GUI
	/// </summary>
	public partial class GameWindow : Window, IGameStateObserver
	{

		private const int BLOCK_SIZE = 100; // Sollte durch 2 teilbar sein
		private const int OFFSET_LEFT = 50;
		private const int OFFSET_TOP = 70;
		private const int OFFSET_BOTTOM = 90;
		private const int OFFSET_RIGHT = 10;
		private const int LINE_THICKNESS = 6; // Sollte durch 2 teilbar sein
		private const int PIECE_RADIUS = 30;
		private const int LEGEND_OFFSET = 30;
		private const int LABEL_BUFFER_SIZE = 40;

		private Ellipse[] pieces;

		private SolidColorBrush primaryColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));

		private Label status;

		public GameWindow()
		{
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
				l.FontSize = 20;
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
				l.FontSize = 20;
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
				var e = new Ellipse();
				e.Fill = null;
				e.Width = e.Height = 2 * PIECE_RADIUS;
				e.HorizontalAlignment = HorizontalAlignment.Left;
				e.VerticalAlignment = VerticalAlignment.Top;

				e.Margin = new Thickness(
					OFFSET_LEFT + BLOCK_SIZE * point[1] + BLOCK_SIZE / 2 - PIECE_RADIUS,
					OFFSET_TOP + BLOCK_SIZE * point[0] + BLOCK_SIZE / 2 - PIECE_RADIUS,
					0, 0);

				grid.Children.Add(e);

				pieces[i] = e;
			}

			// Statusanzeige
			status = new Label();
			status.HorizontalContentAlignment = HorizontalAlignment.Center;
			status.HorizontalAlignment = HorizontalAlignment.Left;
			status.VerticalAlignment = VerticalAlignment.Top;
			status.Width = 7 * BLOCK_SIZE + LINE_THICKNESS;
			status.Margin = new Thickness(OFFSET_LEFT, 10, 0, 0);
			status.FontSize = 40;
			grid.Children.Add(status);
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
							pieces[i].Fill = null;
							break;

						case Occupation.Black:
							pieces[i].Fill = new SolidColorBrush(Colors.Black);
							break;

						case Occupation.White:
							pieces[i].Fill = new SolidColorBrush(Colors.White);
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
				}
			});
		}
	}
}
