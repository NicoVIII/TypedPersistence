namespace TypedPersistence.FSharp.Tests

open Expecto
open Expecto.Logging
open Expecto.Logging.Message
open LiteDB
open System.IO
open TypedPersistence.FSharp

[<AutoOpen>]
module Auto =
    let private config = FsCheckConfig.defaultConfig |> fun config -> { config with maxTest = 500 }
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

    let genericPropertyTest<'a when 'a: equality> setUp (data: 'a) =
        use db = openDatabase dbName

        setUp db data

        saveDocument db data |> ignore

        loadDocument<'a> db
        |> (fun a ->
            db.Dispose()
            File.Delete(dbName)
            a)
        |> checkResultSuccess data

    let simplePropertyTest<'a when 'a: equality> (data: 'a) = genericPropertyTest<'a> (fun _ _ -> ()) data
