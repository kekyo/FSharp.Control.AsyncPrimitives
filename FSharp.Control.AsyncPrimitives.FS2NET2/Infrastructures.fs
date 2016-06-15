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
/// Delegation F#'s async continuation.
/// </summary>
/// <description>
/// Simulate TaskCompletionSource&lt;'T&gt; for F#'s Async&lt;'T&gt;.
/// </description>
/// <typeparam name="'T">Computation result type</typeparam> 
[<Sealed; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncCompletionSource<'T> =

  [<DefaultValue>]
  val mutable private _value : 'T option
  [<DefaultValue>]
  val mutable private _exn : exn option
  [<DefaultValue>]
  val mutable private _ocexn : OperationCanceledException option

  [<DefaultValue>]
  val mutable private _completed : ('T -> unit) option
  [<DefaultValue>]
  val mutable private _caught : (exn -> unit) option
  [<DefaultValue>]
  val mutable private _canceled : (OperationCanceledException -> unit) option

  val private _async : Async<'T>

  /// <summary>
  /// Constructor.
  /// </summary>
  new () as this = {
    _async = Async.FromContinuations<'T>(fun (completed, caught, canceled) ->
      lock (this) (fun _ ->
        match this._value, this._ocexn, this._exn with
        | Some value, _, _ -> completed value
        | _, Some ocexn, _ -> canceled ocexn
        | _, _, Some exn -> caught exn
        | _, _, _ -> ()

        this._completed <- Some completed
        this._caught <- Some caught
        this._canceled <- Some canceled))
  }

  /// <summary>
  /// Target Async&lt;'T&gt; instance.
  /// </summary>
  member this.Async = this._async

  /// <summary>
  /// Set result value and continue continuation.
  /// </summary>
  /// <param name="value">Result value</param>
  member this.SetResult value =
    lock (this) (fun () ->
      match this._completed with
      | Some completed -> completed value
      | None -> this._value <- Some value)

  /// <summary>
  /// Set exception and continue continuation.
  /// </summary>
  /// <param name="exn">Exception instance</param>
  member this.SetException exn =
    lock (this) (fun () ->
      match this._caught with
      | Some caught -> caught exn
      | None -> this._exn <- Some exn)

  /// <summary>
  /// Set canceled and continue continuation.
  /// </summary>
  member this.SetCanceled (?ocexn : OperationCanceledException) =
    let ocexn =
      match ocexn with
      | Some ocexn -> ocexn
      | None ->
        try
          OperationCanceledException() |> raise
        with :? OperationCanceledException as exn ->
          exn
    lock (this) (fun () ->
      match this._canceled with
      | Some canceled -> canceled ocexn
      | None -> this._ocexn <- Some ocexn)
