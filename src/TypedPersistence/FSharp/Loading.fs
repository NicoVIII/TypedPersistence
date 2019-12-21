namespace TypedPersistence.FSharp

open LiteDB
open LiteDB.FSharp.Extensions
open TypedPersistence.FSharp

open TypedPersistence.FSharp.Helpers

[<AutoOpen>]
module Loading =
    let loadDocumentWithId<'a> (database: LiteDatabase) (key: string) =
        database.GetCollection<GenericEntry<'a>>().TryFindById(BsonValue(key))
        |> function
        | Some document -> Ok document.entry
        | None -> Error DocumentNotExisting

    let loadDocumentWithIdFromDatabase<'a> (path: string) (key: string) =
        let execute database = loadDocumentWithId database key
        executeWithDatabaseSetup execute path

    let loadDocument<'a> (database: LiteDatabase) =
        loadDocumentWithId<'a> database "default"

    let loadDocumentFromDatabase<'a> (path: string) =
        let execute database = loadDocument database
        executeWithDatabaseSetup execute path
