namespace TypedPersistence.CouchbaseLite.FSharp

open Couchbase.Lite
open Microsoft.FSharp.Reflection
open System
open System.Collections

// TODO: Think about wrapping Couchbase functions to use Option types

[<AutoOpen>]
module Saving =
    let private preparePrimitiveValue value =
        match box value with
        // Prefix string to safely represent None as empty string
        | :? string as value ->
            "-" + value :> obj
        | _ ->
            value

    let rec saveType saveDictionary (dictionary: IMutableDictionary) (typeObj: Type) (name: string) (value: obj) =
        let saveType = saveType saveDictionary
        match box value with
        | :? string
        | :? double
        | :? single
        | :? int
        | :? int64
        | :? bool as value ->
            // TODO: Think about using ValueObject to ensure Value was prepared
            dictionary.SetValue(name, preparePrimitiveValue value) |> ignore
        | :? IEnumerable as valueList ->
            let arrayObj = MutableArrayObject()
            for value in valueList do
                match box value with
                | :? string
                | :? double
                | :? single
                | :? int
                | :? int64
                | :? bool ->
                    arrayObj.AddValue(preparePrimitiveValue value) |> ignore
                | value when value.GetType() |> FSharpType.IsRecord ->
                    let newDict = MutableDictionaryObject ()
                    arrayObj.AddDictionary(newDict) |> ignore
                    saveDictionary newDict value
                | v ->
                    v.GetType().FullName
                    |> failwithf "Given type is not supported: %s"
            dictionary.SetArray (name, arrayObj) |> ignore
        | _ when Helpers.isOption typeObj ->
            let (unionCaseInfo, objArray) = FSharpValue.GetUnionFields (value, typeObj, false)
            match unionCaseInfo.Name with
            | "None" ->
                dictionary.SetString (name, "") |> ignore
            | "Some" ->
                let subType = typeObj.GetGenericArguments().[0]
                saveType dictionary subType name (objArray.[0])
            | _ ->
                failwith "Something went wrong, Option was neither Some nor None"
        | _ when FSharpType.IsRecord typeObj ->
            let newDict = MutableDictionaryObject ()
            saveDictionary newDict value
            dictionary.SetDictionary (name, newDict) |> ignore
            ()
        | _ ->
            typeObj.FullName
            |> failwithf "Given type is not supported: %s"

    let rec saveDictionary (dictionary: IMutableDictionary) (value: obj) =
        value.GetType()
        |> FSharpType.GetRecordFields
        |> List.ofArray
        |> List.iter (fun propertyInfo ->
            let name = propertyInfo.Name
            let typeObj = propertyInfo.PropertyType
            let value = propertyInfo.GetValue(value)
            saveType saveDictionary dictionary typeObj name value
        )
        ()

    let saveDocument (database: Database) (documentName: string) (record: obj) =
        use doc =
            match database.GetDocument documentName with
            | null -> new MutableDocument(documentName)
            | x -> x.ToMutable ()

        match record.GetType() with
        | typeObj when FSharpType.IsRecord typeObj ->
            saveDictionary doc record
            database.Save doc
        | typeObj ->
            failwithf "Given type is no record: %s" typeObj.FullName

    let saveDocumentWithMapping<'TPersistence, 'T> (mapping: 'T -> 'TPersistence) (database: Database) (documentName: string) (record: 'T) =
        mapping record
        |> saveDocument database documentName
