open Fake.IO

open RunHelpers
open RunHelpers.Shortcuts
open RunHelpers.Templates

[<RequireQualifiedAccess>]
module Config =
    let projects =
        [ "./src/TypedPersistence/Core/TypedPersistence.Core.fsproj"
          "./src/TypedPersistence/Json/TypedPersistence.Json.fsproj" ]

    let packDir = "./pack"

    let testProject =
        "./tests/TypedPersistence.UnitTests/TypedPersistence.UnitTests.fsproj"

module Task =
    let restore () =
        job {
            DotNet.toolRestore ()

            for project in Config.projects do
                DotNet.restore project

            DotNet.restore Config.testProject
        }

    let build config =
        job {
            for project in Config.projects do
                DotNet.build project config

            DotNet.build Config.testProject config
        }

    let pack version =
        job {
            for project in Config.projects do
                DotNet.pack Config.packDir project version
        }

    let test () = DotNet.run Config.testProject

    [<RequireQualifiedAccess>]
    module Docs =
        let build () =
            Shell.cp "./README.md" "docs/index.md"

            dotnet [ "fsdocs"; "build"; "--clean" ]

        let watch () =
            Shell.deleteDir "./tmp"

            Shell.cp "./README.md" "docs/index.md"

            dotnet [ "fsdocs"; "watch" ]

[<EntryPoint>]
let main args =
    args
    |> List.ofArray
    |> function
        | [ "restore" ] -> Task.restore ()
        | [ "build" ] ->
            job {
                Task.restore ()
                Task.build Debug
            }
        | [ "pack"; version ] ->
            job {
                Task.restore ()
                Task.build Release
                Task.pack version
            }
        | []
        | [ "test" ] ->
            job {
                Task.restore ()
                Task.test ()
            }
        | [ "build-docs" ] ->
            job {
                Task.restore ()
                Task.build Debug
                Task.Docs.build ()
            }
        | [ "docs" ] ->
            job {
                Task.restore ()
                Task.build Debug
                Task.Docs.watch ()
            }
        // Missing args cases
        | [ "pack" ] -> Job.error [ "Usage: dotnet run pack <version>" ]
        // Default error case
        | _ ->
            Job.error
                [ "Usage: dotnet run [<command>]"
                  "Look up available commands in run.fs" ]
    |> Job.execute
