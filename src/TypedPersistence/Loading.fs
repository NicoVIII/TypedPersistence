namespace TypedPersistence

open FSharp.Json
open System.IO

open TypedPersistence.Helper

[<AutoOpen>]
module Loading =
    let private getFileContent filepath =
        if File.Exists filepath then File.ReadAllText filepath |> Some else None

    let getVersion (filepath: string) =
        opt {
            let! content = getFileContent filepath
            let! parsed = deserializeJson<OnlyVersion> content
            return parsed.version
        }

    let load<'a> (filepath: string) =
        opt {
            let! content = getFileContent filepath
            let! parsed = deserializeJson<VersionAndData<'a>> content
            return parsed.data
        }
