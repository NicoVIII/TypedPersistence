namespace TypedPersistence.CouchbaseLite.FSharp

open Couchbase.Lite
open Microsoft.FSharp.Reflection
open System

// TODO: Think about wrapping Couchbase functions to use Option types

[<AutoOpen>]
module Saving =
    let rec saveDictionaryGeneric<'a  when 'a :> obj> saveType (dictionary: IMutableDictionary) (value: obj) =
        value.GetType()
        |> FSharpType.GetRecordFields
        |> List.ofArray
        |> List.iter (fun propertyInfo ->
            let name = propertyInfo.Name
            let typeObj = propertyInfo.PropertyType
            let value = propertyInfo.GetValue(value)
            saveType dictionary typeObj name value
        )
        ()

    let rec saveType (dictionary: IMutableDictionary) (typeObj: Type) (name: string) (value: obj) =
        match box value with
        | :? string as value ->
            // Prefix string to safely represent None as empty string
            dictionary.SetString(name, "-" + value) |> ignore
        | :? double as value ->
            dictionary.SetDouble(name, value) |> ignore
        | :? single as value ->
            dictionary.SetFloat(name, value) |> ignore
        | :? int as value ->
            dictionary.SetInt(name, value) |> ignore
        | :? int64 as value ->
            dictionary.SetLong(name, value) |> ignore
        | :? bool as value ->
            dictionary.SetBoolean(name, value) |> ignore
        | value when Helpers.isList typeObj ->
            let arrayObj = MutableArrayObject()
            value :?> seq<obj>
            |> List.ofSeq
            |> List.iter (fun v ->
                match box v with
                | :? string
                | :? double
                | :? single
                | :? int
                | :? int64
                | :? bool ->
                    arrayObj.AddValue(v) |> ignore
                // TODO: Add list
                | value when value.GetType() |> FSharpType.IsRecord ->
                    let newDict = MutableDictionaryObject ()
                    arrayObj.AddDictionary(newDict) |> ignore
                    saveDictionaryGeneric saveType newDict value
                | v ->
                    v.GetType().FullName
                    |> failwithf "Given type is not supported: %s"
            )
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
            dictionary.SetDictionary(name, newDict)
            |> saveDictionaryGeneric saveType <| value
            ()
        | _ ->
            typeObj.FullName
            |> failwithf "Given type is not supported: %s"

    let saveDictionary = saveDictionaryGeneric saveType

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
