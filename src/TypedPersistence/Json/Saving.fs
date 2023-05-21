namespace TypedPersistence.Json

open FSharp.Json
open System
open System.IO
open TypedPersistence.Core

open TypedPersistence.Json.Types

[<AutoOpen>]
module Saving =
    let saveVersion<'a> (filePath: Context) (version: Version) (data: 'a) =
        // Alias for writing text with single arguments and not with tuple
        let writeAllText (filePath: string) content =
            async {
                // Create path, if not existing
                filePath |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore
                // Write data
                do! File.WriteAllTextAsync(filePath, content) |> Async.AwaitTask
            }

        { version = version; data = data }
        |> Json.serialize
        |> (fun text -> text + Environment.NewLine) // Somehow this final new line is missing
        |> writeAllText filePath

    let save<'a> (filePath: Context) (data: 'a) = saveVersion filePath 1u data
