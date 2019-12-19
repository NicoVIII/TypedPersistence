module TypedPersistence.FSharp.Tests.SavingLoading

open Expecto
open FsCheck
open TypedPersistence.FSharp
open System.IO

[<Tests>]
let tests = testList "Saving and loading tests" [
    testCase "Fails to load unexisting document" <| fun _ ->
        let dbName = generateDatabaseName ()
        use db = openDatabase dbName

        loadDocument<int> db
        |> Expect.equal <| Error DocumentNotExisting <| "Should throw error!"

    testProp "Handles ints correctly" <| simplePropertyTest<int>

    testProp "Handles double saving correctly" <| fun (number: int) ->
        genericPropertyTest<int> (fun db data -> saveDocument db data |> ignore) number

    testProp "Handles strings correctly" <| fun (text: NonNull<string>) ->
        text.Get
        |> simplePropertyTest<string>

    testProp "Handles floats correctly" <| fun (number: NormalFloat) ->
        number.Get
        |> simplePropertyTest<float>

    testProp "Handles option ints correctly" <| simplePropertyTest<int option>

    testProp "Handles option strings correctly"
        <| fun (textOption: Option<NonNull<string>>) ->
            let text =
                match textOption with
                | Some text -> Some text.Get
                | None -> None
            simplePropertyTest<string option> text

    testProp "Handles int list correctly" <| simplePropertyTest<int list>

    testProp "Handles string list correctly"
        <| fun (textList: NonNull<string> list) ->
            textList
            |> List.map (fun x -> x.Get)
            |> simplePropertyTest<string list>

    testProp "Handles int record correctly"
        <| simplePropertyTest<GenericRecord<int>>

    testProp "Handles string record correctly"
        <| fun (textRecord: GenericRecord<NonNull<string>>) ->
            { value = textRecord.value.Get }
            |> simplePropertyTest<GenericRecord<string>>

    testProp "Handles int option, string record2 correctly"
        <| fun (record: GenericRecord2<int option, NonNull<string>>) ->
            let record =
                { value1 = record.value1
                  value2 = record.value2.Get }

            simplePropertyTest<GenericRecord2<int option, string>> record

    testProp "Handles int record list correctly"
        <| simplePropertyTest<GenericRecord<int> list>

    testProp "Handles int list record correctly"
        <| simplePropertyTest<GenericRecord<int list>>

    testProp "Handles int list record record correctly"
        <| simplePropertyTest<GenericRecord<GenericRecord<int list>>>

    testProp "Handles saving with mapping with id correctly"
        <| fun (number: int) ->
            let dbName = generateDatabaseName number
            use db = openDatabase dbName

            saveDocumentWithMapping<int, int> id db number |> ignore

            loadDocument<int> db
            |> (fun x -> db.Dispose(); File.Delete(dbName); x)
            |> checkResultSuccess number

    testProp "Handles loading with mapping with id correctly"
        <| fun (number: int) ->
            let dbName = generateDatabaseName number
            use db = openDatabase dbName

            saveDocument<int> db number |> ignore

            loadDocumentWithMapping<int, int> id db
            |> (fun x -> db.Dispose(); File.Delete(dbName); x)
            |> checkResultSuccess number


    testProp "Handles saving and loading with mapping with id correctly"
        <| fun (number: int) ->
            let dbName = generateDatabaseName number
            use db = openDatabase dbName

            saveDocumentWithMapping<int, int> id db number |> ignore

            loadDocumentWithMapping<int, int> id db
            |> (fun x -> db.Dispose(); File.Delete(dbName); x)
            |> checkResultSuccess number


    testProp "Handles saving and loading with mapping with wrapping correctly"
        <| fun (number: int) ->
            let dbName = generateDatabaseName number
            use db = openDatabase dbName

            saveDocumentWithMapping<GenericRecord<int>, int> wrapInRecord db number
            |> ignore

            loadDocumentWithMapping<GenericRecord<int>, int> (fun x -> x.value) db
            |> (fun x -> db.Dispose(); File.Delete(dbName); x)
            |> checkResultSuccess number


    testProp "Handles saving and loading with mapping with +-1 correctly"
        <| fun (number: int) ->
            let dbName = generateDatabaseName number
            use db = openDatabase dbName

            saveDocumentWithMapping<GenericRecord<int>, int> ((+) 1 >> wrapInRecord) db number |> ignore

            loadDocumentWithMapping<GenericRecord<int>, int> ((fun x -> x.value) >> (+) -1) db
            |> checkResultSuccess number
]
