namespace TypedPersistence.FSharp

[<AutoOpen>]
module Types =
    type GenericEntry<'T> =
        {
            id: string
            entry: 'T
        }

    type LoadError =
    | DocumentNotExisting
