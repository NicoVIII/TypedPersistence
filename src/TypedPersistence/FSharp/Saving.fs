namespace TypedPersistence.FSharp

open LiteDB

// TODO: Think about wrapping Couchbase functions to use Option types
[<AutoOpen>]
module Saving =
    let saveDocument<'T> (database: LiteDatabase) (document: 'T) =
        let collection = database.GetCollection<GenericEntry<'T>>()
        collection.Upsert({ id = "default"; entry = document})
        |> function
        | true ->
            Ok ()
        | false ->
            Error ()

    let saveDocumentWithMapping<'TPersistence, 'T> (mapping: 'T -> 'TPersistence) (database: LiteDatabase) (record: 'T) =
        mapping record
        |> saveDocument database
