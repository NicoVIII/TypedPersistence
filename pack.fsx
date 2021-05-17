#!/bin/dotnet fsi
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.Core.ReleaseNotes"

open Fake.Core
open Fake.DotNet

let project = "TypedPersistence"
let submodules = [ "Core"; "Json" ]

let summary =
  "Simple library which should simplify saving and loading files"

let authors = "NicoVIII"
let tags = "Persistence,Typesafety,F#"
let copyright = ""
let license = "MIT"

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

let pack submodule =
  let properties =
    [ ("Version", nugetVersion)
      ("Authors", authors)
      ("PackageProjectUrl", gitUrl)
      ("PackageTags", tags)
      ("RepositoryType", "git")
      ("RepositoryUrl", gitUrl)
      ("PackageLicenseExpression", license)
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
    $"src/{project}/{submodule}/{project}.{submodule}.fsproj"

let packAll () = List.iter pack submodules

packAll ()
