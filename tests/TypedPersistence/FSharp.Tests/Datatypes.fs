module TypedPersistence.FSharp.Tests.Datatypes

open Expecto
open FsCheck
open TypedPersistence.FSharp
open TypedPersistence.FSharp.Loading
open TypedPersistence.FSharp.Saving
open TypedPersistence.FSharp.Tests.Helper

let dbName = "test_datatypes.db"

[<Tests>]
let tests =
    testSequenced
    <| testList "Datatype tests"
           [ testProp "Handles ints correctly"
             <| simplePropertyTest<int> dbName

             testProp "Handles strings correctly"
             <| fun (text: NonNull<string>) ->
                 text.Get |> simplePropertyTest<string> dbName

             testProp "Handles floats correctly"
             <| fun (number: NormalFloat) ->
                 number.Get |> simplePropertyTest<float> dbName

             testProp "Handles option ints correctly"
             <| simplePropertyTest<int option> dbName

             testProp "Handles option strings correctly"
             <| fun (textOption: Option<NonNull<string>>) ->
                 let text =
                     match textOption with
                     | Some text -> Some text.Get
                     | None -> None

                 simplePropertyTest<string option> dbName text

             testProp "Handles int list correctly"
             <| simplePropertyTest<int list> dbName

             testProp "Handles string list correctly"
             <| fun (textList: NonNull<string> list) ->
                 textList
                 |> List.map (fun x -> x.Get)
                 |> simplePropertyTest<string list> dbName

             testProp "Handles int record correctly"
             <| simplePropertyTest<GenericRecord<int>> dbName

             testProp "Handles string record correctly"
             <| fun (textRecord: GenericRecord<NonNull<string>>) ->
                 { value = textRecord.value.Get }
                 |> simplePropertyTest<GenericRecord<string>> dbName

             testProp "Handles int option, string record2 correctly"
             <| fun (record: GenericRecord2<int option, NonNull<string>>) ->
                 let record =
                     { value1 = record.value1
                       value2 = record.value2.Get }

                 simplePropertyTest<GenericRecord2<int option, string>> dbName record

             testProp "Handles int record list correctly"
             <| simplePropertyTest<GenericRecord<int> list> dbName

             testProp "Handles int list record correctly"
             <| simplePropertyTest<GenericRecord<int list>> dbName

             testProp "Handles int list record record correctly"
             <| simplePropertyTest<GenericRecord<GenericRecord<int list>>> dbName

             testProp "Handles single case union type correctly"
             <| simplePropertyTest<SingleCaseUnion> dbName

             testProp "Handles single case int union type correctly"
             <| simplePropertyTest<SingleCaseIntUnion> dbName

             testProp "Handles single case int union type list correctly"
             <| simplePropertyTest<SingleCaseIntUnion list> dbName

             testProp "Handles single case int union type list option correctly"
             <| simplePropertyTest<SingleCaseIntUnion option list> dbName

             testProp "Handles non generic record correctly"
             <| simplePropertyTest<NonGenericRecord2> dbName

             testProp "Handles complex record correctly"
             <| simplePropertyTest<GenericRecord2<GenericRecord<int list>, GenericRecord<GenericRecord<SingleCaseIntUnion>> list>>
                 dbName

             testProp "Handles fallback types correctly"
             <| (fun (data: GenericRecordAlt<int>) ->
                 use db = openDatabase dbName
                 cleanupDatabase db

                 saveDocument db data |> ignore

                 setFallbackFor<GenericRecordAlt<int>> db
                 |> loadDocument<GenericRecordAlt2<int>>
                 |> Result.map (fun x -> x.value1)
                 |> checkResultSuccess data.value1) ]
