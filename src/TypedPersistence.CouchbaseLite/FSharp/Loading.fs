namespace TypedPersistence.CouchbaseLite.FSharp

open Couchbase.Lite
open Microsoft.FSharp.Reflection
open System

// TODO: Think about wrapping Couchbase functions to use Option types
[<AutoOpen>]
module Loading =
    // TODO: save options not with null :/ it's ambivalent (non existing, saved null)
    let rec loadDictionary (typeObj: Type) (dictionary: IDictionaryObject): Result<obj,LoadError> =
        let rec loadType (typeObj: Type) (name: string) =
            match typeObj with
            | typeObj when
                typeObj = typeof<double>
                || typeObj = typeof<single>
                || typeObj = typeof<int>
                || typeObj = typeof<int64>
                || typeObj = typeof<bool> ->
                    dictionary.GetValue(name)
                    |> function
                    | null ->
                        ValueNotExisting name |> Error
                    | value ->
                        // Special case int: Is read as int64 for some reason
                        if typeObj = typeof<int> then
                            value :?> int64
                            |> int
                            |> fun x -> x :> obj
                        else
                            value
                        |> Ok
            | typeObj when typeObj = typeof<string> ->
                dictionary.GetString(name)
                |> function
                    | null ->
                        ValueNotExisting name |> Error
                    | value ->
                        value
                        |> String.substring 1
                        |> fun x -> x :> obj
                        |> Ok
            | typeObj when Helpers.isList typeObj ->
                let subType = typeObj.GetGenericArguments().[0]
                dictionary.GetArray(name)
                |> function
                | null -> ValueNotExisting name |> Error
                | value ->
                    value
                    |> Helpers.convertFromArrayObject (fun index array ->
                        match subType with
                        | subType when
                            subType = typeof<string>
                            || subType = typeof<double>
                            || subType = typeof<single>
                            || subType = typeof<int>
                            || subType = typeof<bool> ->
                                array.GetValue index |> Ok
                        | subType when
                            subType = typeof<int64> ->
                                array.GetLong index :> obj |> Ok
                        | subType when FSharpType.IsRecord subType ->
                            array.GetDictionary index
                            |> loadDictionary subType
                    )
                    |> Result.map (Helpers.CachingReflectiveListBuilder.BuildTypedList subType)
            | typeObj when Helpers.isOption typeObj ->
                let subType = typeObj.GetGenericArguments().[0]
                if dictionary.GetString name = "" then
                    Helpers.makeOptionValue subType () false |> Ok
                else
                    let value = loadType subType name
                    match value with
                    | Error error ->
                        error |> Error
                    | Ok value ->
                        Helpers.makeOptionValue subType value true |> Ok
            | typeObj when FSharpType.IsRecord typeObj ->
                // TODO: optimize recursion away (it's no tail recursion)
                dictionary.GetDictionary(name)
                |> function
                    | null ->
                        ValueNotExisting name |> Error
                    | dictionary ->
                        loadDictionary typeObj dictionary
            | typeObj ->
                failwithf "Given type is not supported: %s" typeObj.FullName

        typeObj
        |> FSharpType.GetRecordFields
        |> List.ofArray
        |> List.traverseResultA (fun propertyInfo ->
            let name = propertyInfo.Name
            let propertyType = propertyInfo.PropertyType
            loadType propertyType name
        )
        |> Result.map List.toArray
        |> function
            | Ok values ->
                FSharpValue.MakeRecord(typeObj, values) |> Ok
            | Error error ->
                error |> Error

    let loadDocument<'T> (database: Database) (documentName: string) =
        use doc = database.GetDocument(documentName)

        match doc with
        | null ->
            DocumentNotExisting documentName |> Error
        | doc ->
            match typeof<'T> with
            | typeObj when FSharpType.IsRecord typeObj ->
                loadDictionary typeObj doc
                |> Result.map (fun x -> x :?> 'T)
            | typeObj ->
                failwithf "Given type is no record: %s" typeObj.FullName

    let loadDocumentWithMapping<'TPersistence, 'T> (mapping: 'TPersistence -> 'T) (database: Database) (documentName: string) =
        loadDocument<'TPersistence> database documentName
        |> Result.map mapping
