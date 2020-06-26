namespace TypedPersistence.FSharp

open LiteDB
open TypedPersistence.FSharp

open TypedPersistence.FSharp.Helpers

[<AutoOpen>]
module Loading =
    let loadDocumentWithId<'a> (database: LiteDatabase) (key: string) =
        let document = database.GetCollection<GenericEntry<'a>>().FindById(BsonValue(key))
        if isNull document then
            Error DocumentNotExisting
        else
            Ok document.entry

    let loadDocumentWithIdFromDatabase<'a> (path: string) (key: string) =
        let execute database = loadDocumentWithId<'a> database key
        executeWithDatabaseSetup execute path

    let loadDocument<'a> (database: LiteDatabase) =
        loadDocumentWithId<'a> database "default"

    let loadDocumentFromDatabase<'a> (path: string) =
        let execute database = loadDocument<'a> database
        executeWithDatabaseSetup execute path
