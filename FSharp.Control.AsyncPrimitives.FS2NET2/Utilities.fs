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
/// Asynchronos lazy instance generator.
/// </summary>
/// <typeparam name="'T">Computation result type</typeparam> 
[<Sealed; NoEquality; NoComparison; AutoSerializable(false)>]
type AsyncLazy<'T> =

  val private _lock : AsyncLock
  val private _asyncBody : unit -> Async<'T>
  val mutable private _value : 'T option

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="asyncBody">Lazy instance factory.</param>
  new (asyncBody: unit -> Async<'T>) = {
    _lock = new AsyncLock()
    _asyncBody = asyncBody
    _value = None
  }
  
  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="async">Lazy instance factory.</param>
  new (async: Async<'T>) = {
    _lock = new AsyncLock()
    _asyncBody = fun _ -> async
    _value = None
  }

  member this.AsyncGetValue() = async {
    use! al = this._lock.AsyncLock()
    match this._value with
    | Some value -> return value
    | None ->
      let! value = this._asyncBody()
      this._value <- Some value
      return value
  }
