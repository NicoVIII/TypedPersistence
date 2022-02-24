# TypedPersistence

[![Last commit](https://img.shields.io/github/last-commit/NicoVIII/TypedPersistence?style=flat-square)](https://github.com/NicoVIII/TypedPersistence/commits)
[![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE.txt)

[![Nuget (Core)](https://img.shields.io/nuget/v/NicoVIII.TypedPersistence.Core.svg?logo=nuget&label=Core&style=flat-square)](https://www.nuget.org/packages/NicoVIII.TypedPersistence.Core)
[![Nuget (Json)](https://img.shields.io/nuget/v/NicoVIII.TypedPersistence.Json.svg?logo=nuget&label=Json&style=flat-square)](https://www.nuget.org/packages/NicoVIII.TypedPersistence.Json)

This project aims at providing a typesafe way to load and safe from and to a persistent local database.
It should be possible to define records as definition for the structure of the data and load and save different versions of data.

## Development

### How to build application

1. Make sure you've installed .Net Core version defined in [global.json](global.json)
2. Run `dotnet tool restore` to install all developer tools required to build the project
3. Run `dotnet fake build` to build default target of [build script](build.fsx)
4. To run tests use `dotnet fake build -t Test`
5. To build documentation use `dotnet fake build -t Docs`

### How to work with documentation

1. Make sure you've installed .Net Core version defined in [global.json](global.json)
2. Run `dotnet tool restore` to install all developer tools required to build the project
3. Run `dotnet fake build` to build default target of [build script](build.fsx)
4. Build documentation to make sure everything is fine with `dotnet fake build -t Docs`
5. Go to docs folder `cd docs` and start Fornax in watch mode `dotnet fornax watch`
6. You documentation should be now accessible on `localhost:8080` and will be regenerated on every file save

### How to release

#### Releasing as part of the CI

1. Update [CHANGELOG.md](./CHANGELOG.md) by adding new entry (`## [X.Y.Z]`) and commit it.
2. Create version tag (`git tag vX.Y.Z`)
3. Run `dotnet fake build -t Pack` to create the nuget package and test/examine it locally.
4. Push the tag to the repo `git push origin vX.Y.Z` - this will start CI process that will create GitHub release and put generated NuGet packages in it
5. Upload generated packages into NuGet.org

#### Releasing from local machine

In case you don't want to create releases automatically as part of the CI process, we provide also set of helper targets in `build.fsx` script.
Create release.cmd or release.sh file (already git-ignored) with following content (sample from `cmd`, but `sh` file should be similar):

```
@echo off
cls

SET nuget-key=YOUR_NUGET_KEY
SET github-user=YOUR_GH_USERNAME
SET github-pw=YOUR_GH_PASSWORD_OR_ACCESS_TOKEN

dotnet fake build --target Release
```
