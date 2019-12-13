namespace TypedPersistence.CouchbaseLite.FSharp.Tests

open Couchbase.Lite
open TypedPersistence.CouchbaseLite.FSharp

[<AutoOpen>]
module Functions =
    let openDatabase() = new Database("testdb")

    let cleanUpDatabase() =
        use db = openDatabase ()
        db.Delete ()

    let saveToDatabase<'a> documentName (data: 'a) =
        use db = openDatabase()
        saveDocument db documentName data
        db.Close()

    let loadFromDatabase<'a> documentName =
        use db = openDatabase()
        let result = loadDocument<'a> db documentName
        db.Close()
        result

    let saveToDatabaseWithMapping<'a, 'b> documentName mapping (data: 'b) =
        use db = openDatabase()
        saveDocumentWithMapping<'a, 'b> mapping db documentName data
        db.Close()

    let loadFromDatabaseWithMapping<'a, 'b> documentName mapping =
        use db = openDatabase()
        let result = loadDocumentWithMapping<'a, 'b> mapping db documentName
        db.Close()
        result

    let categorize result =
        match result with
        | Ok _ -> OkCase
        | Error error ->
            match error with
            | DocumentNotExisting _ -> DocumentNotExistingErrorCase
            | ValueNotExisting _ -> ValueNotExistingErrorCase

    let wrapInRecord data =
        { GenericRecord.value = data }

    let checkResultSuccess data result =
        match result with
        | Ok record ->
            if record.value = data then
                true
            else
                printfn "Expected: %A - Got: %A" data record.value
                false
        | Error error ->
            printfn "Error: %A" error
            false

    let genericPropertyTest<'a when 'a: equality> documentName setUp (data: 'a) =
        let dataRecord = wrapInRecord data

        cleanUpDatabase()

        setUp dataRecord

        saveToDatabase documentName dataRecord

        loadFromDatabase<GenericRecord<'a>> documentName
        |> checkResultSuccess data

    let simplePropertyTest<'a when 'a: equality> documentName (data: 'a) = genericPropertyTest<'a> documentName ignore data
