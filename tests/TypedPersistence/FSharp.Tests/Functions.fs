namespace TypedPersistence.FSharp.Tests

open Expecto
open LiteDB
open System.IO
open TypedPersistence.FSharp

[<AutoOpen>]
module Auto =
    let private config =
        FsCheckConfig.defaultConfig
        |> fun config -> { config with maxTest = 100 }
    let testProp name = testPropertyWithConfig config name
    let ptestProp name = ptestPropertyWithConfig config name
    let ftestProp name = ftestPropertyWithConfig config name
    let etestProp stdgen name = etestPropertyWithConfig stdgen config name

[<AutoOpen>]
module Functions =
    let wrapInRecord value = { GenericRecord.value = value }

    let generateDatabaseName data = data |> hash |> string |> fun name -> "test_" + name + ".db"
    let openDatabase (name: string) = new LiteDatabase(name, FSharpBsonMapperWithGenerics())

    let checkResultSuccess data result =
        match result with
        | Ok result ->
            if result = data then
                true
            else
                printfn "Expected: %A - Got: %A" data result
                false
        | Error error ->
            printfn "Error: %A" error
            false

    let genericPropertyTest<'a when 'a: equality> setUp (data: 'a) =
        let dbName = generateDatabaseName data
        use db = openDatabase dbName

        setUp db data

        saveDocument db data |> ignore

        loadDocument<'a> db
        |> (fun a -> db.Dispose(); File.Delete(dbName); a)
        |> checkResultSuccess data

    let simplePropertyTest<'a when 'a: equality> (data: 'a) = genericPropertyTest<'a> (fun _ _ -> ()) data
