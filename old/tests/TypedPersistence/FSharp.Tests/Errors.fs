module TypedPersistence.FSharp.Tests.SavingLoading

open Expecto
open FsCheck
open TypedPersistence.FSharp
open TypedPersistence.FSharp.Tests.Helper

let dbName = "test_errors.db"

[<Tests>]
let tests =
    testSequenced <| testList "Saving and loading tests"
                         [ testCase "Fails to load unexisting document" <| fun _ ->
                             use db = openDatabase dbName
                             cleanupDatabase db

                             loadDocument<int> db
                             |> Expect.equal
                             <| Error DocumentNotExisting
                             <| "Should throw error!" ]
