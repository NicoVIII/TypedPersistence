module Tests

open Expecto
open FsCheck
open System.IO

open TypedPersistence

let testFileDir = "test-files"

type BasicRecord<'a> = { data: 'a }

/// Function to create a basic save and load test for a given type
let basicSaveLoadTest<'a when 'a: equality> () =
    let typeName = (typeof<'a>).FullName

    let filepath =
        "SaveLoad"
        + typeName
        |> System.Text.Encoding.ASCII.GetBytes
        |> System.Security.Cryptography.MD5.Create().ComputeHash
        |> Seq.map (fun c -> c.ToString("X2"))
        |> Seq.reduce (+)
        |> (fun name -> Path.Combine(testFileDir, name + ".test.json"))

    testProperty ("Save and load: " + typeName) (fun (value: 'a) ->
        save filepath value
        let valueLoaded = (load<'a> filepath).Value
        valueLoaded = value)

/// Test list which is used by Expecto
let tests =
    testList
        "Test List"
        [ testList
            "Misc"
              [ test "Load nonexisting file" {
                    let value = load<string> "lalelu.txt"
                    Expect.equal value None "Load on non-existing file should return None"
                } ]
          testList
              "Save and load"
              [ basicSaveLoadTest<NonNull<string>> ()
                basicSaveLoadTest<int> ()
                basicSaveLoadTest<NonNull<string> list> ()
                basicSaveLoadTest<int list> ()
                basicSaveLoadTest<BasicRecord<NonNull<string>>> ()
                basicSaveLoadTest<BasicRecord<int>> ()
                basicSaveLoadTest<BasicRecord<NonNull<string> list>> ()
                basicSaveLoadTest<BasicRecord<int list>> ()
                basicSaveLoadTest<BasicRecord<BasicRecord<int>>> () ]
          testList
              "Versioning"
              [ testProperty "Load version" (fun (version: uint) ->
                    let filepath =
                        Path.Combine(testFileDir, "load_version.test.json")

                    saveVersion filepath version "Hello World"
                    let versionLoaded = (getVersion filepath).Value
                    versionLoaded = version)
                test "Load by version" {
                    let filepath =
                        Path.Combine(testFileDir, "load_by_version.test.json")

                    let data = { data = "Hello World" }

                    saveVersion filepath 3u data

                    let value =
                        match getVersion filepath with
                        | Some 3u -> (load<BasicRecord<string>> filepath).Value
                        | Some _
                        | None -> failwith "Could not load correct version!"

                    Expect.equal value data "Data should be equal"
                } ] ]
