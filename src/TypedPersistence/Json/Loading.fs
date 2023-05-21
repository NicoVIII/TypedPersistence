namespace TypedPersistence.Json

open FsToolkit.ErrorHandling
open System.IO

open TypedPersistence.Json.Helper
open TypedPersistence.Json.Types

[<AutoOpen>]
module Loading =
    let private getFileContent filepath =
        async {
            if File.Exists filepath then
                let! content = File.ReadAllTextAsync filepath |> Async.AwaitTask
                return content |> Some
            else
                return None
        }

    let getVersion (filepath: string) =
        asyncOption {
            let! content = getFileContent filepath
            let! parsed = deserializeJson<OnlyVersion> content
            return parsed.version
        }

    let load<'a> (filepath: string) =
        asyncOption {
            let! content = getFileContent filepath
            let! parsed = deserializeJson<VersionAndData<'a>> content
            return parsed.data
        }
