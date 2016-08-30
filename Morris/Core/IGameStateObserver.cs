/*
 * IGameStateObserver.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Eine Entität, die ein Spiel "abbonieren" kann und dann über Änderungen
	/// des Spielzustands in Kenntnis gesetzt wird
	/// </summary>
	public interface IGameStateObserver
	{
		/// <summary>
		/// Wird aufgerufen, wenn sich der aktuelle Spielzustand geändert hat
		/// </summary>
		/// <param name="state">Lesesicht auf den aktuellen Spielzustand</param>
		void Notify(IReadOnlyGameState state);
	}
}
