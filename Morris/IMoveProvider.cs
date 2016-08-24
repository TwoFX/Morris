/*
 * IMoveProvider.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Eine Entität, welche in der Lage ist, basierend auf dem
	/// aktuellen Spielzustand einen Spielzug bereitzustellen
	/// (also in der Regel entweder eine Benutzeroberfläche oder
	/// ein Bot).
	/// </summary>
	interface IMoveProvider
	{
		/// <summary>
		/// Bestimmt den nächsten Spielzug
		/// </summary>
		/// <param name="state">Lesesicht auf den aktuellen Spielzustand</param>
		/// <returns>Ein Spielzug</returns>
		GameMove GetNextMove(IReadOnlyGameState state);
	}
}
