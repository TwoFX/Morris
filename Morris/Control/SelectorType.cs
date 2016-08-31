/*
 * SelectorType.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license.
 */

using System;
using System.Reflection;

namespace Morris
{
	/// <summary>
	/// Hält einen Typen fest, der potentiell durch ein <see cref="SelectorNameAttribute"/> einen neuen Namen erhalten hat 
	/// Objekte dieses Typs können dann in eine ListBox oder ComboBox eingefügt werden. Diese rufen .ToString() auf, und
	/// zeigen so den gewünschten Namen an. Wir erhalten dieses Objekt dann zurück von ListBox/ComboBox durch SelectedItem
	/// und können dann über die Type-Eigenschaft erfahren, welche KI oder Anzeige wir instanziieren sollen.
	/// </summary>
	internal class SelectorType
	{
		public Type Type
		{
			get;
			private set;
		}

		private string displayName;

		public SelectorType(Type type)
		{
			Type = type;

			// displayName ist SelectorName, falls ein SelectorNameAttribute existiert
			// und ansonsten einfach der Typname
			displayName = Type
				.GetCustomAttribute<SelectorNameAttribute>()
				?.SelectorName
				?? type.ToString();
		
		}

		public override string ToString()
		{
			return displayName;
		}
	}
}
