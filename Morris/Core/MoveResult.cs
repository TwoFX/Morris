/*
 * MoveResult.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

namespace Morris
{
	/// <summary>
	/// Resultate eines Zuges, die für den <see cref="IMoveProvider"/> relevant sind.
	/// Insbesondere gibt MoveResult keine Information darüber, ob das Spiel durch
	/// aktuellen Zug beendet wurde, da diese Informationen durch einen geeigneten
	/// <see cref="IGameStateObserver"/> an den Benutzer weitergegeben werden soll. 
	/// </summary>
	public enum MoveResult
	{
		GameNotRunning,
		InvalidMove,
		OK
	}
}
