(*
 * FSharpRandomBot.fs
 * Copyright (c) 2016 Markus Himmel
 * This file is distributed under the terms of the MIT license
 *)

namespace ExternalBotTest

open Morris

[<SelectorName("F# KI")>]
type FSharpRandomBot() =

    let rng = System.Random ()
    let chooseRandom n = Seq.item (Seq.length n |> rng.Next) n

    interface IMoveProvider with
        // Funktioniert exakt genauso wie das C#-Pendant
        member this.GetNextMove state =
            let chosen = state.BasicMoves () |> chooseRandom

            match state.IsValidMove chosen with
            | MoveValidity.ClosesMill ->
                [0..GameState.FIELD_SIZE - 1]
                |> Seq.where (fun d -> int state.Board.[d] = int (state.NextToMove.Opponent()))
                |> chooseRandom
                |> chosen.WithRemove
            | _ -> chosen 
