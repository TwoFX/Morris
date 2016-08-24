/*
 * IReadOnlyGameState.cs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Morris
{
	/// <summary>
	/// Eine schreibgeschützte Sicht auf ein Spielfeld, anhand der ein <see cref="IMoveProvider"/>
	/// einen Nachfolgezug bestimmen soll
	/// </summary>
	public interface IReadOnlyGameState
	{
		// Properties, die Auskunft über die Spielsituation geben

		/// <summary>
		/// Belegungsinformationen zum Spielfeld
		/// </summary>
		ReadOnlyCollection<Occupation> Board { get; }

		/// <summary>
		/// Der Spieler, der am Zug ist
		/// </summary>
		Player NextToMove { get; }

		/// <summary>
		/// Ob das Spiel noch läuft oder wie das Spiel ausgegangen ist
		/// </summary>
		GameResult Result { get; }


		// Methoden, die Auskunft über die Spielsituation geben

		/// <summary>
		/// Gibt die Phase, in der sich ein Spieler befindet, zurück
		/// </summary>
		/// <param name="player">Der Spieler, dessen Phase gesucht ist</param>
		/// <returns>Eine Phase</returns>
		Phase GetPhase(Player player);


		// Methoden zur Vereinfachung der Arbeit von IMoveProvider

		/// <summary>
		/// Bestimmt, ob ein Zug in der aktuellen Spielsituation gültig ist
		/// </summary>
		/// <param name="move">Der Zug, der überprüft werden soll</param>
		/// <returns>
		/// <para><see cref="MoveValidity.Valid"/>, wenn der Zug gültig ist.</para>
		/// <para><see cref="MoveValidity.ClosesMill"/>, wenn der Zug gültig ist, aber eine Mühle schließt, und kein zu entfernender Stein angegeben wurde.</para>
		/// <para><see cref="MoveValidity.DoesNotCloseMill"/>, wenn der Zug gültig ist, aber ein zu entfernender Stein angegeben wurde, obwohl der Zug keine Mühle schließt.</para>
		/// <para><see cref="MoveValidity.Invalid"/>, wenn der Zug ungültig ist.</para>
		/// </returns
		MoveValidity IsValidMove(GameMove move);
	}
}
