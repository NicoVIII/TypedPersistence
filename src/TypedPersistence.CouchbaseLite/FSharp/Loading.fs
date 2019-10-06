namespace TypedPersistence.CouchbaseLite.FSharp

open Couchbase.Lite
open Microsoft.FSharp.Reflection
open System

// TODO: Think about wrapping Couchbase functions to use Option types
[<AutoOpen>]
module Loading =
    let private (|PrimitiveType|OptionType|ListType|RecordType|UnsupportedType|) typeObj =
        match typeObj with
        | typeObj when
            typeObj = typeof<string>
            || typeObj = typeof<double>
            || typeObj = typeof<single>
            || typeObj = typeof<int>
            || typeObj = typeof<int64>
            || typeObj = typeof<bool> ->
                PrimitiveType
        | typeObj when Helpers.isOption typeObj ->
            OptionType
        | typeObj when Helpers.isList typeObj ->
            ListType
        | typeObj when FSharpType.IsRecord typeObj ->
            RecordType
        | _ ->
            UnsupportedType

    let private handleValue<'a when 'a : null> onOk typeObj name (value:'a) =
        match value with
        | null ->
            ValueNotExisting name |> Error
        | value ->
            onOk typeObj value

    let private handlePrimitiveValue =
        let onOk typeObj (value:obj) =
            let value =
                match value with
                // Special case int: Is read as int64 for some reason
                | :? int64 as value when typeObj = typeof<int> ->
                    value
                    |> int
                    |> fun x -> x :> obj
                // Special case string: Is saved with leading '-'
                | :? string as value when typeObj = typeof<string> ->
                    value
                    |> String.substring 1
                    |> fun x -> x :> obj
                | _ ->
                    value
            Ok value
        handleValue onOk

    let rec loadType loadDictionary (dictionary:IDictionaryObject) (typeObj: Type) (name: string) =
        let loadType = loadType loadDictionary
        match typeObj with
        | PrimitiveType ->
            dictionary.GetValue(name)
            |> handlePrimitiveValue typeObj name
        | ListType ->
            let subType = typeObj.GetGenericArguments().[0]
            dictionary.GetArray(name)
            |> handleValue (fun subType value ->
                value
                |> Helpers.convertFromArrayObject (fun index array ->
                    match subType with
                    | PrimitiveType ->
                        array.GetValue index
                        |> handlePrimitiveValue subType name
                    | RecordType ->
                        array.GetDictionary index
                        |> handleValue loadDictionary subType name
                    | UnsupportedType ->
                        failwithf "Given type is not supported in lists: %s" subType.FullName
                )
                |> Result.map (Helpers.CachingReflectiveListBuilder.BuildTypedList subType)
            ) subType name
        | OptionType ->
            let subType = typeObj.GetGenericArguments().[0]
            if dictionary.GetString name = "" then
                Helpers.makeOptionValue subType () false |> Ok
            else
                let value = loadType dictionary subType  name
                match value with
                | Error error ->
                    error |> Error
                | Ok value ->
                    Helpers.makeOptionValue subType value true |> Ok
        | RecordType ->
            dictionary.GetDictionary(name)
            |> handleValue loadDictionary typeObj name
        | UnsupportedType ->
            failwithf "Given type is not supported: %s" typeObj.FullName

    let rec private loadDictionary (typeObj: Type) (dictionary: IDictionaryObject): Result<obj,LoadError> =
        typeObj
        |> FSharpType.GetRecordFields
        |> List.ofArray
        |> List.traverseResultA (fun propertyInfo ->
            let name = propertyInfo.Name
            let propertyType = propertyInfo.PropertyType
            loadType loadDictionary dictionary propertyType name
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
            let typeObj = typeof<'T>
            match typeObj with
            | RecordType ->
                loadDictionary typeObj doc
                |> Result.map (fun x -> x :?> 'T)
            | _ ->
                failwithf "Given type is no record: %s" typeObj.FullName

    let loadDocumentWithMapping<'TPersistence, 'T> (mapping: 'TPersistence -> 'T) (database: Database) (documentName: string) =
        loadDocument<'TPersistence> database documentName
        |> Result.map mapping
