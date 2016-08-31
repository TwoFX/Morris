/*
 * SelectorNameAttribute.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed un der the terms of the MIT license
 */

using System;

namespace Morris
{
	/// <summary>
	/// Ein Attribut, welches angibt, wie die Klasse im Auswahldialog benannt werden soll.
	/// Man kann Anzeigen und KIs damit dekorieren, damit nicht der Klassenname (z.B. Morris.AI.NegamaxAI)
	/// im Selektor steht, sondern etwas lesefreundlicheres wie Einfacher Negamax
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class SelectorNameAttribute : Attribute
	{
		public string SelectorName
		{
			get;
			private set;
		}
	
		public SelectorNameAttribute(string selectorName)
		{
			SelectorName = selectorName;
		}
	}
}
