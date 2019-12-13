namespace TypedPersistence.FSharp.Tests

open LiteDB
open System.IO
open TypedPersistence.FSharp

[<AutoOpen>]
module Functions =
    let dbName = "testdb"
    let openDatabase() = new LiteDatabase(dbName)

    let cleanUpDatabase() =
        File.Delete(dbName + ".db")

    let saveToDatabase<'a> (data: 'a) =
        use db = openDatabase()
        saveDocument<'a> db data

    let loadFromDatabase<'a> () =
        use db = openDatabase()
        loadDocument<'a> db

    let saveToDatabaseWithMapping<'a, 'b> mapping (data: 'b) =
        use db = openDatabase()
        saveDocumentWithMapping<'a, 'b> mapping db data

    let loadFromDatabaseWithMapping<'a, 'b> mapping =
        use db = openDatabase()
        loadDocumentWithMapping<'a, 'b> mapping db

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

    let genericPropertyTest<'a when 'a: equality> setUp (data: 'a) =
        let dataRecord = wrapInRecord data

        cleanUpDatabase()

        setUp dataRecord

        saveToDatabase dataRecord |> ignore

        loadFromDatabase<GenericRecord<'a>> ()
        |> checkResultSuccess data

    let simplePropertyTest<'a when 'a: equality> (data: 'a) = genericPropertyTest<'a> ignore data
