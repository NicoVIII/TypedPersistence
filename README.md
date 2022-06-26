# TypedPersistence

[![Last commit](https://img.shields.io/github/last-commit/NicoVIII/TypedPersistence?style=flat-square)](https://github.com/NicoVIII/TypedPersistence/commits)
[![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE.txt)

[![Nuget (Core)](https://img.shields.io/nuget/v/NicoVIII.TypedPersistence.Core.svg?logo=nuget&label=Core&style=flat-square)](https://www.nuget.org/packages/NicoVIII.TypedPersistence.Core)
[![Nuget (Json)](https://img.shields.io/nuget/v/NicoVIII.TypedPersistence.Json.svg?logo=nuget&label=Json&style=flat-square)](https://www.nuget.org/packages/NicoVIII.TypedPersistence.Json)

This project aims at providing a typesafe way to load and safe from and to a persistent local database.
It should be possible to define records as definition for the structure of the data and load and save different versions of data.

**Disclaimer**  
I'm not sure where I want to go with this. Current implementation(s) are using reflection which isn't very nice. But without a convenient way for metaprogramming and code generation on compile time (Myraid sadly adds unnecessary dependencies to a project which can cause problems), I couldn't find a better way...
Also I had a look at the Avro format recently and thought about providing partial reading and appending operations but this is completely different to the current approach. So yeah, much to figure out here.
