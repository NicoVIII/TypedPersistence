#!/bin/dotnet fsi
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.Core.ReleaseNotes"

open Fake.Core
open Fake.DotNet
open System.IO

let project = "TypedPersistence"

let summary =
  "Simple library which should simplify saving and loading files"

let authors = "NicoVIII"
let tags = "Persistence,Typesafety,F#"
let copyright = ""

let gitOwner = "NicoVIII"
let gitName = project
let gitHome = "https://github.com/" + gitOwner
let gitUrl = gitHome + "/" + gitName

let nugetDir = "./out/"

let changelogFilename = "CHANGELOG.md"
let changelog = Changelog.load changelogFilename
let latestEntry = changelog.LatestEntry

let nugetVersion = latestEntry.NuGetVersion

let packageReleaseNotes =
  sprintf "%s/blob/v%s/CHANGELOG.md" gitUrl latestEntry.NuGetVersion

let pack () =
  let properties =
    [ ("Version", nugetVersion)
      ("Authors", authors)
      ("PackageProjectUrl", gitUrl)
      ("PackageTags", tags)
      ("RepositoryType", "git")
      ("RepositoryUrl", gitUrl)
      ("PackageLicenseUrl", gitUrl + "/LICENSE")
      ("Copyright", copyright)
      ("PackageReleaseNotes", packageReleaseNotes)
      ("PackageDescription", summary)
      ("EnableSourceLink", "true") ]

  DotNet.pack
    (fun p ->
      { p with
          Configuration = DotNet.BuildConfiguration.Release
          OutputPath = Some nugetDir
          MSBuildParams =
            { p.MSBuildParams with
                Properties = properties } })
    "TypedPersistence.sln"

let removeTests () =
  Directory.GetFiles nugetDir
  |> List.ofArray
  |> List.filter (fun name -> name.Contains "UnitTests")
  |> List.iter File.Delete

pack ()
removeTests ()
