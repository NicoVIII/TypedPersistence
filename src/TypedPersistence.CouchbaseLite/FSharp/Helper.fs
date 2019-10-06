namespace TypedPersistence.CouchbaseLite.FSharp

open Couchbase.Lite
open FSharp.Reflection
open System

module Helpers =
    // Taken from: http://www.fssnip.net/1L/title/Convert-a-obj-list-to-a-typed-list-without-generics
    type ReflectiveListBuilder =
        static member BuildList<'a> (args: obj list) =
            [ for a in args do yield a :?> 'a ]
        static member BuildTypedList lType (args: obj list) =
            typeof<ReflectiveListBuilder>
                .GetMethod("BuildList")
                .MakeGenericMethod([|lType|])
                .Invoke(null, [|args|])

    type CachingReflectiveListBuilder =
        static member ReturnTypedListBuilder<'a> () : obj list -> obj =
            let createList (args : obj list) = [ for a in args do yield a :?> 'a ] :> obj
            createList
        static member private builderMap = ref Map.empty<string, obj list -> obj>
        static member BuildTypedList (lType: System.Type) =
            let currentMap = !CachingReflectiveListBuilder.builderMap
            if Map.containsKey (lType.FullName) currentMap then
                currentMap.[lType.FullName]
            else
               let builder = typeof<CachingReflectiveListBuilder>
                                .GetMethod("ReturnTypedListBuilder")
                                .MakeGenericMethod([|lType|])
                                .Invoke(null, null)
                                :?> obj list -> obj
               CachingReflectiveListBuilder.builderMap := Map.add lType.FullName builder currentMap
               builder

    let isOption (t:Type) =
        t.IsGenericType &&
        t.GetGenericTypeDefinition() = typedefof<Option<_>>

    let isList (t:Type) =
        t.IsGenericType &&
        t.GetGenericTypeDefinition() = typedefof<List<_>>

    let convertFromArrayObject fnc array  =
        let rec helper lst index fnc (array: ArrayObject)  =
            if array.Count > index then
                let result = fnc index array
                match result with
                | Ok value ->
                    let out = value::lst
                    helper out (index+1) fnc array
                | Error error ->
                    Error error
            else
                Ok lst
        helper [] 0 fnc array

    let makeOptionValue typey v isSome =
        let createOptionType typeParam =
            typeof<unit option>.GetGenericTypeDefinition().MakeGenericType([| typeParam |])

        let optionType = createOptionType typey
        let cases = FSharpType.GetUnionCases(optionType)
        let cases = cases |> Array.partition (fun x -> x.Name = "Some")
        let someCase = fst cases |> Array.exactlyOne
        let noneCase = snd cases |> Array.exactlyOne
        let relevantCase, args =
            match isSome with
            | true -> someCase, [| v |]
            | false -> noneCase, [| |]
        FSharpValue.MakeUnion(relevantCase, args)
