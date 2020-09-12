namespace TypedPersistence

open System

[<AutoOpen>]
module Types =
    type VersionMap = Map<uint32, Type>
