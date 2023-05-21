module Tests

open Expecto
open FsCheck
open System.IO

open TypedPersistence
open TypedPersistence.Core

let testFileDir = "test-files"

type BasicRecord<'a> = { data: 'a }

let testPropertyAsync name func =
    testProperty name (func >> Async.RunSynchronously)

/// Function to create a basic save and load test for a given type and provider with context
let genericSaveLoadTest<'a, 'b when 'a: equality>
    (provider: IPersistenceProvider<'b>)
    context
    =
    let typeName = (typeof<'a>).FullName

    let context = context typeName

    testPropertyAsync ("Save and load: " + typeName) (fun (value: 'a) ->
        async {
            do! provider.save context value
            let! content = provider.load<'a> context
            let valueLoaded = content.Value
            return valueLoaded = value
        })

let basicSaveLoadTests<'a when 'a: equality> () =
    // Test for JsonProvider
    let jsonProviderTest =
        let provider = Json.Api.fsharpProvider

        let context =
            fun typeName ->
                "SaveLoad" + typeName
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
              [ testAsync "Load nonexisting file" {
                    // TODO: use provider to support new provider
                    let! value = Json.Loading.load<string> "lalelu.txt"

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
              [ testPropertyAsync "Load version" (fun (version: Version) ->
                    async {
                        // TODO: use provider to support new provider
                        let filepath =
                            Path.Combine(testFileDir, "load_version.test.json")

                        do! Json.Saving.saveVersion filepath version "Hello World"
                        let! content = Json.Loading.getVersion filepath
                        let versionLoaded = content.Value
                        return versionLoaded = version
                    })
                testAsync "Load by version" {
                    let filepath = Path.Combine(testFileDir, "load_by_version.test.json")

                    let data = { data = "Hello World" }

                    do! Json.Saving.saveVersion filepath 3u data

                    let! version = Json.Loading.getVersion filepath

                    match version with
                    | Some 3u ->
                        let! content = Json.Loading.load<BasicRecord<string>> filepath

                        Expect.equal content.Value data "Data should be equal"
                    | Some _
                    | None -> failwith "Could not load correct version!"
                } ] ]
