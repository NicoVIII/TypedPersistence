module TypedPersistence.FSharp.Tests.Mapping

open Expecto
open FsCheck
open TypedPersistence.FSharp
open TypedPersistence.FSharp.Tests.Helper

let dbName = "test_mapping.db"

[<Tests>]
let tests =
    testSequenced
    <| testList "Mapping tests"
           [ testProp "Handles saving with mapping with id correctly" <| fun (number: int) ->
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
