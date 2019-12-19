namespace TypedPersistence.FSharp

open LiteDB
open LiteDB.FSharp.Extensions
open TypedPersistence.FSharp

[<AutoOpen>]
module Loading =
    let loadDocument<'T> (database: LiteDatabase) =
        database.GetCollection<GenericEntry<'T>>().TryFindById(BsonValue("default"))
        |> function
        | Some document ->
            Ok document.entry
        | None ->
            Error DocumentNotExisting

    let loadDocumentWithMapping<'TPersistence, 'T> (mapping: 'TPersistence -> 'T) (database: LiteDatabase) =
        loadDocument<'TPersistence> database
        |> Result.map mapping
