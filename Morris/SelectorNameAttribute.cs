/*
 * SelectorNameAttribute.cs
 * Copyright (c) 2016 Makrus Himmel
 * This file is distributed un der the terms of the MIT license
 */

using System;

namespace Morris
{
	/// <summary>
	/// Ein Attribut, welches angibt, wie die Klasse im Auswahldialog benannt werden soll
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
