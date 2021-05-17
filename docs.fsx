#!/bin/dotnet fsi

#r "nuget: Fake.Core.Process"
#r "nuget: Fake.DotNet.Cli"

open Fake.Core
open Fake.DotNet

let buildDir = "./build/"

let isNullOrWhiteSpace = System.String.IsNullOrWhiteSpace

let exec cmd args dir =
  let proc =
    CreateProcess.fromRawCommandLine cmd args
    |> CreateProcess.ensureExitCodeWithMessage (sprintf "Error while running '%s' with args: %s" cmd args)

  (if isNullOrWhiteSpace dir then
     proc
   else
     proc |> CreateProcess.withWorkingDirectory dir)
  |> Proc.run
  |> ignore

let build () =
  DotNet.build
    (fun p ->
      { p with
          Configuration = DotNet.BuildConfiguration.Release
          OutputPath = Some buildDir })
    "TypedPersistence.sln"

let docs () = exec "dotnet" @"fornax build" "docs"

build ()
docs ()
