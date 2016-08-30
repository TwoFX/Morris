/*
 * Controller.xaml.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace Morris
{
	/// <summary>
	/// Gibt dem Benutzer die Möglichkeit, ein Mühlespiel zu kontrollieren
	/// </summary>
	internal partial class Controller : Window
	{
		public Controller()
		{
			InitializeComponent();

			populateLists(AppDomain.CurrentDomain.GetAssemblies());

			whiteBox.ItemsSource = blackBox.ItemsSource = players;
			displayBox.ItemsSource = displays;
		}

		// Das aktuelle Spiel
		private Game theGame;

		// Die Objekte, die die ComboBoxen und die ListBox nehmen und wieder zurückgeben
		private ObservableCollection<SelectorType> players = new ObservableCollection<SelectorType>();
		private ObservableCollection<SelectorType> displays = new ObservableCollection<SelectorType>();

		// Instanzen, die gecacht werden, weil es von dem Typen nur eine Instanz geben darf
		private Dictionary<Type, object> singleInstances = new Dictionary<Type, object>();

		// Instanzen, die gecacht werden, weil das Display wieder abgewählt werden kann und wir die Instanz
		// brauchen, um Game.RemoveObserver damit aufzurufen.
		private Dictionary<Type, IGameStateObserver> displayObjects = new Dictionary<Type, IGameStateObserver>();

		// Fügt alle relevatenten Typen in den angegebenen Assemblies in die entsprechenden Listen ein
		private void populateLists(IEnumerable<Assembly> assemblies)
		{
			foreach (var type in getTypes(typeof(IMoveProvider), assemblies))
				players.Add(type);

			foreach (var type in getTypes(typeof(IGameStateObserver), assemblies))
				displays.Add(type);
		}

		// Gibt eine Instanz des angegebenen Typen unter Beachtung der
		// Möglichkeit, dass der Typ das SingleInstanceAttribute haben kann,
		// zurück.
		private object getInstance(SelectorType selectorType)
		{
			Type type = selectorType.Type;

			// Anmerkung: Wir wissen an dieser Stelle sicher, dass type einen
			// parameterlosen Konstruktor hat

			// Für jede Verwendung eine neue Instanz?
			if (type.GetCustomAttribute<SingleInstanceAttribute>() == null)
				return Activator.CreateInstance(type);

			// C# 7 bekommt out variables, dann wird das schöner
			object result;
			if (singleInstances.TryGetValue(type, out result))
				return result;

			return singleInstances[type] = Activator.CreateInstance(type);
		}
		
		// Gibt alle Typen in assemblies zurück, die @interface implementieren und einen parameterlosen Konstruktor haben
		private IEnumerable<SelectorType> getTypes(Type @interface, IEnumerable<Assembly> assemblies)
		{
			return assemblies
				.SelectMany(s => s.GetTypes())
				.Where(type => type.IsClass && type.GetInterfaces().Contains(@interface) && type.GetConstructor(Type.EmptyTypes) != null)
				.Select(type => new SelectorType(type));
		}

		// Versucht, ein neues Display zu registrieren
		private void tryAddDisplay(SelectorType type)
		{
			if (!displayObjects.ContainsKey(type.Type))
			{
				// Objekt muss noch erstellt werden
				IGameStateObserver o = getInstance(type) as IGameStateObserver;
				if (o == null)
				{
					MessageBox.Show($"Anzeige {type} konnte nicht erstellt werden.");
				}
				displayObjects[type.Type] = o;
			}
			// Objekt existiert jetzt sicher, registrieren
			theGame.AddObserver(displayObjects[type.Type]);
		}

		private void tryRemoveDisplay(SelectorType type)
		{
			IGameStateObserver obs;
			if (!displayObjects.TryGetValue(type.Type, out obs))
				return;

			theGame.RemoveObserver(obs);
		}

		// Extrahiert einen IMoveProvider aus der ComboBox-Auswahl
		private IMoveProvider getFromBox(ComboBox source)
		{
			var Type = source.SelectedItem as SelectorType;

			if (Type == null)
			{
				MessageBox.Show("Bitte Spieler auswählen.");
				return null;
			}

			IMoveProvider prov = getInstance(Type) as IMoveProvider;
			if (prov == null)
			{
				MessageBox.Show("Spieler konnte nicht erstellt werden.");
				return null;
			}

			return prov;
		}

		// Neues Spiel
		private void newGame_Click(object sender, RoutedEventArgs e)
		{
			// Altes Spiel terminieren
			if (theGame != null)
				theGame.Stop();

			var white = getFromBox(whiteBox);
			var black = getFromBox(blackBox);

			if (white == null || black == null)
				return;

			theGame = new Game(white, black, (int)delay.Value);
			moveBox.ItemsSource = theGame.Moves;

			foreach (SelectorType type in displayBox.SelectedItems)
			{
				tryAddDisplay(type);
			}

			theGame.Start();		
		}

		private void white_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (theGame != null)
				theGame.White = getFromBox(whiteBox);
		}

		private void black_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (theGame != null)
				theGame.Black = getFromBox(blackBox);
		}

		private void displayBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (theGame == null)
				return;

			foreach (var item in e.AddedItems.OfType<SelectorType>())
				tryAddDisplay(item);

			foreach (var item in e.RemovedItems.OfType<SelectorType>())
				tryRemoveDisplay(item);
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void loadAssembly_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				Filter = "Dynamic Link Libraries (*.dll)|*.dll|Executables (*.exe)|*.exe|Alle Dateien (*.*)|*.*"
			};

			var result = dialog.ShowDialog();
			if (result != true)
				return;

			try
			{
				var assembly = Assembly.LoadFrom(dialog.FileName);
				if (assembly == null)
					throw new Exception();
				populateLists(new[] { assembly });
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Assembly konnte nicht geladen werden: {ex.Message} ({ex.GetType().ToString()})");
			}
		}

		private void delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (theGame != null)
				theGame.Delay = (int)e.NewValue;
		}

		private void moveBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			theGame.RewindTo(moveBox.SelectedItem as GameMove);
		}
	}
}
