namespace TypedPersistence.FSharp

open LiteDB

open TypedPersistence.FSharp.Helpers

[<AutoOpen>]
module Saving =
    let saveDocumentWithId<'a> (database: LiteDatabase) (key: string) (document: 'a) =
        let collection = database.GetCollection<GenericEntry<'a>>()
        collection.Upsert
            ({ id = key
               entry = document })
        |> function
        | true -> Ok()
        | false -> Error()

    let saveDocumentWithIdToDatabase<'a> (path: string) (key: string) (document: 'a) =
        let execute database = saveDocumentWithId database key document
        executeWithDatabaseSetup execute path

    let saveDocument<'a> database document =
       saveDocumentWithId<'a> database "default" document

    let saveDocumentToDatabase<'a> path (document: 'a) =
       let execute database = saveDocument database document
       executeWithDatabaseSetup execute path
