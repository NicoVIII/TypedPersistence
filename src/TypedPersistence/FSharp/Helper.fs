namespace TypedPersistence.FSharp

open FSharp.Reflection
open LiteDB
open System

module Helpers =
    // Taken from: http://www.fssnip.net/1L/title/Convert-a-obj-list-to-a-typed-list-without-generics
    type ReflectiveListBuilder =

        static member BuildList<'a>(args: obj list) =
            [ for a in args do
                yield a :?> 'a ]

        static member BuildTypedList lType (args: obj list) =
            typeof<ReflectiveListBuilder>.GetMethod("BuildList").MakeGenericMethod([| lType |]).Invoke(null, [| args |])

    type CachingReflectiveListBuilder =

        static member ReturnTypedListBuilder<'a>(): obj list -> obj =
            let createList (args: obj list) =
                [ for a in args do
                    yield a :?> 'a ] :> obj
            createList

        static member private builderMap = ref Map.empty<string, obj list -> obj>
        static member BuildTypedList(lType: System.Type) =
            let currentMap = !CachingReflectiveListBuilder.builderMap
            if Map.containsKey (lType.FullName) currentMap then
                currentMap.[lType.FullName]
            else
                let builder =
                    typeof<CachingReflectiveListBuilder>.GetMethod("ReturnTypedListBuilder")
                        .MakeGenericMethod([| lType |]).Invoke(null, null) :?> obj list -> obj
                CachingReflectiveListBuilder.builderMap := Map.add lType.FullName builder currentMap
                builder

    let inline isNull (x:^T when ^T : not struct) = obj.ReferenceEquals (x, null)

    let isOption (t: Type) = t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Option<_>>

    let isList (t: Type) = t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<List<_>>

    let makeOptionValue typey v isSome =
        let createOptionType typeParam = typeof<unit option>.GetGenericTypeDefinition().MakeGenericType([| typeParam |])

        let optionType = createOptionType typey
        let cases = FSharpType.GetUnionCases(optionType)
        let cases = cases |> Array.partition (fun x -> x.Name = "Some")
        let someCase = fst cases |> Array.exactlyOne
        let noneCase = snd cases |> Array.exactlyOne

        let relevantCase, args =
            match isSome with
            | true -> someCase, [| v |]
            | false -> noneCase, [||]
        FSharpValue.MakeUnion(relevantCase, args)

    let executeWithDatabaseSetup executeWithDatabase (path: string) =
        use db = new LiteDatabase(path, FSharpBsonMapperWithGenerics())
        let result = executeWithDatabase db
        db.Dispose()
        result
