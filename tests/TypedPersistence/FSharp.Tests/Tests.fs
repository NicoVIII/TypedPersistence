module TypedPersistence.FSharp.Tests.SavingLoading

open Expecto
open FsCheck
open TypedPersistence.FSharp
open System.IO

[<Tests>]
let tests =
    testSequenced
    <| testList "Saving and loading tests"
           [ testCase "Fails to load unexisting document" <| fun _ ->
               use db = openDatabase dbName
               cleanupDatabase db

               loadDocument<int> db
               |> Expect.equal
               <| Error DocumentNotExisting
               <| "Should throw error!"

             testProp "Handles ints correctly" <| simplePropertyTest<int>

             testProp "Handles double saving correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocument<int> db number |> ignore

                 saveDocument<int> db number |> ignore

                 loadDocument<int> db |> checkResultSuccess number

             testProp "Handles double loading correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocument<int> db number |> ignore

                 loadDocument<int> db |> ignore

                 loadDocument<int> db |> checkResultSuccess number

             testProp "Handles double saving & loading correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocument<int> db number |> ignore

                 saveDocument<int> db number |> ignore

                 loadDocument<int> db |> ignore

                 loadDocument<int> db |> checkResultSuccess number

             testProp "Handles multiple savings & loadings correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocument<int> db number |> ignore

                 let prevRes = loadDocument<int> db |> checkResultSuccess number

                 saveDocument<int> db number |> ignore

                 loadDocument<int> db
                 |> checkResultSuccess number
                 |> (&&) prevRes

             testProp "Handles strings correctly"
             <| fun (text: NonNull<string>) -> text.Get |> simplePropertyTest<string>

             testProp "Handles floats correctly" <| fun (number: NormalFloat) -> number.Get |> simplePropertyTest<float>

             testProp "Handles option ints correctly" <| simplePropertyTest<int option>

             testProp "Handles option strings correctly" <| fun (textOption: Option<NonNull<string>>) ->
                 let text =
                     match textOption with
                     | Some text -> Some text.Get
                     | None -> None
                 simplePropertyTest<string option> text

             testProp "Handles int list correctly" <| simplePropertyTest<int list>

             testProp "Handles string list correctly" <| fun (textList: NonNull<string> list) ->
                 textList
                 |> List.map (fun x -> x.Get)
                 |> simplePropertyTest<string list>

             testProp "Handles int record correctly" <| simplePropertyTest<GenericRecord<int>>

             testProp "Handles string record correctly"
             <| fun (textRecord: GenericRecord<NonNull<string>>) ->
                 { value = textRecord.value.Get } |> simplePropertyTest<GenericRecord<string>>

             testProp "Handles int option, string record2 correctly" <| fun (record: GenericRecord2<int option, NonNull<string>>) ->
                 let record =
                     { value1 = record.value1
                       value2 = record.value2.Get }

                 simplePropertyTest<GenericRecord2<int option, string>> record

             testProp "Handles int record list correctly" <| simplePropertyTest<GenericRecord<int> list>

             testProp "Handles int list record correctly" <| simplePropertyTest<GenericRecord<int list>>

             testProp "Handles int list record record correctly"
             <| simplePropertyTest<GenericRecord<GenericRecord<int list>>>

             testProp "Handles single case union type correctly" <| simplePropertyTest<SingleCaseUnion>

             testProp "Handles single case int union type correctly" <| simplePropertyTest<SingleCaseIntUnion>

             testProp "Handles single case int union type list correctly" <| simplePropertyTest<SingleCaseIntUnion list>

             testProp "Handles single case int union type list option correctly"
             <| simplePropertyTest<SingleCaseIntUnion option list>

             testProp "Handles complex record correctly"
             <| simplePropertyTest<GenericRecord2<GenericRecord<int list>, GenericRecord<GenericRecord<SingleCaseIntUnion>> list>>

             testProp "Handles saving with mapping with id correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocumentWithMapping<int, int> id db number |> ignore

                 loadDocument<int> db |> checkResultSuccess number

             testProp "Handles loading with mapping with id correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocument<int> db number |> ignore

                 loadDocumentWithMapping<int, int> id db |> checkResultSuccess number


             testProp "Handles saving and loading with mapping with id correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocumentWithMapping<int, int> id db number |> ignore

                 loadDocumentWithMapping<int, int> id db |> checkResultSuccess number


             testProp "Handles saving and loading with mapping with wrapping correctly" <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocumentWithMapping<GenericRecord<int>, int> wrapInRecord db number |> ignore

                 loadDocumentWithMapping<GenericRecord<int>, int> (fun x -> x.value) db |> checkResultSuccess number


             testProp "Handles saving and loading with mapping with +-1 correctly"
             <| fun (number: int) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocumentWithMapping<GenericRecord<int>, int> ((+) 1 >> wrapInRecord) db number |> ignore

                 loadDocumentWithMapping<GenericRecord<int>, int> ((fun x -> x.value) >> (+) -1) db
                 |> checkResultSuccess number ]
