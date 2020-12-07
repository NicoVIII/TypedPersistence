namespace TypedPersistence.Json

open FSharp.Json
open System
open System.IO
open TypedPersistence.Core

open TypedPersistence.Json.Types

[<AutoOpen>]
module Saving =
    let saveVersion<'a> (filePath: Context) (Version version) (data: 'a) =
        // Alias for writing text with single arguments and not with tuple
        let writeAllText filePath content =
            // Create path, if not existing
            filePath
            |> Path.GetDirectoryName
            |> Directory.CreateDirectory
            |> ignore
            // Write data
            File.WriteAllText(filePath, content)

        { version = version; data = data }
        |> Json.serialize
        |> (fun text -> text + Environment.NewLine) // Somehow this final new line is missing
        |> writeAllText filePath

    let save<'a> (filePath: Context) (data: 'a) = saveVersion filePath (Version 1u) data
