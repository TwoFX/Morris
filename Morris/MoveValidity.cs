/*
 * MoveValidity.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Gibt an, ob ein Spielzug gültig ist
	/// </summary>
	public enum MoveValidity
	{
		Valid,
		Invalid,
		ClosesMill,
		DoesNotCloseMill
	}
}
