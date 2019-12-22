namespace TypedPersistence.FSharp

open LiteDB.FSharp
open System
open System.Security.Cryptography
open System.Text

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
            else
                resolveCollectionName.Invoke(t)

        do
            this.ResolveCollectionName <-
                Func<Type, string>(fun t ->
                    use md5 = MD5.Create()
                    let name =
                        if t.IsGenericType then
                            genericName t
                            |> Encoding.ASCII.GetBytes
                            |> md5.ComputeHash
                            |> Array.map (fun (x : byte) -> String.Format("{0:X2}", x))
                            |> String.concat String.Empty
                        else
                            resolveCollectionName.Invoke(t)
                    name)

    type GenericEntry<'T> =
        { id: string
          entry: 'T }

    type LoadError =
        | DatabaseNotExisting
        | DocumentNotExisting
