# TypedPersistence
[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)
[![GitHub Release](https://img.shields.io/github/release/NicoVIII/TypedPersistence.svg)](https://github.com/NicoVIII/TypedPersistence/releases/latest)
[![Github Pre-Release](https://img.shields.io/github/release/NicoVIII/TypedPersistence/all.svg?label=prerelease)](https://github.com/NicoVIII/TypedPersistence/releases)
[![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/NicoVIII/TypedPersistence/master/LICENSE.txt)

This project aims at providing a typesafe way to load and safe from and to a persistent local database.
It should be possible to define records as definition for the structure of the data and load and save different versions of data.

## Development
[![Build Status](https://github.com/NicoVIII/TypedPersistence/workflows/Continuous%20Integration/badge.svg)](https://github.com/NicoVIII/TypedPersistence/actions)

## How to build application

1. Make sure you've installed .Net Core version defined in [global.json](global.json)
2. Run `dotnet tool restore` to install all developer tools required to build the project
3. Run `dotnet fake build` to build default target of [build script](build.fsx)
4. To run tests use `dotnet fake build -t Test`
5. To build documentation use `dotnet fake build -t Docs`

## How to work with documentation

1. Make sure you've installed .Net Core version defined in [global.json](global.json)
2. Run `dotnet tool restore` to install all developer tools required to build the project
3. Run `dotnet fake build` to build default target of [build script](build.fsx)
4. Build documentation to make sure everything is fine with `dotnet fake build -t Docs`
5. Go to docs folder `cd docs` and start Fornax in watch mode `dotnet fornax watch`
6. You documentation should be now accessible on `localhost:8080` and will be regenerated on every file save

## How to release

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

## How to contribute

*Imposter syndrome disclaimer*: I want your help. No really, I do.

There might be a little voice inside that tells you you're not ready; that you need to do one more tutorial, or learn another framework, or write a few more blog posts before you can help me with this project.

I assure you, that's not the case.

This project has some clear Contribution Guidelines and expectations that you can [read here](CONTRIBUTING.md).

The contribution guidelines outline the process that you'll need to follow to get a patch merged. By making expectations and process explicit, I hope it will make it easier for you to contribute.

And you don't just have to write code. You can help out by writing documentation, tests, or even by giving feedback about this work. (And yes, that includes giving feedback about the contribution guidelines.)

Thank you for contributing!


## Contributing and copyright

The project is hosted on [GitHub](https://github.com/NicoVIII/TypedPersistence) where you can report issues, fork
the project and submit pull requests.

The library is available under [MIT license](LICENSE.md), which allows modification and redistribution for both commercial and non-commercial purposes.

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

## Versioning

I will try to stick to Semantic Versioning 2.0.0 (<http://semver.org/spec/v2.0.0.html>).

## Used Tools

I write the code in "Visual Studio Code" (<https://code.visualstudio.com/>).
