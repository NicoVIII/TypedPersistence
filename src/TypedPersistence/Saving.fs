namespace TypedPersistence

open FSharp.Json
open System
open System.IO

[<AutoOpen>]
module Saving =
    let saveVersion (filePath: string) (version: uint) (data: 'a) =
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

    let save (filePath: string) (data: 'a) = saveVersion filePath 1u data
