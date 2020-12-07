module Tests

open Expecto
open FsCheck
open System.IO

open TypedPersistence
open TypedPersistence.Core

let testFileDir = "test-files"

type BasicRecord<'a> = { data: 'a }

/// Function to create a basic save and load test for a given type and provider with context
let genericSaveLoadTest<'a, 'b when 'a: equality> (provider: IPersistenceProvider<'b>)
                                                  context
                                                  =
    let typeName = (typeof<'a>).FullName

    let context = context typeName

    testProperty ("Save and load: " + typeName) (fun (value: 'a) ->
        provider.save context value
        let valueLoaded = (provider.load<'a> context).Value
        valueLoaded = value)

let basicSaveLoadTests<'a when 'a: equality> () =
    // Test for JsonProvider
    let jsonProviderTest =
        let provider = Json.Api.fsharpProvider

        let context =
            fun typeName ->
                "SaveLoad"
                + typeName
                |> System.Text.Encoding.ASCII.GetBytes
                |> System.Security.Cryptography.MD5.Create().ComputeHash
                |> Seq.map (fun c -> c.ToString("X2"))
                |> Seq.reduce (+)
                |> (fun name -> Path.Combine(testFileDir, name + ".test.json"))

        genericSaveLoadTest<'a, string> provider context

    [ jsonProviderTest ]

/// Test list which is used by Expecto
let tests =
    testList
        "Test List"
        [ testList
            "Misc"
              [ test "Load nonexisting file" {
                    // TODO: use provider to support new provider
                    let value = Json.Loading.load<string> "lalelu.txt"

                    Expect.equal value None "Load on non-existing file should return None"
                } ]
          testList
              "Save and load"
              ([ basicSaveLoadTests<NonNull<string>> ()
                 basicSaveLoadTests<int> ()
                 basicSaveLoadTests<NonNull<string> list> ()
                 basicSaveLoadTests<int list> ()
                 basicSaveLoadTests<BasicRecord<NonNull<string>>> ()
                 basicSaveLoadTests<BasicRecord<int>> ()
                 basicSaveLoadTests<BasicRecord<NonNull<string> list>> ()
                 basicSaveLoadTests<BasicRecord<int list>> ()
                 basicSaveLoadTests<BasicRecord<BasicRecord<int>>> () ]
               |> List.concat)
          testList
              "Versioning"
              [ testProperty "Load version" (fun (version: Version) ->
                    // TODO: use provider to support new provider
                    let filepath =
                        Path.Combine(testFileDir, "load_version.test.json")

                    Json.Saving.saveVersion filepath version "Hello World"
                    let versionLoaded = (Json.Loading.getVersion filepath).Value
                    versionLoaded = version)
                test "Load by version" {
                    let filepath =
                        Path.Combine(testFileDir, "load_by_version.test.json")

                    let data = { data = "Hello World" }

                    Json.Saving.saveVersion filepath (Version 3u) data

                    let value =
                        match Json.Loading.getVersion filepath with
                        | Some (Version 3u) ->
                            (Json.Loading.load<BasicRecord<string>> filepath).Value
                        | Some _
                        | None -> failwith "Could not load correct version!"

                    Expect.equal value data "Data should be equal"
                } ] ]
