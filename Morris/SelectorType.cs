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
