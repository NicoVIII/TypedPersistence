namespace TypedPersistence.CouchbaseLite.FSharp

[<AutoOpen>]
module Types =
    type LoadError =
        | ValueNotExisting of string
        | DocumentNotExisting of string
