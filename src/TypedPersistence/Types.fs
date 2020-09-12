namespace TypedPersistence

open System

[<AutoOpen>]
module Types =
    type OnlyVersion = { version: uint32 }
    type VersionAndData<'a> = { data: 'a; version: uint32 }
