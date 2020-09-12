namespace TypedPersistence.FSharp.Tests.Helper

open Expecto
open Expecto.Logging
open Expecto.Logging.Message
open LiteDB
open TypedPersistence.FSharp

[<AutoOpen>]
module Auto =
    let private config = FsCheckConfig.defaultConfig
    let testProp name = testPropertyWithConfig config name
    let ptestProp name = ptestPropertyWithConfig config name
    let ftestProp name = ftestPropertyWithConfig config name
    let etestProp stdgen name = etestPropertyWithConfig stdgen config name

[<AutoOpen>]
module Types =
    type SingleCaseUnion = OnlyCase

    type SingleCaseIntUnion = OnlyIntCase of int

    type NonGenericRecord1 =
        { value1: int
          value2: string }

    type NonGenericRecord2 =
        { value1: NonGenericRecord1 option
          value2: string }

    type GenericRecord<'a> =
        { value: 'a }

    type GenericRecordAlt<'a> =
        { value1: 'a }

    type GenericRecordAlt2<'a> =
        { value1: 'a }

    type GenericRecord2<'a, 'b> =
        { value1: 'a
          value2: 'b }

    type LoadErrorCategories =
        | OkCase
        | DocumentNotExistingErrorCase
        | ValueNotExistingErrorCase

[<AutoOpen>]
module Functions =
    let logger = Log.create "MyTests"

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

    let simplePropertyTest<'a when 'a: equality> dbName (data: 'a) =
        use db = openDatabase dbName
        cleanupDatabase db

        saveDocument db data |> ignore

        loadDocument<'a> db |> checkResultSuccess data
