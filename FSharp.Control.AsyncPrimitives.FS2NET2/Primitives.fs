/////////////////////////////////////////////////////////////////////////////////////////////////
//
// FSharp.Control.AsyncPrimitives - F# Async synchronization primitives library.
// Copyright (c) 2016 Kouji Matsui (@kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.FSharp.Control

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading

///////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Pseudo lock primitive on F#'s async workflow.
/// </summary>
[<Sealed; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncLock () =

  let _queue = new Queue<unit -> unit>()
  let mutable _enter = false

  let locker continuation =
    let result =
      lock (_queue) (fun _ ->
        match _enter with
        | true ->
          _queue.Enqueue(continuation)
          false
        | false ->
          _enter <- true
          true)
    match result with
    | true -> continuation()
    | false -> ()

  let unlocker () =
    let result =
      lock (_queue) (fun _ ->
        match _queue.Count with
        | 0 ->
          _enter <- false
          None
        | _ ->
          Some (_queue.Dequeue()))
    match result with
    | Some continuation -> continuation()
    | None -> ()

  let disposable = {
    new IDisposable with
      member __.Dispose() = unlocker()
  }

  member __.AsyncLock(token: CancellationToken) =
    let acs = new AsyncCompletionSource<IDisposable>()
    token.Register ((fun _ -> acs.SetCanceled()), null) |> ignore
    locker (fun _ -> acs.SetResult disposable)
    acs.Async

  member __.AsyncLock() =
    let acs = new AsyncCompletionSource<IDisposable>()
    locker (fun _ -> acs.SetResult disposable)
    acs.Async
