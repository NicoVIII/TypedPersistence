namespace TypedPersistence.FSharp.Tests

open Expecto
open Expecto.Logging
open Expecto.Logging.Message
open LiteDB
open System.IO
open TypedPersistence.FSharp

[<AutoOpen>]
module Auto =
    let private config = FsCheckConfig.defaultConfig |> fun config -> { config with maxTest = 200 }
    let testProp name = testPropertyWithConfig config name
    let ptestProp name = ptestPropertyWithConfig config name
    let ftestProp name = ftestPropertyWithConfig config name
    let etestProp stdgen name = etestPropertyWithConfig stdgen config name

[<AutoOpen>]
module Functions =
    let logger = Log.create "MyTests"
    let dbName = "test.db"

    let wrapInRecord value = { GenericRecord.value = value }

    let generateDatabaseName data =
        data.GetType()
        |> hash
        |> string
        |> fun hash -> "test_" + hash + ".db"

    let openDatabase (name: string) = new LiteDatabase(name, FSharpBsonMapperWithGenerics())

    let cleanupDatabase (db: LiteDatabase) =
        db.GetCollectionNames()
        |> List.ofSeq
        |> List.iter (db.DropCollection >> ignore)

    let checkResultSuccess data result =
        match result with
        | Ok result ->
            if result = data then
                true
            else
                logger.error
                    (eventX "Expected: {data} - Got: {result}"
                     >> setField "data" data
                     >> setField "result" result)
                false
        | Core.Error error ->
            logger.error (eventX "Error: {error}" >> setField "error" error)
            false

    let simplePropertyTest<'a when 'a: equality> (data: 'a) =
        use db = openDatabase dbName
        cleanupDatabase db

        saveDocument db data |> ignore

        loadDocument<'a> db |> checkResultSuccess data
