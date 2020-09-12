namespace TypedPersistence

open FSharp.Json
open System.IO

[<AutoOpen>]
module Loading =
    let getVersion (filepath: string) =
        let content = File.ReadAllText filepath
        try
            Json.deserialize<OnlyVersion> content
            |> (fun v -> v.version)
            |> Some
        with :? JsonDeserializationError -> None

    let load<'a> (filepath: string) =
        let content = File.ReadAllText filepath
        try
            Json.deserialize<VersionAndData<'a>> content
            |> (fun d -> d.data)
            |> Some
        with :? JsonDeserializationError -> None
