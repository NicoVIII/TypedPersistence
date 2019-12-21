module TypedPersistence.FSharp.Tests.Process

open Expecto
open TypedPersistence.FSharp
open TypedPersistence.FSharp.Tests.Helper

let dbName = "test_process.db"

[<Tests>]
let tests =
    testSequenced <| testList "Process tests"
                         [ testProp "Handles double saving correctly" <| fun (number: int) ->
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
                               |> (&&) prevRes ]
