namespace TypedPersistence.Json

open TypedPersistence.Core
open TypedPersistence.Json.Types

module Api =
    let fsharpProvider =
        { new IPersistenceProvider<Context> with
            member _.getVersion context = getVersion context
            member _.load<'a> context = load<'a> context
            member _.save<'a> context object = save<'a> context object

            member _.saveVersion<'a> context version object =
                saveVersion<'a> context version object }

    let csharpProvider = Provider.fsharpToCsharp fsharpProvider
