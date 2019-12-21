namespace TypedPersistence.FSharp

open LiteDB.FSharp
open System

[<AutoOpen>]
module Types =
    type FSharpBsonMapperWithGenerics() as this =
        inherit FSharpBsonMapper()
        let resolveCollectionName = this.ResolveCollectionName

        let rec genericName (t: Type) =
            if t.IsGenericType then
                t.GetGenericArguments()
                |> List.ofArray
                |> List.map genericName
                |> List.reduce (fun a b -> a + "," + b)
                |> (+) (t.Name + "-")
                |> hash
                |> string
            else
                resolveCollectionName.Invoke(t)

        do
            this.ResolveCollectionName <-
                Func<Type, string>(fun t ->
                    if t.IsGenericType then
                        genericName t
                    else
                        resolveCollectionName.Invoke(t))

    type GenericEntry<'T> =
        { id: string
          entry: 'T }

    type LoadError =
        | DatabaseNotExisting
        | DocumentNotExisting
