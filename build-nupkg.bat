@echo off

rem FSharp.Control.AsyncPrimitives - F# Async synchronization primitives library.
rem Copyright (c) 2016 Kouji Matsui (@kekyo2)
rem 
rem Licensed under the Apache License, Version 2.0 (the "License");
rem you may not use this file except in compliance with the License.
rem You may obtain a copy of the License at
rem 
rem http://www.apache.org/licenses/LICENSE-2.0
rem 
rem Unless required by applicable law or agreed to in writing, software
rem distributed under the License is distributed on an "AS IS" BASIS,
rem WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
rem See the License for the specific language governing permissions and
rem limitations under the License.

.nuget\nuget pack FSharp.Control.AsyncPrimitives.FS20.nuspec -Prop Version=0.5.1 -Prop Configuration=Release
.nuget\nuget pack FSharp.Control.AsyncPrimitives.FS30.nuspec -Prop Version=0.5.1 -Prop Configuration=Release
.nuget\nuget pack FSharp.Control.AsyncPrimitives.FS31.nuspec -Prop Version=0.5.1 -Prop Configuration=Release
.nuget\nuget pack FSharp.Control.AsyncPrimitives.FS40.nuspec -Prop Version=0.5.1 -Prop Configuration=Release
