/*
 * Program.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Windows;

namespace Morris
{
	/// <summary>
	/// Enthält den Haupteinsprungspunkt für unser Programm
	/// </summary>
	internal class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			// Die Verwendung der Controller-Klasse und der anderen Klassen
			// im Ordner Control ist für die Kernlogik des Spiels vollkommen
			// irrelevant. Stattdessen könnten hier auch ein paar fest ein-
			// programmierte Befehle zum Erstellen das Game-Objekts oder ein
			// Command Line Interface oder oder oder stehen, je nachdem, wie
			// die Logik eingesetzt wird. Die Architektur der Logik lässt es zu,
			// die Logik (und auch die KIs) in allen möglichen Formen der
			// Benutzerinteraktion wiederzuverwenden, sei es eine WPF-Applikation,
			// eine Kommandozeilenanwendung, die auch auf macOS und Linux läuft,
			// eine Xamarin Mobile App, die auf iOS und Android zuhause ist,
			// eine ASP.NET-Webapplikation, die das Spiel im Browser spielbar
			// macht, eine App auf der Universal Windows Platform, sodass das
			// Spiel auf Windows Phone und Xbox One läuft, etc. etc. etc.
			// Und das ist nur die "Spitze des Eisbergs", denn es können auch
			// KIs und Displays eingebunden werden, die in C++/CLI verfasst sind,
			// sprich Qt, OpenGL, etc.
			// Allerdings sind das lediglich Möglichkeiten, die die Architektur
			// der Software zulässt, tatsächlich vorhanden sind eine WPF-GUI
			// zur Auswahl von KI, Display und zur Kontrolle des Spiel sowie
			// GUIs für WPF und die Konsole sowie eine handvoll KIs, die alle
			// recht mäßig spielen, und eine Brücke zur Mühleplattform Malom,
			// durch die zwei weitere KIs verfügbar werden: Eine perfekte KI,
			// die auf der vollständigen Lösung von Mühle beruht und stets
			// das spieltheoretisch beste aus einer Spielsituation macht, die
			// allerdings auch eine Datenbank benötigt, die groß und recht
			// aufwändig zu berechnen ist, und eine heuristische KI, die auf
			// Alpha-Beta-Pruning beruht.
			new Application().Run(new Controller());
		}
	}
}
