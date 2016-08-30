/*
 * SingleInstanceAttribute.cs
 * Copyright (c) 2016 Markus Himmel
 * This file ist distributed under the terms of the MIT license
 */

using System;

namespace Morris
{
	/// <summary>
	/// Signalisiert, dass der Controller nur eine Instanz dieser Klasse erstellen sollte,
	/// wenn sie mehrfach angefordert ist (z.B. nur eine GUI, die Input nimmt und ausgibt)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class SingleInstanceAttribute : Attribute
	{
	}
}
