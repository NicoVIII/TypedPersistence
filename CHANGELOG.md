# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.6.0] - 2024-03-09

### Added
- JSON support
- Unified Provider classes for F# and C#
- Version SCU for Savefileversion

### Changed
- Project structure (Core + module for JSON)
- PackageIDs (leading Owner)
- Loading and Saving are async now
- Require .NET 6

### Removed
- LiteDB support (for now)

## [0.2.0] - 2019-12-19

### Changed
- Use LiteDB instead of CouchbaseLite

## [0.1.1] - 2019-11-22

### Changed
- Workflow has now FAKE and Paket

## [0.1.0] - 2019-10-07

### Added
- Basic functionality


[Unreleased]: https://github.com/NicoVIII/TypedPersistence/compare/v0.6.0...HEAD
[0.6.0]: https://github.com/NicoVIII/TypedPersistence/compare/v0.5.4..v0.6.0
[0.2.0]: https://github.com/NicoVIII/TypedPersistence/compare/v0.1.1..v0.2.0
[0.1.1]: https://github.com/NicoVIII/TypedPersistence/compare/v0.1.0..v0.1.1
[0.1.0]: https://github.com/NicoVIII/TypedPersistence/commits/v0.1.0/
