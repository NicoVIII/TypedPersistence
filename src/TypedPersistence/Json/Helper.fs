namespace TypedPersistence.Json

open FSharp.Json

module Helper =
    type OptionBuilder() =
        member _.Bind(v, f) = Option.bind f v
        member _.Return v = Some v
        member _.ReturnFrom o = o
        member _.Zero() = None

    let opt = OptionBuilder()

    let deserializeJson<'a> content =
        try
            Json.deserialize<'a> content |> Some
        with :? JsonDeserializationError ->
            None
